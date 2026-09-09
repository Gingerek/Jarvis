using System.Globalization;
using Jarvis.Commands;
using Jarvis.Lightroom;

namespace Jarvis.VoiceHost;

internal sealed class LightroomCommandExecutor
{
    private readonly LightroomBridgeClient _client;

    private static readonly Dictionary<string, string> Labels = new(StringComparer.Ordinal)
    {
        ["Exposure2012"] = "ekspozycja",
        ["Contrast2012"] = "kontrast",
        ["Highlights2012"] = "światła",
        ["Shadows2012"] = "cienie",
        ["Whites2012"] = "biele",
        ["Blacks2012"] = "czernie",
        ["Texture"] = "tekstura",
        ["Clarity2012"] = "przejrzystość",
        ["Dehaze"] = "odmglenie",
        ["Vibrance"] = "wibracja",
        ["Saturation"] = "nasycenie",
        ["Temperature"] = "temperatura",
        ["Tint"] = "odcień",
        ["local_Exposure"] = "ekspozycja maski",
        ["local_Contrast"] = "kontrast maski",
        ["local_Highlights"] = "światła maski",
        ["local_Shadows"] = "cienie maski",
        ["local_Whites"] = "biele maski",
        ["local_Blacks"] = "czernie maski",
        ["local_Texture"] = "tekstura maski",
        ["local_Clarity"] = "przejrzystość maski",
        ["local_Dehaze"] = "odmglenie maski",
        ["local_Saturation"] = "nasycenie maski",
        ["local_Temperature"] = "temperatura maski",
        ["local_Tint"] = "odcień maski",
        ["ParametricShadows"] = "krzywa cienie",
        ["ParametricDarks"] = "krzywa ciemne",
        ["ParametricLights"] = "krzywa jasne",
        ["ParametricHighlights"] = "krzywa swiatla",
        ["SaturationAdjustmentRed"] = "nasycenie czerwieni",
        ["SaturationAdjustmentOrange"] = "nasycenie pomaranczowego",
        ["SaturationAdjustmentYellow"] = "nasycenie zoltego",
        ["SaturationAdjustmentGreen"] = "nasycenie zieleni",
        ["SaturationAdjustmentAqua"] = "nasycenie aqua",
        ["SaturationAdjustmentBlue"] = "nasycenie niebieskiego",
        ["SaturationAdjustmentPurple"] = "nasycenie fioletowego",
        ["SaturationAdjustmentMagenta"] = "nasycenie magenty",
        ["HueAdjustmentRed"] = "odcien czerwieni",
        ["HueAdjustmentOrange"] = "odcien pomaranczowego",
        ["HueAdjustmentYellow"] = "odcien zoltego",
        ["HueAdjustmentGreen"] = "odcien zieleni",
        ["HueAdjustmentAqua"] = "odcien aqua",
        ["HueAdjustmentBlue"] = "odcien niebieskiego",
        ["HueAdjustmentPurple"] = "odcien fioletowego",
        ["HueAdjustmentMagenta"] = "odcien magenty",
        ["LuminanceAdjustmentRed"] = "luminancja czerwieni",
        ["LuminanceAdjustmentOrange"] = "luminancja pomaranczowego",
        ["LuminanceAdjustmentYellow"] = "luminancja zoltego",
        ["LuminanceAdjustmentGreen"] = "luminancja zieleni",
        ["LuminanceAdjustmentAqua"] = "luminancja aqua",
        ["LuminanceAdjustmentBlue"] = "luminancja niebieskiego",
        ["LuminanceAdjustmentPurple"] = "luminancja fioletowego",
        ["LuminanceAdjustmentMagenta"] = "luminancja magenty",
        ["SplitToningShadowHue"] = "grading odcien cieni",
        ["SplitToningShadowSaturation"] = "grading nasycenie cieni",
        ["ColorGradeShadowLum"] = "grading luminancja cieni",
        ["SplitToningHighlightHue"] = "grading odcien swiatel",
        ["SplitToningHighlightSaturation"] = "grading nasycenie swiatel",
        ["ColorGradeHighlightLum"] = "grading luminancja swiatel",
        ["ColorGradeMidtoneHue"] = "grading odcien poltonow",
        ["ColorGradeMidtoneSat"] = "grading nasycenie poltonow",
        ["ColorGradeMidtoneLum"] = "grading luminancja poltonow",
        ["ColorGradeGlobalHue"] = "grading odcien globalny",
        ["ColorGradeGlobalSat"] = "grading nasycenie globalne",
        ["ColorGradeGlobalLum"] = "grading luminancja globalna",
        ["SplitToningBalance"] = "grading balans",
        ["ColorGradeBlending"] = "grading mieszanie",
        ["Sharpness"] = "wyostrzenie",
        ["SharpenRadius"] = "promien wyostrzenia",
        ["SharpenDetail"] = "detal wyostrzenia",
        ["SharpenEdgeMasking"] = "maskowanie wyostrzenia",
        ["LuminanceSmoothing"] = "redukcja szumu luminancji",
        ["LuminanceNoiseReductionDetail"] = "detal redukcji szumu luminancji",
        ["LuminanceNoiseReductionContrast"] = "kontrast redukcji szumu luminancji",
        ["ColorNoiseReduction"] = "redukcja szumu koloru",
        ["ColorNoiseReductionDetail"] = "detal redukcji szumu koloru",
        ["ColorNoiseReductionSmoothness"] = "gladkosc redukcji szumu koloru",
        ["PostCropVignetteAmount"] = "winieta",
        ["PostCropVignetteMidpoint"] = "srodek winiety",
        ["PostCropVignetteFeather"] = "miekkosc winiety",
        ["PostCropVignetteRoundness"] = "okrąglosc winiety",
        ["GrainAmount"] = "ziarno",
        ["GrainSize"] = "rozmiar ziarna",
        ["GrainFrequency"] = "nieregularnosc ziarna",
        ["AutoLateralCA"] = "aberracja chromatyczna",
        ["LensProfileEnable"] = "profil obiektywu",
        ["LensProfileDistortionScale"] = "korekcja dystorsji profilu",
        ["LensProfileVignettingScale"] = "korekcja winiety profilu",
        ["LensManualDistortionAmount"] = "dystorsja reczna",
        ["PerspectiveVertical"] = "transformacja pionowa",
        ["PerspectiveHorizontal"] = "transformacja pozioma",
        ["PerspectiveRotate"] = "obrot transformacji",
        ["PerspectiveScale"] = "skala transformacji",
        ["PerspectiveAspect"] = "proporcje transformacji",
        ["PerspectiveX"] = "przesuniecie transformacji x",
        ["PerspectiveY"] = "przesuniecie transformacji y",
        ["ShadowTint"] = "kalibracja odcien cieni",
        ["RedHue"] = "kalibracja odcien czerwonego",
        ["RedSaturation"] = "kalibracja nasycenie czerwonego",
        ["GreenHue"] = "kalibracja odcien zielonego",
        ["GreenSaturation"] = "kalibracja nasycenie zielonego",
        ["BlueHue"] = "kalibracja odcien niebieskiego",
        ["BlueSaturation"] = "kalibracja nasycenie niebieskiego",
        ["LensBlurAmount"] = "rozmycie obiektywu",
        ["LensBlurCatEye"] = "cat eye rozmycia",
        ["LensBlurHighlightsBoost"] = "wzmocnienie swiatel rozmycia",
        ["local_Sharpness"] = "wyostrzenie maski",
        ["local_LuminanceNoise"] = "redukcja szumu maski",
        ["local_Moire"] = "moire maski",
        ["local_Defringe"] = "defringe maski",
        ["local_Hue"] = "odcien maski lokalny",
        ["local_Grain"] = "ziarno maski",
        ["local_RefineSaturation"] = "refine saturation maski"
    };

