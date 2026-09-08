using Jarvis.Audio;
using Jarvis.VAD;
using System.Diagnostics;

var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
var model = Path.Combine(root, "models", "silero-vad", "silero_vad.onnx");
const string norTecId = "{0.0.1.00000000}.{e40fea3e-eb20-4abb-93c2-ae05d304560b}";

using var vad = new SileroVadEngine(model);
var silence = new short[SileroVadEngine.FrameSamples];
var times = new List<double>(1000);

for (var i = 0; i < 50; i++) vad.Process(silence);
vad.Reset();
for (var i = 0; i < 1000; i++) times.Add(vad.Process(silence).InferenceMs);

times.Sort();
static double P(IReadOnlyList<double> x, double q) => x[(int)Math.Min(x.Count - 1, Math.Ceiling(q * x.Count) - 1)];
Console.WriteLine($"CPU inference 1000 frames: p50={P(times,0.50):F3}ms p90={P(times,0.90):F3}ms p95={P(times,0.95):F3}ms p99={P(times,0.99):F3}ms max={times[^1]:F3}ms");

var captured = new List<byte[]>();
using var capture = new WasapiCaptureSession(norTecId, 2_097_152);
capture.ChunkAvailable += (_, e) => captured.Add(e.Data.ToArray());
Console.WriteLine($"Capturing Nor-Tec for 10 seconds: {capture.Format}");
await capture.CaptureForAsync(TimeSpan.FromSeconds(10));
var allBytes = captured.SelectMany(x => x).ToArray();
var normalized = Pcm16MonoNormalizer.NormalizeFloat32(allBytes, capture.Format.SampleRate, capture.Format.Channels);

vad.Reset();
var probs = new List<float>();
var liveTimes = new List<double>();
for (var offset = 0; offset + SileroVadEngine.FrameSamples <= normalized.Length; offset += SileroVadEngine.FrameSamples)
{
    var result = vad.Process(normalized.AsSpan(offset, SileroVadEngine.FrameSamples));
    probs.Add(result.SpeechProbability);
    liveTimes.Add(result.InferenceMs);
}

var speechFrames = probs.Count(x => x >= 0.5f);
var maxProb = probs.Count == 0 ? 0 : probs.Max();
var meanProb = probs.Count == 0 ? 0 : probs.Average();
liveTimes.Sort();
Console.WriteLine($"Live frames={probs.Count} speech>=0.5={speechFrames} rate={(probs.Count==0?0:(double)speechFrames/probs.Count):P2}");
Console.WriteLine($"Probability mean={meanProb:F4} max={maxProb:F4}");
if (liveTimes.Count > 0)
    Console.WriteLine($"Live inference p50={P(liveTimes,0.50):F3}ms p95={P(liveTimes,0.95):F3}ms p99={P(liveTimes,0.99):F3}ms max={liveTimes[^1]:F3}ms");

