namespace Jarvis.Vision;

public enum VisionTask
{
    Describe,
    ReadText,
    Diagnose
}

public sealed class VisionService : IAsyncDisposable
{
    private readonly WindowsCameraCapture _camera;
    private readonly OllamaVisionClient _vision;
    private readonly VisionMonitor _monitor;
    private readonly CancellationTokenSource _cts = new();

    public VisionService(WindowsCameraCapture? camera = null, OllamaVisionClient? vision = null)
    {
        _camera = camera ?? new WindowsCameraCapture();
        _vision = vision ?? new OllamaVisionClient();
        _monitor = new VisionMonitor(_camera);
        _monitor.Start();
        _ = Task.Run(() => WarmWhenCameraAppearsAsync(_cts.Token));
    }

    public Task<IReadOnlyList<CameraDevice>> ListCamerasAsync() => _camera.ListAsync();

    public async Task<VisionAnalysisResult> AnalyzeCameraAsync(VisionTask task, CancellationToken cancellationToken = default)
    {
        var snapshot = _monitor.Latest;
        byte[]? jpeg = snapshot?.Jpeg;
        if (jpeg is null || snapshot is null || DateTimeOffset.UtcNow - snapshot.CapturedAt > TimeSpan.FromSeconds(10))
        {
            var frame = await _camera.CaptureJpegAsync();
            if (!frame.Success || frame.Jpeg is null)
                return new(false, frame.Message);
            jpeg = frame.Jpeg;
        }

        var prompt = task switch
        {
            VisionTask.ReadText => "Przeczytaj dokładnie tekst widoczny na obrazie. Jeśli to komunikat błędu, zacytuj najważniejszy kod i treść. Odpowiedz po polsku, krótko i precyzyjnie.",
            VisionTask.Diagnose => "Przeanalizuj obraz jak techniczny asystent. Powiedz co widać, wskaż błąd lub problem i podaj najbardziej prawdopodobne rozwiązanie. Odpowiedz po polsku i konkretnie.",
            _ => "Opisz krótko i konkretnie po polsku, co widzisz na obrazie. Zwróć uwagę na przedmioty, ekran, tekst i komunikaty błędów."
        };

        return await _vision.AnalyzeAsync(jpeg, prompt, cancellationToken);
    }

    private async Task WarmWhenCameraAppearsAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (_monitor.Latest is not null)
                {
                    await _vision.WarmUpAsync(cancellationToken);
                    return;
                }
                await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }
    }

    public async ValueTask DisposeAsync()
    {
        _cts.Cancel();
        await _monitor.DisposeAsync();
        await _camera.DisposeAsync();
        _cts.Dispose();
    }
}