    private static string SetLabel(string parameter) => parameter switch
    {
        "Exposure2012" => "ekspozycję",
        "Texture" => "teksturę",
        "Vibrance" => "wibrację",
        "Temperature" => "temperaturę",
        "local_Exposure" => "ekspozycję maski",
        "local_Texture" => "teksturę maski",
        "local_Temperature" => "temperaturę maski",
        _ => Label(parameter)
    };

    private static string Speak(string? value) => (value ?? string.Empty).Replace('.', ',');

    public LightroomCommandExecutor(LightroomBridgeClient client) => _client = client;

    public async Task<CommandExecutionOutcome?> ExecuteAsync(
        CommandRequest request,
        CancellationToken cancellationToken)
    {
        return request.Intent switch
        {
            CommandIntent.LightroomGetAdjustment => await GetAsync(request.Argument!, cancellationToken),
            CommandIntent.LightroomSetAdjustment => await SetAsync(request.Argument!, cancellationToken),
            CommandIntent.LightroomAdjustAdjustment => await AdjustAsync(request.Argument!, cancellationToken),
            CommandIntent.LightroomResetAdjustment => await ResetAdjustmentAsync(request.Argument!, cancellationToken),
            CommandIntent.LightroomResetAllDevelop => await ActionAsync("develop_reset_all", "Resetuję całą obróbkę zdjęcia.", "LightroomResetAllDevelop", cancellationToken),
            CommandIntent.LightroomResetTransforms => await ActionAsync("develop_reset_transforms", "Resetuję transformacje zdjęcia.", "LightroomResetTransforms", cancellationToken),
            CommandIntent.LightroomAutoTone => await ActionAsync("auto_tone", "Włączam Auto Tone w Lightroomie.", "LightroomAutoTone", cancellationToken),
            CommandIntent.LightroomAutoWhiteBalance => await ActionAsync("auto_wb", "Ustawiam automatyczny balans bieli.", "LightroomAutoWhiteBalance", cancellationToken),
            CommandIntent.LightroomNextPhoto => await ActionAsync("selection_next", "Przechodzę do następnego zdjęcia.", "LightroomNextPhoto", cancellationToken),
            CommandIntent.LightroomPreviousPhoto => await ActionAsync("selection_previous", "Wracam do poprzedniego zdjęcia.", "LightroomPreviousPhoto", cancellationToken),
            CommandIntent.LightroomGetRating => await GetRatingAsync(cancellationToken),
            CommandIntent.LightroomSetRating => await SetRatingAsync(request.Argument!, cancellationToken),
            CommandIntent.LightroomIncreaseRating => await RatingStepAsync("rating_up", cancellationToken),
            CommandIntent.LightroomDecreaseRating => await RatingStepAsync("rating_down", cancellationToken),
            CommandIntent.LightroomGetFlag => await GetFlagAsync(cancellationToken),
            CommandIntent.LightroomFlagPick => await ActionAsync("flag_pick", "Oznaczam zdjęcie jako wybrane.", "LightroomFlagPick", cancellationToken),
            CommandIntent.LightroomFlagReject => await ActionAsync("flag_reject", "Oznaczam zdjęcie jako odrzucone.", "LightroomFlagReject", cancellationToken),
            CommandIntent.LightroomClearFlag => await ActionAsync("flag_clear", "Usuwam flagę ze zdjęcia.", "LightroomClearFlag", cancellationToken),
            CommandIntent.LightroomGetCropAngle => await GetCropAngleAsync(cancellationToken),
            CommandIntent.LightroomSetCropAngle => await SetCropAngleAsync(request.Argument!, cancellationToken),
            CommandIntent.LightroomResetCrop => await ActionAsync("crop_reset", "Resetuję kadrowanie.", "LightroomResetCrop", cancellationToken),
            CommandIntent.LightroomGetMaskCount => await GetMaskCountAsync(cancellationToken),
            CommandIntent.LightroomCreateSubjectMask => await CreateAiMaskAsync("subject", "obiektu", cancellationToken),
            CommandIntent.LightroomCreateSkyMask => await CreateAiMaskAsync("sky", "nieba", cancellationToken),
            CommandIntent.LightroomCreateBackgroundMask => await CreateAiMaskAsync("background", "tła", cancellationToken),
            CommandIntent.LightroomCreateMaskComponent => await MaskComponentAsync("mask_create_component", request.Argument!, "Tworzę nową maskę.", "LightroomCreateMaskComponent", cancellationToken),
            CommandIntent.LightroomAddMaskComponent => await MaskComponentAsync("mask_add_component", request.Argument!, "Dodaję składnik do maski.", "LightroomAddMaskComponent", cancellationToken),
            CommandIntent.LightroomSubtractMaskComponent => await MaskComponentAsync("mask_subtract_component", request.Argument!, "Odejmuję składnik od maski.", "LightroomSubtractMaskComponent", cancellationToken),
            CommandIntent.LightroomIntersectMaskComponent => await MaskComponentAsync("mask_intersect_component", request.Argument!, "Przecinam maskę ze składnikiem.", "LightroomIntersectMaskComponent", cancellationToken),
            CommandIntent.LightroomToggleMaskOverlay => await ActionAsync("mask_overlay", "Przełączam nakładkę maski.", "LightroomMaskOverlay", cancellationToken),
            CommandIntent.LightroomResetMasks => await ActionAsync("mask_reset", "Usuwam wszystkie maski.", "LightroomResetMasks", cancellationToken),
            CommandIntent.LightroomGetDevelopTool => await QueryAsync("develop_get_tool", "Aktywne narzędzie: ", "LightroomGetDevelopTool", cancellationToken),
            CommandIntent.LightroomSelectDevelopTool => await ActionWithArgAsync("develop_select_tool", request.Argument!, $"Włączam narzędzie {request.Argument}.", "LightroomSelectDevelopTool", cancellationToken),
            CommandIntent.LightroomGetColorGradingView => await QueryAsync("grading_get_view", "Widok Color Grading: ", "LightroomGetColorGradingView", cancellationToken),
            CommandIntent.LightroomSetColorGradingView => await ActionWithArgAsync("grading_set_view", request.Argument!, $"Ustawiam widok Color Grading: {request.Argument}.", "LightroomSetColorGradingView", cancellationToken),
            CommandIntent.LightroomGetLensBlurBokeh => await QueryAsync("lensblur_get_bokeh", "Bokeh Lens Blur: ", "LightroomGetLensBlurBokeh", cancellationToken),
            CommandIntent.LightroomSetLensBlurBokeh => await ActionWithArgAsync("lensblur_set_bokeh", request.Argument!, $"Ustawiam bokeh {request.Argument}.", "LightroomSetLensBlurBokeh", cancellationToken),
            CommandIntent.LightroomOpenRemove => await ActionWithArgAsync("remove_open", request.Argument!, "Otwieram narzędzie Remove.", "LightroomOpenRemove", cancellationToken),
            CommandIntent.LightroomResetRemove => await ActionAsync("remove_reset", "Resetuję poprawki Remove.", "LightroomResetRemove", cancellationToken),
            CommandIntent.LightroomCopyDevelopSettings => await ActionAsync("develop_copy_settings", "Kopiuję ustawienia obróbki.", "LightroomCopyDevelopSettings", cancellationToken),
            CommandIntent.LightroomPasteDevelopSettings => await ActionAsync("develop_paste_settings", "Wklejam skopiowane ustawienia obróbki.", "LightroomPasteDevelopSettings", cancellationToken),
            CommandIntent.LightroomUndo => await ActionAsync("undo", "Cofam ostatnią zmianę w Lightroomie.", "LightroomUndo", cancellationToken),
            CommandIntent.LightroomRedo => await ActionAsync("redo", "Ponawiam ostatnią zmianę w Lightroomie.", "LightroomRedo", cancellationToken),
            _ => null
        };
    }

