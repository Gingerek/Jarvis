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
    [InlineData("ustaw ekspozycję maski na 0,5", CommandIntent.LightroomSetAdjustment, "local_Exposure|0.5")]
    [InlineData("zwiększ kontrast maski o 10", CommandIntent.LightroomAdjustAdjustment, "local_Contrast|10")]
    [InlineData("ustaw nasycenie niebieskiego na -20", CommandIntent.LightroomSetAdjustment, "SaturationAdjustmentBlue|-20")]
    [InlineData("zwiększ luminancję pomarańczowego o 10", CommandIntent.LightroomAdjustAdjustment, "LuminanceAdjustmentOrange|10")]
    [InlineData("ustaw wyostrzenie na 45", CommandIntent.LightroomSetAdjustment, "Sharpness|45")]
    [InlineData("ustaw ziarno na 25", CommandIntent.LightroomSetAdjustment, "GrainAmount|25")]
    [InlineData("ustaw transformację pionową na 12", CommandIntent.LightroomSetAdjustment, "PerspectiveVertical|12")]
    [InlineData("ustaw nasycenie niebieskiego w kalibracji na 20", CommandIntent.LightroomSetAdjustment, "BlueSaturation|20")]
    [InlineData("ustaw rozmycie obiektywu na 35", CommandIntent.LightroomSetAdjustment, "LensBlurAmount|35")]
    [InlineData("ustaw wyostrzenie maski na 15", CommandIntent.LightroomSetAdjustment, "local_Sharpness|15")]
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
    [InlineData("jaki kąt kadrowania", CommandIntent.LightroomGetCropAngle, null)]
    [InlineData("ustaw kąt kadrowania na 1,5", CommandIntent.LightroomSetCropAngle, "1.5")]
    [InlineData("resetuj kadrowanie", CommandIntent.LightroomResetCrop, null)]
    [InlineData("ile masek", CommandIntent.LightroomGetMaskCount, null)]
    [InlineData("utwórz maskę obiektu", CommandIntent.LightroomCreateSubjectMask, null)]
    [InlineData("utwórz maskę nieba", CommandIntent.LightroomCreateSkyMask, null)]
    [InlineData("utwórz maskę tła", CommandIntent.LightroomCreateBackgroundMask, null)]
    [InlineData("utwórz maskę ludzi", CommandIntent.LightroomCreateMaskComponent, "aiSelection|people")]
    [InlineData("utwórz maskę gradient liniowy", CommandIntent.LightroomCreateMaskComponent, "gradient|")]
    [InlineData("dodaj do maski niebo", CommandIntent.LightroomAddMaskComponent, "aiSelection|sky")]
    [InlineData("odejmij od maski ludzi", CommandIntent.LightroomSubtractMaskComponent, "aiSelection|people")]
    [InlineData("przetnij maskę z zakresem luminancji", CommandIntent.LightroomIntersectMaskComponent, "rangeMask|luminance")]
    [InlineData("pokaż maski", CommandIntent.LightroomToggleMaskOverlay, null)]
    [InlineData("usuń wszystkie maski", CommandIntent.LightroomResetMasks, null)]
    [InlineData("jakie narzędzie Lightrooma", CommandIntent.LightroomGetDevelopTool, null)]
    [InlineData("otwórz kadrowanie", CommandIntent.LightroomSelectDevelopTool, "crop")]
    [InlineData("otwórz maskowanie", CommandIntent.LightroomSelectDevelopTool, "masking")]
    [InlineData("wróć do lupy", CommandIntent.LightroomSelectDevelopTool, "loupe")]
    [InlineData("ustaw grading cienie", CommandIntent.LightroomSetColorGradingView, "shadow")]
    [InlineData("bokeh pierścień", CommandIntent.LightroomSetLensBlurBokeh, "Ring")]
    [InlineData("otwórz klonowanie", CommandIntent.LightroomOpenRemove, "clone")]
    [InlineData("resetuj usuwanie", CommandIntent.LightroomResetRemove, null)]
    [InlineData("resetuj wyostrzenie", CommandIntent.LightroomResetAdjustment, "Sharpness")]
    [InlineData("resetuj całą obróbkę", CommandIntent.LightroomResetAllDevelop, null)]
    [InlineData("resetuj transformacje", CommandIntent.LightroomResetTransforms, null)]
    public void Registry_Parses_Lightroom_Selection_Commands(string text, CommandIntent intent, string? argument)
    {
        var request = new CommandRegistry().Parse(text);
        Assert.NotNull(request);
        Assert.Equal(intent, request.Intent);
        Assert.Equal(argument, request.Argument);
    }

}
