using Jarvis.Commands;

namespace Jarvis.Core.Tests;

public sealed class SystemCommandParserTests
{
    [Theory]
    [InlineData("głośniej", CommandIntent.VolumeUp, null)]
    [InlineData("Jarvis proszę ciszej", CommandIntent.VolumeDown, null)]
    [InlineData("ustaw głośność na 40 procent", CommandIntent.SetVolume, "40")]
    [InlineData("wycisz dźwięk", CommandIntent.Mute, null)]
    [InlineData("włącz dźwięk", CommandIntent.Unmute, null)]
    [InlineData("minimalizuj okno", CommandIntent.MinimizeWindow, null)]
    [InlineData("maksymalizuj okno", CommandIntent.MaximizeWindow, null)]
    [InlineData("przywróć okno", CommandIntent.RestoreWindow, null)]
    [InlineData("zamknij aktywne okno", CommandIntent.CloseWindow, null)]
    [InlineData("pokaż pulpit", CommandIntent.ShowDesktop, null)]
    [InlineData("zablokuj komputer", CommandIntent.LockComputer, null)]
    public void Registry_Parses_System_Commands(string text, CommandIntent intent, string? argument)
    {
        var result = new CommandRegistry().Parse(text);
        Assert.NotNull(result);
        Assert.Equal(intent, result.Intent);
        Assert.Equal(argument, result.Argument);
    }
}
