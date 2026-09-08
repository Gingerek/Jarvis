using Jarvis.Audio;
using NAudio.Wave;

const string norTecId = "{0.0.1.00000000}.{e40fea3e-eb20-4abb-93c2-ae05d304560b}";
var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
var outDir = Path.Combine(root, "benchmarks", "asr");
Directory.CreateDirectory(outDir);
var outPath = Path.Combine(outDir, "polish-command-sample.wav");

using var capture = new WasapiCaptureSession(norTecId, 2_097_152);
using var writer = new WaveFileWriter(outPath, new WaveFormat(16000, 16, 1));

capture.ChunkAvailable += (_, chunk) =>
{
    var pcm = Pcm16MonoNormalizer.NormalizeFloat32(
        chunk.Data.Span,
        capture.Format.SampleRate,
        capture.Format.Channels);
    var bytes = new byte[pcm.Length * sizeof(short)];
    Buffer.BlockCopy(pcm, 0, bytes, 0, bytes.Length);
    writer.Write(bytes, 0, bytes.Length);
};
Console.WriteLine("Recording 12 seconds from Nor-Tec...");
capture.Start();
await Task.Delay(TimeSpan.FromSeconds(12));
var metrics = capture.Stop();
writer.Flush();

Console.WriteLine($"Saved: {outPath}");
Console.WriteLine($"Input: {capture.Format}");
Console.WriteLine($"Peak={metrics.Peak:F6} RMS={metrics.Rms:F6}");
Console.WriteLine($"First={metrics.FirstFrameLatencyMs:F1}ms Jitter={metrics.MaxCallbackJitterMs:F1}ms");
