using Jarvis.WakeWord;

namespace Jarvis.Core.Tests;

public sealed class VoiceModeCommandParserTests
{
    [Theory]
    [InlineData("obudź się")]
    [InlineData("obudz sie")]
    [InlineData("obróż")]
    [InlineData("nie zasypiaj")]
    [InlineData("bądź aktywny")]
    public void Continuous_Phrases_Are_Recognized(string text)
    {
        Assert.Equal(VoiceModeCommand.EnableContinuous, VoiceModeCommandParser.Parse(text));
    }

    [Theory]
    [InlineData("idź spać")]
    [InlineData("idz spac")]
    [InlineData("możesz spać")]
    public void Sleep_Phrases_Are_Recognized(string text)
    {
        Assert.Equal(VoiceModeCommand.Sleep, VoiceModeCommandParser.Parse(text));
    }

    [Theory]
    [InlineData("obróć zdjęcie")]
    [InlineData("obraz")]
    public void Similar_Unrelated_Phrases_Are_Not_Continuous_Mode(string text)
    {
        Assert.Equal(VoiceModeCommand.None, VoiceModeCommandParser.Parse(text));
    }
}