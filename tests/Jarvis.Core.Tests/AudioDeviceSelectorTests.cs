using Jarvis.Audio;

namespace Jarvis.Core.Tests;

public sealed class AudioDeviceSelectorTests
{
    [Fact]
    public void PreferredEndpointWinsOverWindowsDefault()
    {
        var devices = new[]
        {
            new AudioDeviceInfo("default", "Default", true, 48000, 2, 32),
            new AudioDeviceInfo("preferred", "Preferred", false, 48000, 2, 32)
        };

        var selected = AudioDeviceSelector.Select(devices, "preferred");
        Assert.Equal("preferred", selected.Id);
    }

    [Fact]
    public void FallsBackToWindowsDefaultWhenPreferredMissing()
    {
        var devices = new[] { new AudioDeviceInfo("default", "Default", true, 48000, 2, 32) };
        Assert.Equal("default", AudioDeviceSelector.Select(devices, "missing").Id);
    }
}
