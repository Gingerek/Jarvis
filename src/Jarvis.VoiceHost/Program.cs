using System.Text.Json;
using System.Threading.Channels;
using Jarvis.Audio;
using Jarvis.ASR;
using Jarvis.Brain;
using Jarvis.Commands;
using Jarvis.Execution;
using Jarvis.Intents;
using Jarvis.Security;
using Jarvis.Speech;
using Jarvis.VAD;
using Jarvis.VoiceHost;
using Jarvis.WakeWord;

string? Option(string name)
{
    var index = Array.FindIndex(args, x => x.Equals(name, StringComparison.OrdinalIgnoreCase));
    return index >= 0 && index + 1 < args.Length ? args[index + 1] : null;
}

var serviceMode = args.Contains("--service", StringComparer.OrdinalIgnoreCase);
var root = Option("--root") ?? Directory.GetCurrentDirectory();
var pipeName = Option("--pipe");
Directory.SetCurrentDirectory(root);

var parser = new OpenApplicationCommandParser();
var resolver = new KnownEntityResolver();
var launcher = new ApplicationLauncher();
foreach (var app in KnownApplications.All)
    resolver.Add(app.Id, app.Aliases.Append(app.DisplayName).ToArray());
ApplicationLaunchResult? ExecuteCommand(string text)
{
    var parsed = parser.Parse(text);
    if (parsed is null) return null;
    var match = resolver.Resolve(parsed.RequestedName, 0.55);
    if (match is null) return null;

    var target = KnownApplications.All.First(x => x.Id == match.CanonicalName);
    var result = launcher.Launch(target);
    Console.WriteLine($"EXECUTE: {target.DisplayName} -> {result.Status}");
    return result;
}

if (args.Length > 1 && args[0].Equals("--command", StringComparison.OrdinalIgnoreCase))
{
    var commandText = string.Join(' ', args.Skip(1));
    var result = ExecuteCommand(commandText);
    if (result is null) Console.WriteLine($"UNHANDLED COMMAND: {commandText}");
    return;
}

using var lifetimeCts = serviceMode
    ? new CancellationTokenSource()
    : new CancellationTokenSource(TimeSpan.FromSeconds(45));

Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    lifetimeCts.Cancel();
};

await using var events = await HostEventSink.CreateAsync(pipeName, lifetimeCts.Token);
await events.EmitAsync("state", "STARTING", "Uruchamiam moduły głosowe");
var settingsPath = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
    "Jarvis", "config", "settings.json");
await using var settingsStream = File.OpenRead(settingsPath);
using var settings = await JsonDocument.ParseAsync(settingsStream, cancellationToken: lifetimeCts.Token);

var deviceId = settings.RootElement.GetProperty("AudioInputDeviceId").GetString()
    ?? throw new InvalidOperationException("AudioInputDeviceId is not configured.");
var deviceName = settings.RootElement.GetProperty("AudioInputDeviceName").GetString()
    ?? "Configured microphone";
// Speech is optional: local command execution must also work without cloud credentials.
var ttsVoiceId = settings.RootElement.TryGetProperty("TtsVoiceId", out var voiceSetting)
    ? voiceSetting.GetString() : null;
var ttsModelId = settings.RootElement.TryGetProperty("TtsModelId", out var modelSetting)
    ? modelSetting.GetString() ?? "eleven_flash_v2_5" : "eleven_flash_v2_5";
string? ttsApiKey = null;
try { ttsApiKey = new WindowsCredentialSecretStore().Read("ElevenLabsApiKey"); }
catch (Exception)
{
    await events.EmitAsync("warning", text: "Głos niedostępny. Polecenia lokalne nadal działają.");
}

var python = Path.Combine(root, ".venv-asr", "Scripts", "python.exe");
var worker = Path.Combine(root, "tools", "asr_worker.py");
var models = Path.Combine(root, ".models-asr");
var vadModel = Path.Combine(root, "models", "silero-vad", "silero_vad.onnx");

await events.EmitAsync("state", "STARTING", $"Mikrofon: {deviceName}");
await using var asr = await PythonFasterWhisperAsrEngine.CreateAsync(
    python, worker, models, "base", lifetimeCts.Token);
using var vad = new SileroVadEngine(vadModel);
var vadGate = new VadSpeechGate(
    startThreshold: 0.40f,
    endThreshold: 0.25f,
    minimumSpeechMs: 96,
    minimumSilenceMs: 480);
