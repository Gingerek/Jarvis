namespace Jarvis.Audio;

public static class AudioDeviceSelector
{
    public static AudioDeviceInfo Select(
        IReadOnlyList<AudioDeviceInfo> devices,
        string? preferredDeviceId = null)
    {
        if (devices.Count == 0)
            throw new InvalidOperationException("No active capture devices were found.");

        if (!string.IsNullOrWhiteSpace(preferredDeviceId))
        {
            var preferred = devices.FirstOrDefault(d =>
                string.Equals(d.Id, preferredDeviceId, StringComparison.OrdinalIgnoreCase));
            if (preferred is not null) return preferred;
        }

        return devices.FirstOrDefault(d => d.IsDefault) ?? devices[0];
    }
}
