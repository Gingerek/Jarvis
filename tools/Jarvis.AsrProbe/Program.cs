using Jarvis.ASR;
using NAudio.Wave;

var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
var wav = Path.Combine(root, "benchmarks", "asr", "polish-reference-tts.wav");
var python = Path.Combine(root, ".venv-asr", "Scripts", "python.exe");
var worker = Path.Combine(root, "tools", "asr_worker.py");
var models = Path.Combine(root, ".models-asr");

using var reader = new WaveFileReader(wav);
if (reader.WaveFormat.SampleRate != 16000 || reader.WaveFormat.Channels != 1 || reader.WaveFormat.BitsPerSample != 16)
    throw new InvalidOperationException($"Unexpected WAV format: {reader.WaveFormat}");

var bytes = new byte[reader.Length];
var read = reader.Read(bytes, 0, bytes.Length);
var pcm = new short[read / 2];
Buffer.BlockCopy(bytes, 0, pcm, 0, read);

await using var engine = await PythonFasterWhisperAsrEngine.CreateAsync(python, worker, models, "base");
Console.WriteLine($"Worker ready: {engine.ModelName}");
for (var i = 1; i <= 3; i++)
{
    var result = await engine.TranscribeAsync(pcm);
    Console.WriteLine($"run{i}: {result.InferenceMs:F1} ms | {result.Text}");
}