    private async Task<CommandExecutionOutcome> GetAsync(string parameter, CancellationToken ct)
    {
        var result = await _client.SendAsync("develop_get", parameter, cancellationToken: ct);
        if (!result.Success) return Failed(result);
        var label = Label(parameter);
        var verb = parameter is "Highlights2012" or "Shadows2012" or "Whites2012" or "Blacks2012" ? "wynoszą" : "wynosi";
        return new($"{label} {verb} {Speak(result.Value)}.", "LightroomGetAdjustment", parameter);
    }

    private async Task<CommandExecutionOutcome> SetAsync(string packed, CancellationToken ct)
    {
        var parsed = LightroomCommandParser.Unpack(packed);
        if (parsed is null) return new("Nie rozumiem wartości Lightrooma.", "LightroomBadValue");
        var (parameter, value) = parsed.Value;
        var result = await _client.SendAsync("develop_set", parameter,
            value.ToString("0.####", CultureInfo.InvariantCulture), ct);
        if (!result.Success) return Failed(result);
        return new($"Ustawiam {SetLabel(parameter)} na {Speak(result.Value)}.",
            "LightroomSetAdjustment", parameter);
    }

    private async Task<CommandExecutionOutcome> AdjustAsync(string packed, CancellationToken ct)
    {
        var parsed = LightroomCommandParser.Unpack(packed);
        if (parsed is null) return new("Nie rozumiem zmiany Lightrooma.", "LightroomBadDelta");
        var (parameter, delta) = parsed.Value;
        var current = await _client.SendAsync("develop_get", parameter, cancellationToken: ct);
        if (!current.Success || !double.TryParse(current.Value,
            NumberStyles.Float, CultureInfo.InvariantCulture, out var currentValue))
            return Failed(current);

        var target = currentValue + delta;
        var set = await _client.SendAsync("develop_set", parameter,
            target.ToString("0.####", CultureInfo.InvariantCulture), ct);
        if (!set.Success) return Failed(set);
        return new($"Zmieniam {SetLabel(parameter)} z {Speak(current.Value)} na {Speak(set.Value)}.",
            "LightroomAdjustAdjustment", parameter);
    }

