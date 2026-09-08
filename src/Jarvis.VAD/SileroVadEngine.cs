using System.Diagnostics;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace Jarvis.VAD;

public sealed record VadFrameResult(float SpeechProbability, double InferenceMs)
{
    public bool IsSpeech(float threshold = 0.5f) => SpeechProbability >= threshold;
}

public sealed class SileroVadEngine : IDisposable
{
    public const int SampleRate = 16000;
    public const int FrameSamples = 512;
    public const int ContextSamples = 64;

    private readonly InferenceSession _session;
    private float[] _state = new float[2 * 1 * 128];
    private float[] _context = new float[ContextSamples];
    private bool _disposed;

    public SileroVadEngine(string modelPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(modelPath);
        _session = new InferenceSession(modelPath);
    }
    public VadFrameResult Process(ReadOnlySpan<short> pcm16)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (pcm16.Length != FrameSamples)
            throw new ArgumentException($"Silero VAD requires {FrameSamples} samples per frame.", nameof(pcm16));

        var inputData = new float[ContextSamples + FrameSamples];
        _context.CopyTo(inputData, 0);
        for (var i = 0; i < pcm16.Length; i++)
            inputData[ContextSamples + i] = pcm16[i] / 32768f;

        var input = new DenseTensor<float>(inputData, new[] { 1, inputData.Length });
        var state = new DenseTensor<float>(_state, new[] { 2, 1, 128 });
        var sr = new DenseTensor<long>(new[] { (long)SampleRate }, Array.Empty<int>());

        var inputs = new[]
        {
            NamedOnnxValue.CreateFromTensor("input", input),
            NamedOnnxValue.CreateFromTensor("state", state),
            NamedOnnxValue.CreateFromTensor("sr", sr)
        };
        var sw = Stopwatch.StartNew();
        using var results = _session.Run(inputs);
        sw.Stop();

        var probability = results.First(x => x.Name == "output").AsTensor<float>().ToArray()[0];
        var nextState = results.First(x => x.Name == "stateN").AsTensor<float>().ToArray();
        if (nextState.Length != _state.Length)
            throw new InvalidOperationException($"Unexpected Silero state length: {nextState.Length}.");

        _state = nextState;
        Array.Copy(inputData, inputData.Length - ContextSamples, _context, 0, ContextSamples);
        return new VadFrameResult(probability, sw.Elapsed.TotalMilliseconds);
    }

    public void Reset()
    {
        Array.Clear(_state);
        Array.Clear(_context);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _session.Dispose();
        _disposed = true;
    }
}