var segmenter = new SpeechSegmenter(vad, vadGate, preRollMs: 480, maxSpeechMs: 6000);
var detector = new TranscriptWakeWordDetector("Jarvis");
var session = new VoiceSessionStateMachine(TimeSpan.FromSeconds(20));
var voice = new VoiceInteractionCoordinator(detector, session);
var pipeline = new VoicePipelineProcessor(asr, voice);
await using var speech = !string.IsNullOrWhiteSpace(ttsApiKey) && !string.IsNullOrWhiteSpace(ttsVoiceId)
    ? new SpeechOutputManager(new ElevenLabsTtsProvider(ttsApiKey, ttsVoiceId, ttsModelId))
    : null;
if (speech is null)
    await events.EmitAsync("warning", text: "Głos nieskonfigurowany. Odpowiedzi będą wyświetlane tekstowo.");

using var capture = new WasapiCaptureSession(deviceId);
var format = capture.Format;
if (format.BitsPerSample != 32)
    throw new NotSupportedException($"VoiceHost expects float32 capture, got {format}.");

var audio = Channel.CreateBounded<short[]>(new BoundedChannelOptions(256)
{
    FullMode = BoundedChannelFullMode.DropOldest,
    SingleReader = true,
    SingleWriter = true
});

capture.ChunkAvailable += (_, chunk) =>
{
    var normalized = Pcm16MonoNormalizer.NormalizeFloat32(
        chunk.Data.Span, format.SampleRate, format.Channels);
    audio.Writer.TryWrite(normalized);
};

capture.Start();
await events.EmitAsync("state", "SLEEPING", $"Nasłuch: {deviceName}");
try
{
    await foreach (var chunk in audio.Reader.ReadAllAsync(lifetimeCts.Token))
    {
        var tick = voice.Tick(DateTimeOffset.UtcNow);
        if (tick.Disposition == VoiceInputDisposition.SessionTimedOut)
            await events.EmitAsync("state", "SLEEPING", "Sesja wygasła");

        var segment = segmenter.Push(chunk);
        if (segment is null) continue;

        await events.EmitAsync("state", "PROCESSING", "Rozpoznaję mowę");
        var result = await pipeline.ProcessSpeechAsync(
            segment.Samples, DateTimeOffset.UtcNow, lifetimeCts.Token);
        await events.EmitAsync("transcript", text: result.Transcript,
            data: new { result.AsrInferenceMs, disposition = result.Disposition.ToString() });

        if (result.Disposition == VoiceInputDisposition.IgnoredWhileSleeping)
        {
            await events.EmitAsync("state", "SLEEPING", "Czekam na Jarvis");
            continue;
        }

        if (result.Disposition == VoiceInputDisposition.WakeDetected &&
            string.IsNullOrWhiteSpace(result.CommandText))
        {
            await events.EmitAsync("state", "LISTENING", "Słucham");
            continue;
        }

        if (string.IsNullOrWhiteSpace(result.CommandText)) continue;
        await events.EmitAsync("command", "PROCESSING", result.CommandText);
        var execution = ExecuteCommand(result.CommandText);
        if (execution is null)
        {
            await events.EmitAsync("state", "LISTENING", "Nie znam jeszcze tej komendy");
            continue;
        }

        var reply = execution.Status switch
        {
            ApplicationLaunchStatus.Started => $"Otwieram {execution.Target.DisplayName}.",
            ApplicationLaunchStatus.AlreadyRunning => $"{execution.Target.DisplayName} jest już otwarty.",
            ApplicationLaunchStatus.ExecutableNotFound => $"Nie znalazłem programu {execution.Target.DisplayName}.",
            _ => $"Nie udało się uruchomić {execution.Target.DisplayName}."
        };

        await events.EmitAsync("execution", text: reply,
            data: new { status = execution.Status.ToString(), target = execution.Target.DisplayName });
        if (speech is not null)
        {
            await events.EmitAsync("state", "SPEAKING", reply);
            try
            {
                capture.Stop();
                segmenter.Reset();
                var spoken = await SpeechAttempt.TryAsync(
                    token => speech.SpeakAsync(reply, token), lifetimeCts.Token);
                if (!spoken)
                    await events.EmitAsync("warning", text: "Nie udało się odtworzyć głosu. Wynik polecenia jest widoczny w oknie.");
            }
            finally
            {
                if (!lifetimeCts.IsCancellationRequested) capture.Start();
            }
        }

        await events.EmitAsync("state", "LISTENING", "Słucham");
    }
}
catch (OperationCanceledException) when (lifetimeCts.IsCancellationRequested)
{
    // Normal shutdown.
}
catch (Exception ex)
{
    await events.EmitAsync("error", "ERROR", ex.Message);
    throw;
}
finally
{
    try { capture.Stop(); } catch { }
    await events.EmitAsync("state", "STOPPED", "Jarvis zatrzymany");
}
