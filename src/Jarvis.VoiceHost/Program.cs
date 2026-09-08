using System.Text.Json;
using System.Threading.Channels;
using Jarvis.Audio;
using Jarvis.ASR;
using Jarvis.Brain;
using Jarvis.VAD;
using Jarvis.WakeWord;

var root = Directory.GetCurrentDirectory();
var settingsPath = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
    "Jarvis", "config", "settings.json");

await using var settingsStream = File.OpenRead(settingsPath);
using var settings = await JsonDocument.ParseAsync(settingsStream);
var deviceId = settings.RootElement.GetProperty("AudioInputDeviceId").GetString()
    ?? throw new InvalidOperationException("AudioInputDeviceId is not configured.");
var deviceName = settings.RootElement.GetProperty("AudioInputDeviceName").GetString();

var python = Path.Combine(root, ".venv-asr", "Scripts", "python.exe");
var worker = Path.Combine(root, "tools", "asr_worker.py");
var models = Path.Combine(root, ".models-asr");
var vadModel = Path.Combine(root, "models", "silero-vad", "silero_vad.onnx");

Console.WriteLine($"Input: {deviceName}");
Console.WriteLine("Loading ASR worker...");await using var asr = await PythonFasterWhisperAsrEngine.CreateAsync(
    python, worker, models, "base");
using var vad = new SileroVadEngine(vadModel);
var segmenter = new SpeechSegmenter(vad, maxSpeechMs: 6000);
var detector = new TranscriptWakeWordDetector("Jarvis");
var session = new VoiceSessionStateMachine(TimeSpan.FromSeconds(20));
var voice = new VoiceInteractionCoordinator(detector, session);
var pipeline = new VoicePipelineProcessor(asr, voice);

using var capture = new WasapiCaptureSession(deviceId);
var format = capture.Format;
if (format.BitsPerSample != 32)
    throw new NotSupportedException($"Live probe expects float32 capture, got {format}.");

long chunkCount = 0;
double rawPeak = 0;
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
    Interlocked.Increment(ref chunkCount);
    foreach (var sample in normalized)
        rawPeak = Math.Max(rawPeak, Math.Abs(sample / 32768.0));
    audio.Writer.TryWrite(normalized);
};

using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
Console.WriteLine("LIVE READY. Say: Jarvis, otworz Lightroom");
Console.WriteLine("Then say another command without Jarvis.");
capture.Start();

try
{
    await foreach (var chunk in audio.Reader.ReadAllAsync(cts.Token))
    {
        var segment = segmenter.Push(chunk);
        if (segment is null) continue;

        Console.WriteLine($"Speech segment: {segment.Duration.TotalSeconds:F2}s, VAD peak={segment.PeakProbability:F3}");
        var result = await pipeline.ProcessSpeechAsync(
            segment.Samples, DateTimeOffset.UtcNow, cts.Token);
        Console.WriteLine($"ASR {result.AsrInferenceMs:F0}ms: {result.Transcript}");
        Console.WriteLine($"Disposition: {result.Disposition}");
        if (!string.IsNullOrWhiteSpace(result.CommandText))
            Console.WriteLine($"COMMAND: {result.CommandText}");
    }
}catch (OperationCanceledException) when (cts.IsCancellationRequested)
{
    Console.WriteLine("Live probe finished.");
}
finally
{
    try { capture.Stop(); } catch { }
    Console.WriteLine($"Audio chunks={chunkCount}, rawPeak={rawPeak:F6}");
}

