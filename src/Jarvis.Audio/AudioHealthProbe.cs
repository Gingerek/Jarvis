namespace Jarvis.Audio;

public enum AudioHealthState
{
    Healthy,
    Degraded,
    Unavailable
}

public sealed record AudioHealthReport(
    AudioHealthState State,
    string Message,
    AudioDeviceInfo? SelectedDevice,
    int CaptureDeviceCount,
    int RenderDeviceCount);

public sealed class AudioHealthProbe(WasapiDeviceService deviceService)
{
    public AudioHealthReport Check(string? preferredDeviceId)
    {
        var capture = deviceService.GetCaptureDevices();
        var render = deviceService.GetRenderDevices();
        if (capture.Count == 0)
            return new(AudioHealthState.Unavailable, "No active capture device.", null, 0, render.Count);

        var selected = AudioDeviceSelector.Select(capture, preferredDeviceId);
        var preferredOk = string.IsNullOrWhiteSpace(preferredDeviceId) ||
            string.Equals(selected.Id, preferredDeviceId, StringComparison.OrdinalIgnoreCase);
        return preferredOk
            ? new(AudioHealthState.Healthy, "Audio endpoints available.", selected, capture.Count, render.Count)
            : new(AudioHealthState.Degraded, "Preferred microphone unavailable; fallback selected.", selected, capture.Count, render.Count);
    }
}
