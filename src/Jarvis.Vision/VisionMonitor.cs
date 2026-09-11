namespace Jarvis.Vision;

public sealed record VisionFrameSnapshot(byte[] Jpeg, string? CameraName, DateTimeOffset CapturedAt);

public sealed class VisionMonitor : IAsyncDisposable
{
    private readonly WindowsCameraCapture _camera;
    private readonly TimeSpan _interval;
    private readonly object _sync = new();
    private readonly CancellationTokenSource _cts = new();
    private Task? _loop;
    private VisionFrameSnapshot? _latest;

    public VisionMonitor(WindowsCameraCapture camera, TimeSpan? interval = null)
    {
        _camera = camera;
        _interval = interval ?? TimeSpan.FromSeconds(2);
    }

    public VisionFrameSnapshot? Latest
    {
        get { lock (_sync) return _latest; }
    }

    public void Start()
    {
        _loop ??= Task.Run(() => LoopAsync(_cts.Token));
    }
    private async Task LoopAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var frame = await _camera.CaptureJpegAsync();
            if (frame.Success && frame.Jpeg is not null)
            {
                lock (_sync)
                    _latest = new(frame.Jpeg, frame.CameraName, DateTimeOffset.UtcNow);
            }
            try { await Task.Delay(_interval, cancellationToken); }
            catch (OperationCanceledException) { break; }
        }
    }

    public async ValueTask DisposeAsync()
    {
        _cts.Cancel();
        if (_loop is not null)
        {
            try { await _loop; } catch (OperationCanceledException) { }
        }
        _cts.Dispose();
    }
}
