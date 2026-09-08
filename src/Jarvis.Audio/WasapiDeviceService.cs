using NAudio.CoreAudioApi;

namespace Jarvis.Audio;

public sealed class WasapiDeviceService
{
    public IReadOnlyList<AudioDeviceInfo> GetCaptureDevices() =>
        Enumerate(DataFlow.Capture, Role.Communications);

    public IReadOnlyList<AudioDeviceInfo> GetRenderDevices() =>
        Enumerate(DataFlow.Render, Role.Multimedia);

    public AudioDeviceInfo GetDefaultCaptureDevice() =>
        GetCaptureDevices().First(device => device.IsDefault);

    private static IReadOnlyList<AudioDeviceInfo> Enumerate(DataFlow flow, Role role)
    {
        using var enumerator = new MMDeviceEnumerator();
        var defaultDevice = enumerator.GetDefaultAudioEndpoint(flow, role);
        var devices = enumerator.EnumerateAudioEndPoints(flow, DeviceState.Active);
        return devices.Select(device => new AudioDeviceInfo(
            device.ID,
            device.FriendlyName,
            string.Equals(device.ID, defaultDevice.ID, StringComparison.OrdinalIgnoreCase),
            device.AudioClient.MixFormat.SampleRate,
            device.AudioClient.MixFormat.Channels,
            device.AudioClient.MixFormat.BitsPerSample)).ToArray();
    }
}
