using Jarvis.Commands;

namespace Jarvis.Core.Tests;

public sealed class WindowsSettingsCommandTests
{
    [Theory]
    [InlineData("otwórz ustawienia dźwięku", "ms-settings:sound")]
    [InlineData("ustawienia bluetooth", "ms-settings:bluetooth")]
    [InlineData("otwórz ustawienia wifi", "ms-settings:network-wifi")]
    [InlineData("otwórz ustawienia mikrofonu", "ms-settings:privacy-microphone")]
    [InlineData("otwórz ustawienia kamery", "ms-settings:privacy-webcam")]
    [InlineData("ustawienia aktualizacje", "ms-settings:windowsupdate")]
    public void Registry_Parses_Windows_Settings(string text, string expected)
    {
        var result = new CommandRegistry().Parse(text);
        Assert.NotNull(result);
        Assert.Equal(CommandIntent.OpenSettings, result.Intent);
        Assert.Equal(expected, result.Argument);
    }
}
