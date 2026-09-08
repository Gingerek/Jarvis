namespace Jarvis.Audio;

public sealed record AudioDeviceResolution(
    AudioDeviceInfo Device,
    bool PreferredAvailable,
    bool UsedFallback);

public sealed class AudioDeviceMonitor(WasapiDeviceService deviceService)
{
    public AudioDeviceResolution Resolve(string? preferredDeviceId)
    {
        var devices = deviceService.GetCaptureDevices();
        var preferredAvailable = !string.IsNullOrWhiteSpace(preferredDeviceId) &&
            devices.Any(d => string.Equals(d.Id, preferredDeviceId, StringComparison.OrdinalIgnoreCase));
        var selected = AudioDeviceSelector.Select(devices, preferredDeviceId);
        return new AudioDeviceResolution(selected, preferredAvailable, !preferredAvailable);
    }

    public async Task<AudioDeviceInfo> WaitForPreferredAsync(
        string preferredDeviceId,
        TimeSpan pollInterval,
        CancellationToken cancellationToken)
    {
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var match = deviceService.GetCaptureDevices().FirstOrDefault(d =>
                string.Equals(d.Id, preferredDeviceId, StringComparison.OrdinalIgnoreCase));
            if (match is not null) return match;
            await Task.Delay(pollInterval, cancellationToken);
        }
    }
}