    private async Task<CommandExecutionOutcome> ResetAdjustmentAsync(string parameter, CancellationToken ct)
    {
        var result = await _client.SendAsync("develop_reset_param", parameter, cancellationToken: ct);
        return result.Success ? new($"Resetuję {SetLabel(parameter)} do wartości domyślnej.", "LightroomResetAdjustment", parameter) : Failed(result);
    }

    private async Task<CommandExecutionOutcome> GetRatingAsync(CancellationToken ct)
    {
        var result = await _client.SendAsync("rating_get", cancellationToken: ct);
        if (!result.Success) return Failed(result);
        return new($"Ocena zdjęcia: {Speak(result.Value)} z 5.", "LightroomGetRating");
    }

    private async Task<CommandExecutionOutcome> SetRatingAsync(string rating, CancellationToken ct)
    {
        var result = await _client.SendAsync("rating_set", rating, cancellationToken: ct);
        if (!result.Success) return Failed(result);
        return new($"Ustawiam ocenę na {Speak(result.Value)} z 5.", "LightroomSetRating", result.Value);
    }

    private async Task<CommandExecutionOutcome> RatingStepAsync(string command, CancellationToken ct)
    {
        var result = await _client.SendAsync(command, cancellationToken: ct);
        if (!result.Success) return Failed(result);
        return new($"Ocena zdjęcia: {Speak(result.Value)} z 5.", "LightroomRatingStep", result.Value);
    }

