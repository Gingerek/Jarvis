using NAudio.Wave;

namespace Jarvis.Speech;

public sealed class SpeechOutputManager : IAsyncDisposable
{
    private readonly ITtsProvider _provider;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private CancellationTokenSource? _activeSpeech;
    private bool _disposed;

    public SpeechOutputManager(ITtsProvider provider)
    {
        _provider = provider;
    }

    public bool IsSpeaking { get; private set; }

    public async Task SpeakAsync(
        string text,
        CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        await _gate.WaitAsync(cancellationToken);
        try
        {
            _activeSpeech = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            await PlayStreamAsync(text, _activeSpeech.Token);
        }
        finally
        {
            _activeSpeech?.Dispose();
            _activeSpeech = null;
            IsSpeaking = false;
            _gate.Release();
        }
    }

    private async Task PlayStreamAsync(string text, CancellationToken cancellationToken)
    {
        var format = new WaveFormat(24000, 16, 1);
        var buffer = new BufferedWaveProvider(format)
        {
            BufferDuration = TimeSpan.FromSeconds(8),
            DiscardOnBufferOverflow = false
        };
        using var output = new WaveOutEvent();
        output.Init(buffer);
        output.Play();
        IsSpeaking = true;

        await foreach (var chunk in _provider.StreamAsync(text, cancellationToken))
        {
            var bytes = chunk.ToArray();
            buffer.AddSamples(bytes, 0, bytes.Length);
        }

        while (buffer.BufferedBytes > 0 && !cancellationToken.IsCancellationRequested)
            await Task.Delay(20, cancellationToken);
    }

    public void Interrupt()
    {
        _activeSpeech?.Cancel();
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;
        Interrupt();
        await _provider.DisposeAsync();
        _gate.Dispose();
    }
}
