using System.Diagnostics;
using System.Globalization;
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

var registry = new CommandRegistry();
var resolver = new KnownEntityResolver();
var launcher = new ApplicationLauncher();
var windows = new WindowsSystemController();
var folders = new KnownFolderExecutor(root);
foreach (var app in KnownApplications.All)
    resolver.Add(app.Id, app.Aliases.Append(app.DisplayName).ToArray());

CommandExecutionOutcome? ExecuteCommand(string text)
{
    var request = registry.Parse(text);
    if (request is null) return null;

    if (request.Intent is CommandIntent.OpenApplication or CommandIntent.CloseApplication)
    {
        var match = resolver.Resolve(request.Argument ?? string.Empty, 0.55);
        if (match is null) return null;
        var target = KnownApplications.All.First(x => x.Id == match.CanonicalName);
        if (request.Intent == CommandIntent.OpenApplication)
        {
            var result = launcher.Launch(target);
            return result.Status switch
            {
                ApplicationLaunchStatus.Started => new($"Otwieram {target.DisplayName}.", result.Status.ToString(), target.DisplayName),
                ApplicationLaunchStatus.AlreadyRunning => new($"{target.DisplayName} jest już otwarty.", result.Status.ToString(), target.DisplayName),
                ApplicationLaunchStatus.ExecutableNotFound => new($"Nie znalazłem programu {target.DisplayName}.", result.Status.ToString(), target.DisplayName),
                _ => new($"Nie udało się uruchomić {target.DisplayName}.", result.Status.ToString(), target.DisplayName)
            };
        }

        var close = launcher.Close(target);
        return close.Status switch
        {
            ApplicationCloseStatus.CloseRequested => new($"Zamykam {target.DisplayName}.", close.Status.ToString(), target.DisplayName),
            ApplicationCloseStatus.NotRunning => new($"{target.DisplayName} nie jest uruchomiony.", close.Status.ToString(), target.DisplayName),
            _ => new($"Nie udało się zamknąć {target.DisplayName}.", close.Status.ToString(), target.DisplayName)
        };
    }

    return ExecuteBuiltIn(request);
}

CommandExecutionOutcome? ExecuteBuiltIn(CommandRequest request)
{
    var pl = CultureInfo.GetCultureInfo("pl-PL");
    switch (request.Intent)
    {
        case CommandIntent.OpenFolder:
            var folder = folders.Open(request.Argument!);
            return new(folder.Message, folder.Success ? "OpenFolder" : "OpenFolderFailed", folder.DisplayName);
        case CommandIntent.OpenSettings:
            OpenUrl(request.Argument!);
            return new("Otwieram ustawienia Windows.", "OpenSettings", request.Argument);
        case CommandIntent.OpenWebsite:
            OpenUrl(request.Argument!);
            var site = request.Argument!.Contains("youtube", StringComparison.OrdinalIgnoreCase) ? "YouTube" :
                request.Argument.Contains("marktplaats", StringComparison.OrdinalIgnoreCase) ? "Marktplaats" : "Google";
            return new($"Otwieram {site}.", "OpenedWebsite", site);
        case CommandIntent.SearchWeb:
            OpenUrl($"https://www.google.com/search?q={Uri.EscapeDataString(request.Argument!)}");
            return new($"Szukam w Google: {request.Argument}.", "SearchWeb", "Google");
        case CommandIntent.SearchYouTube:
            OpenUrl($"https://www.youtube.com/results?search_query={Uri.EscapeDataString(request.Argument!)}");
            return new($"Szukam na YouTube: {request.Argument}.", "SearchYouTube", "YouTube");
        case CommandIntent.GetTime:
            return new($"Jest {DateTime.Now.ToString("HH:mm", pl)}.", "GetTime");
        case CommandIntent.GetDate:
            return new($"Dzisiaj jest {DateTime.Now.ToString("d MMMM yyyy", pl)}.", "GetDate");
        case CommandIntent.GetDayOfWeek:
            return new($"Dzisiaj jest {DateTime.Now.ToString("dddd", pl)}.", "GetDayOfWeek");
        case CommandIntent.VolumeUp:
            return SystemOutcome(windows.ChangeVolume(10), "VolumeUp");
        case CommandIntent.VolumeDown:
            return SystemOutcome(windows.ChangeVolume(-10), "VolumeDown");
        case CommandIntent.SetVolume:
            return SystemOutcome(windows.SetVolumePercent(int.Parse(request.Argument!)), "SetVolume");
        case CommandIntent.Mute:
            return SystemOutcome(windows.SetMute(true), "Mute");
        case CommandIntent.Unmute:
            return SystemOutcome(windows.SetMute(false), "Unmute");
        case CommandIntent.MinimizeWindow:
            return SystemOutcome(windows.MinimizeForegroundWindow(), "MinimizeWindow");
        case CommandIntent.MaximizeWindow:
            return SystemOutcome(windows.MaximizeForegroundWindow(), "MaximizeWindow");
        case CommandIntent.RestoreWindow:
            return SystemOutcome(windows.RestoreForegroundWindow(), "RestoreWindow");
        case CommandIntent.CloseWindow:
            return SystemOutcome(windows.CloseForegroundWindow(), "CloseWindow");
        case CommandIntent.ShowDesktop:
            return SystemOutcome(windows.ShowDesktop(), "ShowDesktop");
        case CommandIntent.LockComputer:
            return SystemOutcome(windows.LockComputer(), "LockComputer");
        default:
            return null;
    }
}

