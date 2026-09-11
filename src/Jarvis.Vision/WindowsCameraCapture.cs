using Windows.Devices.Enumeration;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage.Streams;

namespace Jarvis.Vision;

public sealed record CameraDevice(string Id, string Name);
public sealed record CameraFrameResult(bool Success, byte[]? Jpeg, string Message, string? CameraName = null);

public sealed class WindowsCameraCapture : IAsyncDisposable
{
    private readonly SemaphoreSlim _gate = new(1, 1);
    private MediaCapture? _capture;
    private string? _activeDeviceId;
    private string? _activeCameraName;

    public async Task<IReadOnlyList<CameraDevice>> ListAsync()
    {
        var devices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);
        return devices.Select(x => new CameraDevice(x.Id, x.Name)).ToArray();
    }

    public async Task<CameraFrameResult> CaptureJpegAsync(string? preferredDeviceId = null)
    {
        await _gate.WaitAsync();
        try
        {
            var initialized = await EnsureInitializedAsync(preferredDeviceId);
            if (!initialized.Success) return initialized;
            using var stream = new InMemoryRandomAccessStream();
            await _capture!.CapturePhotoToStreamAsync(ImageEncodingProperties.CreateJpeg(), stream);
            stream.Seek(0);
            using var reader = new DataReader(stream.GetInputStreamAt(0));
            await reader.LoadAsync((uint)stream.Size);
            var bytes = new byte[(int)stream.Size];
            reader.ReadBytes(bytes);
            return new(true, bytes, "Klatka z kamery gotowa.", _activeCameraName);
        }
        catch (UnauthorizedAccessException)
        {
            ResetCapture();
            return new(false, null, "Windows nie pozwala Jarvisowi użyć kamery. Sprawdź uprawnienia kamery.", _activeCameraName);
        }
        catch (Exception ex)
        {
            ResetCapture();
            return new(false, null, $"Nie udało się pobrać obrazu z kamery: {ex.Message}", _activeCameraName);
        }
        finally
        {
            _gate.Release();
        }
    }

    private async Task<CameraFrameResult> EnsureInitializedAsync(string? preferredDeviceId)
    {
        if (_capture is not null && (preferredDeviceId is null || preferredDeviceId == _activeDeviceId))
            return new(true, null, "Kamera aktywna.", _activeCameraName);
        var devices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);
        if (devices.Count == 0)
            return new(false, null, "Nie widzę żadnej podłączonej kamery.");

        var selected = string.IsNullOrWhiteSpace(preferredDeviceId)
            ? devices[0]
            : devices.FirstOrDefault(x => x.Id == preferredDeviceId) ?? devices[0];

        ResetCapture();
        var capture = new MediaCapture();
        var settings = new MediaCaptureInitializationSettings
        {
            VideoDeviceId = selected.Id,
            StreamingCaptureMode = StreamingCaptureMode.Video,
            MemoryPreference = MediaCaptureMemoryPreference.Cpu
        };
        await capture.InitializeAsync(settings);
        _capture = capture;
        _activeDeviceId = selected.Id;
        _activeCameraName = selected.Name;
        return new(true, null, "Kamera aktywna.", selected.Name);
    }

    private void ResetCapture()
    {
        try { _capture?.Dispose(); } catch { }
        _capture = null;
        _activeDeviceId = null;
        _activeCameraName = null;
    }
    public async ValueTask DisposeAsync()
    {
        await _gate.WaitAsync();
        try { ResetCapture(); }
        finally
        {
            _gate.Release();
            _gate.Dispose();
        }
    }
}
