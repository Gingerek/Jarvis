using Jarvis.Commands;

namespace Jarvis.Core.Tests;

public sealed class LightroomCommandParserTests
{
    [Theory]
    [InlineData("jaka jest ekspozycja", CommandIntent.LightroomGetAdjustment, "Exposure2012")]
    [InlineData("ustaw ekspozycję na 0,7", CommandIntent.LightroomSetAdjustment, "Exposure2012|0.7")]
    [InlineData("ekspozycja -0,5", CommandIntent.LightroomSetAdjustment, "Exposure2012|-0.5")]
    [InlineData("zwiększ cienie o 10", CommandIntent.LightroomAdjustAdjustment, "Shadows2012|10")]
    [InlineData("zmniejsz światła o 20", CommandIntent.LightroomAdjustAdjustment, "Highlights2012|-20")]
    [InlineData("ekspozycja plus 0,3", CommandIntent.LightroomAdjustAdjustment, "Exposure2012|0.3")]
    [InlineData("temperatura 5600", CommandIntent.LightroomSetAdjustment, "Temperature|5600")]
    [InlineData("ustaw temperaturę na 5600", CommandIntent.LightroomSetAdjustment, "Temperature|5600")]
    [InlineData("odcień -5", CommandIntent.LightroomSetAdjustment, "Tint|-5")]
    public void Registry_Parses_Lightroom_Adjustments(
        string text, CommandIntent intent, string argument)
    {
        var request = new CommandRegistry().Parse(text);
        Assert.NotNull(request);
        Assert.Equal(intent, request.Intent);
        Assert.Equal(argument, request.Argument);
    }

    [Theory]
    [InlineData("auto ton", CommandIntent.LightroomAutoTone)]
    [InlineData("automatyczny balans bieli", CommandIntent.LightroomAutoWhiteBalance)]
    [InlineData("Jarvis proszę auto balans bieli", CommandIntent.LightroomAutoWhiteBalance)]
    public void Registry_Parses_Lightroom_Auto_Commands(string text, CommandIntent intent)
    {
        var request = new CommandRegistry().Parse(text);
        Assert.NotNull(request);
        Assert.Equal(intent, request.Intent);
    }

    [Theory]
    [InlineData("następne zdjęcie", CommandIntent.LightroomNextPhoto, null)]
    [InlineData("poprzednie zdjęcie", CommandIntent.LightroomPreviousPhoto, null)]
    [InlineData("ustaw ocenę na 4", CommandIntent.LightroomSetRating, "4")]
    [InlineData("zwiększ ocenę", CommandIntent.LightroomIncreaseRating, null)]
    [InlineData("zmniejsz ocenę", CommandIntent.LightroomDecreaseRating, null)]
    [InlineData("oznacz jako wybrane", CommandIntent.LightroomFlagPick, null)]
    [InlineData("oznacz jako odrzucone", CommandIntent.LightroomFlagReject, null)]
    [InlineData("usuń flagę", CommandIntent.LightroomClearFlag, null)]
    [InlineData("kopiuj ustawienia develop", CommandIntent.LightroomCopyDevelopSettings, null)]
    [InlineData("wklej ustawienia lightrooma", CommandIntent.LightroomPasteDevelopSettings, null)]
    [InlineData("cofnij w lightroomie", CommandIntent.LightroomUndo, null)]
    [InlineData("ponów w lightroomie", CommandIntent.LightroomRedo, null)]
    public void Registry_Parses_Lightroom_Selection_Commands(string text, CommandIntent intent, string? argument)
    {
        var request = new CommandRegistry().Parse(text);
        Assert.NotNull(request);
        Assert.Equal(intent, request.Intent);
        Assert.Equal(argument, request.Argument);
    }

}
