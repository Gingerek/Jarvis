using Jarvis.Commands;

namespace Jarvis.Core.Tests;

public sealed class CommandRegistryTests
{
    [Theory]
    [InlineData("otwórz YouTube", CommandIntent.OpenWebsite, "https://www.youtube.com/")]
    [InlineData("Jarvis proszę otwórz YouTube teraz", CommandIntent.OpenWebsite, "https://www.youtube.com/")]
    [InlineData("czy możesz otworzyć Google", CommandIntent.OpenWebsite, "https://www.google.com/")]
    [InlineData("wyszukaj na YouTube Warszawa", CommandIntent.SearchYouTube, "warszawa")]
    [InlineData("wyszukaj w Google Sony A7 IV", CommandIntent.SearchWeb, "sony a7 iv")]
    public void Registry_Parses_General_Commands(string text, CommandIntent intent, string argument)
    {
        var result = new CommandRegistry().Parse(text);
        Assert.NotNull(result);
        Assert.Equal(intent, result.Intent);
        Assert.Equal(argument, result.Argument);
    }

    [Theory]
    [InlineData("która godzina", CommandIntent.GetTime)]
    [InlineData("która jest godzina", CommandIntent.GetTime)]
    [InlineData("który jest godzina", CommandIntent.GetTime)]
    [InlineData("która jest wedzina", CommandIntent.GetTime)]
    [InlineData("który jest wedzina", CommandIntent.GetTime)]
    [InlineData("jaka jest data", CommandIntent.GetDate)]
    [InlineData("jaki dzisiaj jest dzień", CommandIntent.GetDate)]
    [InlineData("jaki dzisiaj jest Janie", CommandIntent.GetDate)]
    [InlineData("jaki dziś dzień tygodnia", CommandIntent.GetDayOfWeek)]
    [InlineData("jaki dzisiaj jest dzień tygodnia", CommandIntent.GetDayOfWeek)]
    [InlineData("jaka jest pogoda", CommandIntent.GetWeather)]
    [InlineData("jaka jest pogoda dzisiaj", CommandIntent.GetWeather)]
    [InlineData("ile mam baterii", CommandIntent.GetBattery)]
    [InlineData("ile miejsca na dysku", CommandIntent.GetDiskSpace)]
    [InlineData("jakie okno jest aktywne", CommandIntent.GetActiveWindow)]
    [InlineData("następne okno", CommandIntent.SwitchWindow)]
    [InlineData("kopiuj", CommandIntent.KeyboardShortcut)]
    [InlineData("play pause", CommandIntent.MediaPlayPause)]
    [InlineData("co widzisz", CommandIntent.VisionDescribe)]
    [InlineData("przeczytaj ten błąd", CommandIntent.VisionReadText)]
    [InlineData("co jest nie tak", CommandIntent.VisionDiagnose)]
    [InlineData("jakie kamery widzisz", CommandIntent.VisionListCameras)]
    public void Registry_Parses_Local_Info(string text, CommandIntent intent)
    {
        var result = new CommandRegistry().Parse(text);
        Assert.NotNull(result);
        Assert.Equal(intent, result.Intent);
    }
}