CommandExecutionOutcome SystemOutcome(SystemActionResult result, string status) =>
    new(result.Message, result.Success ? status : status + "Failed");

void OpenUrl(string url)
{
    Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
}

if (args.Length > 1 && args[0].Equals("--command", StringComparison.OrdinalIgnoreCase))
{
    var commandText = string.Join(' ', args.Skip(1));
    var result = ExecuteCommand(commandText);
    if (result is null) Console.WriteLine($"UNHANDLED COMMAND: {commandText}");
    else Console.WriteLine($"EXECUTE: {result.Status} -> {result.Reply}");
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
var ttsVoiceId = settings.RootElement.GetProperty("TtsVoiceId").GetString()
    ?? throw new InvalidOperationException("TtsVoiceId is not configured.");
var ttsModelId = settings.RootElement.GetProperty("TtsModelId").GetString()
    ?? "eleven_flash_v2_5";
var ttsApiKey = new WindowsCredentialSecretStore().Read("ElevenLabsApiKey")
    ?? throw new InvalidOperationException("ElevenLabs API key is not configured.");

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
var ttsProvider = new ElevenLabsTtsProvider(ttsApiKey, ttsVoiceId, ttsModelId);
await using var speech = new SpeechOutputManager(ttsProvider);

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
            const string fallbackReply = "Nie zrozumiałem polecenia. Powiedz na przykład: otwórz Lightroom.";
            await events.EmitAsync("execution", text: fallbackReply,
                data: new { status = "Unhandled", command = result.CommandText });
            await events.EmitAsync("state", "SPEAKING", fallbackReply);
            try
            {
                capture.Stop();
                segmenter.Reset();
                await speech.SpeakAsync(fallbackReply, lifetimeCts.Token);
            }
            finally
            {
                if (!lifetimeCts.IsCancellationRequested) capture.Start();
            }
            await events.EmitAsync("state", "LISTENING", "Słucham");
            continue;
        }

        var reply = execution.Reply;
        await events.EmitAsync("execution", text: reply,
            data: new { status = execution.Status, target = execution.Target });
        await events.EmitAsync("state", "SPEAKING", reply);

        try
        {
            capture.Stop();
            segmenter.Reset();
            await speech.SpeakAsync(reply, lifetimeCts.Token);
        }
        finally
        {
            if (!lifetimeCts.IsCancellationRequested) capture.Start();
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