    private async Task<CommandExecutionOutcome> GetFlagAsync(CancellationToken ct)
    {
        var result = await _client.SendAsync("flag_get", cancellationToken: ct);
        if (!result.Success) return Failed(result);
        var text = result.Value switch
        {
            "1" => "Zdjęcie jest oznaczone jako wybrane.",
            "-1" => "Zdjęcie jest oznaczone jako odrzucone.",
            _ => "Zdjęcie nie ma flagi."
        };
        return new(text, "LightroomGetFlag", result.Value);
    }

    private async Task<CommandExecutionOutcome> GetCropAngleAsync(CancellationToken ct)
    {
        var result = await _client.SendAsync("crop_get_angle", cancellationToken: ct);
        return result.Success ? new($"Kąt kadrowania: {Speak(result.Value)} stopnia.", "LightroomGetCropAngle", result.Value) : Failed(result);
    }

    private async Task<CommandExecutionOutcome> SetCropAngleAsync(string angle, CancellationToken ct)
    {
        var result = await _client.SendAsync("crop_set_angle", angle, cancellationToken: ct);
        return result.Success ? new($"Ustawiam kąt kadrowania na {Speak(result.Value)} stopnia.", "LightroomSetCropAngle", result.Value) : Failed(result);
    }

    private async Task<CommandExecutionOutcome> GetMaskCountAsync(CancellationToken ct)
    {
        var result = await _client.SendAsync("mask_count", cancellationToken: ct);
        return result.Success ? new($"Liczba masek: {result.Value}.", "LightroomGetMaskCount", result.Value) : Failed(result);
    }

    private async Task<CommandExecutionOutcome> CreateAiMaskAsync(string subtype, string label, CancellationToken ct)
    {
        var result = await _client.SendAsync("mask_create_ai", subtype, cancellationToken: ct);
        return result.Success ? new($"Tworzę maskę {label}.", "LightroomCreateAiMask", subtype) : Failed(result);
    }

    private async Task<CommandExecutionOutcome> QueryAsync(string command, string prefix, string status, CancellationToken ct)
    {
        var result = await _client.SendAsync(command, cancellationToken: ct);
        return result.Success ? new($"{prefix}{result.Value}.", status, result.Value) : Failed(result);
    }

    private async Task<CommandExecutionOutcome> ActionWithArgAsync(string command, string argument, string reply, string status, CancellationToken ct)
    {
        var result = await _client.SendAsync(command, argument, cancellationToken: ct);
        return result.Success ? new(reply, status, result.Value) : Failed(result);
    }

    private async Task<CommandExecutionOutcome> MaskComponentAsync(string command, string spec, string reply, string status, CancellationToken ct)
    {
        var parts = spec.Split('|', 2);
        var type = parts[0];
        var subtype = parts.Length > 1 ? parts[1] : string.Empty;
        var result = await _client.SendAsync(command, type, subtype, ct);
        return result.Success ? new(reply, status, spec) : Failed(result);
    }

    private async Task<CommandExecutionOutcome> ActionAsync(
        string command, string reply, string status, CancellationToken ct)
    {
        var result = await _client.SendAsync(command, cancellationToken: ct);
        return result.Success ? new(reply, status) : Failed(result);
    }

    private static string Label(string parameter) =>
        Labels.TryGetValue(parameter, out var label) ? label : parameter;

    private static CommandExecutionOutcome Failed(LightroomCommandResult result) =>
        new(result.Error is null
            ? "Lightroom nie wykonał polecenia."
            : $"Lightroom: {result.Error}",
            "LightroomUnavailable");
}
