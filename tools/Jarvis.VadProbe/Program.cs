using Jarvis.Audio;
using Jarvis.VAD;
using System.Diagnostics;

var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
var model = Path.Combine(root, "models", "silero-vad", "silero_vad.onnx");
const string norTecId = "{0.0.1.00000000}.{e40fea3e-eb20-4abb-93c2-ae05d304560b}";

if (!File.Exists(model))
{
    Console.Error.WriteLine($"Model not found: {model}");
    return 2;
}

if (args.Contains("--live", StringComparer.OrdinalIgnoreCase))
{
    await RunLiveAsync(model, norTecId);
    return 0;
}

RunCpuBenchmark(model);
return 0;
static void RunCpuBenchmark(string model)
{
    using var vad = new SileroVadEngine(model);
    var silence = new short[SileroVadEngine.FrameSamples];
    var times = new List<double>(1000);
    for (var i = 0; i < 50; i++) vad.Process(silence);
    vad.Reset();
    for (var i = 0; i < 1000; i++) times.Add(vad.Process(silence).InferenceMs);
    times.Sort();
    Console.WriteLine($"CPU inference 1000 frames: p50={P(times,0.50):F3}ms p95={P(times,0.95):F3}ms p99={P(times,0.99):F3}ms max={times[^1]:F3}ms");
}

static double P(List<double> sorted, double p)
{
    var index = (int)Math.Ceiling(p * sorted.Count) - 1;
    return sorted[Math.Clamp(index, 0, sorted.Count - 1)];
}
static async Task RunLiveAsync(string model, string deviceId)
{
    using var vad = new SileroVadEngine(model);
    var gate = new VadSpeechGate();
    using var capture = new WasapiCaptureSession(deviceId, 2_097_152);

    var pending = new List<short>(SileroVadEngine.FrameSamples * 4);
    var infer = new List<double>();
    var probs = new List<float>();
    var transitions = new List<(VadTransition Kind, long Ms)>();
    var clock = Stopwatch.StartNew();

    capture.ChunkAvailable += (_, chunk) =>
    {
        var mono = Pcm16MonoNormalizer.NormalizeFloat32(
            chunk.Data.Span,
            capture.Format.SampleRate,
            capture.Format.Channels);
        pending.AddRange(mono);
        while (pending.Count >= SileroVadEngine.FrameSamples)
        {
            var frame = pending.GetRange(0, SileroVadEngine.FrameSamples).ToArray();
            pending.RemoveRange(0, SileroVadEngine.FrameSamples);
            var result = vad.Process(frame);
            infer.Add(result.InferenceMs);
            probs.Add(result.SpeechProbability);
            var transition = gate.Push(result.SpeechProbability);
            if (transition != VadTransition.None) transitions.Add((transition, clock.ElapsedMilliseconds));
        }
    };
    Console.WriteLine("Live VAD benchmark: speak naturally for ~10 seconds, then stay silent.");
    capture.Start();
    await Task.Delay(TimeSpan.FromSeconds(15));
    var captureMetrics = capture.Stop();

    infer.Sort();
    var maxProb = probs.Count == 0 ? 0 : probs.Max();
    var meanProb = probs.Count == 0 ? 0 : probs.Average();
    Console.WriteLine($"Frames={infer.Count}, maxProb={maxProb:F3}, meanProb={meanProb:F3}");
    if (infer.Count > 0)
        Console.WriteLine($"Inference: p50={P(infer,0.50):F3}ms p95={P(infer,0.95):F3}ms p99={P(infer,0.99):F3}ms max={infer[^1]:F3}ms");
    Console.WriteLine($"Capture first={captureMetrics.FirstFrameLatencyMs:F1}ms jitter={captureMetrics.MaxCallbackJitterMs:F1}ms");
    if (transitions.Count == 0) Console.WriteLine("Transitions: none");
    else foreach (var item in transitions) Console.WriteLine($"Transition {item.Kind} at {item.Ms} ms");
}
