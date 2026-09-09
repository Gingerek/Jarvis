using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Jarvis.Commands;

public sealed class LightroomCommandParser
{
    private static readonly Dictionary<string, string> Adjustments = new(StringComparer.Ordinal)
    {
        ["ekspozycja"] = "Exposure2012",
        ["ekspozycje"] = "Exposure2012",
        ["kontrast"] = "Contrast2012",
        ["swiatla"] = "Highlights2012",
        ["cienie"] = "Shadows2012",
        ["biele"] = "Whites2012",
        ["czernie"] = "Blacks2012",
        ["tekstura"] = "Texture",
        ["teksture"] = "Texture",
        ["przejrzystosc"] = "Clarity2012",
        ["klarownosc"] = "Clarity2012",
        ["odmglenie"] = "Dehaze",
        ["wibracja"] = "Vibrance",
        ["wibracje"] = "Vibrance",
        ["nasycenie"] = "Saturation",
        ["temperatura"] = "Temperature",
        ["temperature"] = "Temperature",
        ["odcien"] = "Tint",
        ["ekspozycja maski"] = "local_Exposure",
        ["ekspozycje maski"] = "local_Exposure",
        ["kontrast maski"] = "local_Contrast",
        ["swiatla maski"] = "local_Highlights",
        ["cienie maski"] = "local_Shadows",
        ["biele maski"] = "local_Whites",
        ["czernie maski"] = "local_Blacks",
        ["tekstura maski"] = "local_Texture",
        ["teksture maski"] = "local_Texture",
        ["przejrzystosc maski"] = "local_Clarity",
        ["odmglenie maski"] = "local_Dehaze",
        ["nasycenie maski"] = "local_Saturation",
        ["temperatura maski"] = "local_Temperature",
        ["temperature maski"] = "local_Temperature",
        ["odcien maski"] = "local_Tint",
        ["krzywa cienie"] = "ParametricShadows",
        ["krzywa ciemne"] = "ParametricDarks",
        ["krzywa jasne"] = "ParametricLights",
        ["krzywa swiatla"] = "ParametricHighlights",
        ["nasycenie czerwieni"] = "SaturationAdjustmentRed",
        ["nasycenie pomaranczowego"] = "SaturationAdjustmentOrange",
        ["nasycenie zoltego"] = "SaturationAdjustmentYellow",
        ["nasycenie zieleni"] = "SaturationAdjustmentGreen",
        ["nasycenie aqua"] = "SaturationAdjustmentAqua",
        ["nasycenie niebieskiego"] = "SaturationAdjustmentBlue",
        ["nasycenie fioletowego"] = "SaturationAdjustmentPurple",
        ["nasycenie magenty"] = "SaturationAdjustmentMagenta",
        ["odcien czerwieni"] = "HueAdjustmentRed",
        ["odcien pomaranczowego"] = "HueAdjustmentOrange",
        ["odcien zoltego"] = "HueAdjustmentYellow",
        ["odcien zieleni"] = "HueAdjustmentGreen",
        ["odcien aqua"] = "HueAdjustmentAqua",
        ["odcien niebieskiego"] = "HueAdjustmentBlue",
        ["odcien fioletowego"] = "HueAdjustmentPurple",
        ["odcien magenty"] = "HueAdjustmentMagenta",
        ["luminancja czerwieni"] = "LuminanceAdjustmentRed",
        ["luminancje czerwieni"] = "LuminanceAdjustmentRed",
        ["luminancja pomaranczowego"] = "LuminanceAdjustmentOrange",
        ["luminancje pomaranczowego"] = "LuminanceAdjustmentOrange",
        ["luminancja zoltego"] = "LuminanceAdjustmentYellow",
        ["luminancje zoltego"] = "LuminanceAdjustmentYellow",
        ["luminancja zieleni"] = "LuminanceAdjustmentGreen",
        ["luminancje zieleni"] = "LuminanceAdjustmentGreen",
        ["luminancja aqua"] = "LuminanceAdjustmentAqua",
        ["luminancje aqua"] = "LuminanceAdjustmentAqua",
        ["luminancja niebieskiego"] = "LuminanceAdjustmentBlue",
        ["luminancje niebieskiego"] = "LuminanceAdjustmentBlue",
        ["luminancja fioletowego"] = "LuminanceAdjustmentPurple",
        ["luminancje fioletowego"] = "LuminanceAdjustmentPurple",
        ["luminancja magenty"] = "LuminanceAdjustmentMagenta",
        ["luminancje magenty"] = "LuminanceAdjustmentMagenta",
        ["grading odcien cieni"] = "SplitToningShadowHue",
        ["grading nasycenie cieni"] = "SplitToningShadowSaturation",
        ["grading luminancja cieni"] = "ColorGradeShadowLum",
        ["grading odcien swiatel"] = "SplitToningHighlightHue",
        ["grading nasycenie swiatel"] = "SplitToningHighlightSaturation",
        ["grading luminancja swiatel"] = "ColorGradeHighlightLum",
        ["grading odcien poltonow"] = "ColorGradeMidtoneHue",
        ["grading nasycenie poltonow"] = "ColorGradeMidtoneSat",
        ["grading luminancja poltonow"] = "ColorGradeMidtoneLum",
        ["grading odcien globalny"] = "ColorGradeGlobalHue",
        ["grading nasycenie globalne"] = "ColorGradeGlobalSat",
        ["grading luminancja globalna"] = "ColorGradeGlobalLum",
        ["grading balans"] = "SplitToningBalance",
        ["grading mieszanie"] = "ColorGradeBlending",
        ["wyostrzenie"] = "Sharpness",
        ["promien wyostrzenia"] = "SharpenRadius",
        ["detal wyostrzenia"] = "SharpenDetail",
        ["maskowanie wyostrzenia"] = "SharpenEdgeMasking",
        ["redukcja szumu luminancji"] = "LuminanceSmoothing",
        ["redukcje szumu luminancji"] = "LuminanceSmoothing",
        ["detal redukcji szumu luminancji"] = "LuminanceNoiseReductionDetail",
        ["kontrast redukcji szumu luminancji"] = "LuminanceNoiseReductionContrast",
        ["redukcja szumu koloru"] = "ColorNoiseReduction",
        ["redukcje szumu koloru"] = "ColorNoiseReduction",
        ["detal redukcji szumu koloru"] = "ColorNoiseReductionDetail",
        ["gladkosc redukcji szumu koloru"] = "ColorNoiseReductionSmoothness",
        ["winieta"] = "PostCropVignetteAmount",
        ["srodek winiety"] = "PostCropVignetteMidpoint",
        ["miekkosc winiety"] = "PostCropVignetteFeather",
        ["okraglosc winiety"] = "PostCropVignetteRoundness",
        ["ziarno"] = "GrainAmount",
        ["rozmiar ziarna"] = "GrainSize",
        ["nieregularnosc ziarna"] = "GrainFrequency",
        ["aberracja chromatyczna"] = "AutoLateralCA",
        ["profil obiektywu"] = "LensProfileEnable",
        ["korekcja dystorsji profilu"] = "LensProfileDistortionScale",
        ["korekcje dystorsji profilu"] = "LensProfileDistortionScale",
        ["korekcja winiety profilu"] = "LensProfileVignettingScale",
        ["korekcje winiety profilu"] = "LensProfileVignettingScale",
        ["dystorsja reczna"] = "LensManualDistortionAmount",
        ["transformacja pionowa"] = "PerspectiveVertical",
        ["transformacje pionowa"] = "PerspectiveVertical",
        ["transformacja pozioma"] = "PerspectiveHorizontal",
        ["transformacje pozioma"] = "PerspectiveHorizontal",
        ["obrot transformacji"] = "PerspectiveRotate",
        ["skala transformacji"] = "PerspectiveScale",
        ["proporcje transformacji"] = "PerspectiveAspect",
        ["przesuniecie transformacji x"] = "PerspectiveX",
        ["przesuniecie transformacji y"] = "PerspectiveY",
        ["kalibracja odcien cieni"] = "ShadowTint",
        ["odcien cieni w kalibracji"] = "ShadowTint",
        ["kalibracja odcien czerwonego"] = "RedHue",
        ["odcien czerwonego w kalibracji"] = "RedHue",
        ["kalibracja nasycenie czerwonego"] = "RedSaturation",
        ["nasycenie czerwonego w kalibracji"] = "RedSaturation",
        ["kalibracja odcien zielonego"] = "GreenHue",
        ["odcien zielonego w kalibracji"] = "GreenHue",
        ["kalibracja nasycenie zielonego"] = "GreenSaturation",
        ["nasycenie zielonego w kalibracji"] = "GreenSaturation",
        ["kalibracja odcien niebieskiego"] = "BlueHue",
        ["odcien niebieskiego w kalibracji"] = "BlueHue",
        ["kalibracja nasycenie niebieskiego"] = "BlueSaturation",
        ["nasycenie niebieskiego w kalibracji"] = "BlueSaturation",
        ["rozmycie obiektywu"] = "LensBlurAmount",
        ["cat eye rozmycia"] = "LensBlurCatEye",
        ["wzmocnienie swiatel rozmycia"] = "LensBlurHighlightsBoost",
        ["wyostrzenie maski"] = "local_Sharpness",
        ["redukcja szumu maski"] = "local_LuminanceNoise",
        ["moire maski"] = "local_Moire",
        ["defringe maski"] = "local_Defringe",
        ["odcien maski lokalny"] = "local_Hue",
        ["ziarno maski"] = "local_Grain",
        ["refine saturation maski"] = "local_RefineSaturation"
    };
    private static readonly Dictionary<string, string> MaskComponents = new(StringComparer.Ordinal)
    {
        ["obiekt"] = "aiSelection|subject", ["obiektu"] = "aiSelection|subject", ["subject"] = "aiSelection|subject",
        ["niebo"] = "aiSelection|sky", ["nieba"] = "aiSelection|sky", ["niebem"] = "aiSelection|sky", ["tlo"] = "aiSelection|background", ["tla"] = "aiSelection|background", ["tlem"] = "aiSelection|background",
        ["obiekty"] = "aiSelection|objects", ["obiektow"] = "aiSelection|objects", ["ludzie"] = "aiSelection|people", ["ludzi"] = "aiSelection|people",
        ["osoby"] = "aiSelection|people", ["krajobraz"] = "aiSelection|landscape", ["krajobrazu"] = "aiSelection|landscape",
        ["pedzel"] = "brush|", ["pedzlem"] = "brush|", ["gradient liniowy"] = "gradient|", ["gradientem liniowym"] = "gradient|",
        ["gradient radialny"] = "radialGradient|", ["gradientem radialnym"] = "radialGradient|", ["zakres koloru"] = "rangeMask|color", ["zakresem koloru"] = "rangeMask|color",
        ["zakres luminancji"] = "rangeMask|luminance", ["zakresem luminancji"] = "rangeMask|luminance", ["zakres glebi"] = "rangeMask|depth", ["zakresem glebi"] = "rangeMask|depth"
    };

    public CommandRequest? Parse(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        var value = Simplify(NormalizePreservingNumbers(text));

        if (value is "auto ton" or "automatyczny ton" or "auto tone" or "automatyczna korekcja")
            return new(CommandIntent.LightroomAutoTone);
        if (value is "auto balans bieli" or "automatyczny balans bieli" or "auto wb")
            return new(CommandIntent.LightroomAutoWhiteBalance);

        if (value is "nastepne zdjecie" or "kolejne zdjecie" or "przejdz do nastepnego zdjecia")
            return new(CommandIntent.LightroomNextPhoto);
        if (value is "poprzednie zdjecie" or "przejdz do poprzedniego zdjecia")
            return new(CommandIntent.LightroomPreviousPhoto);
        if (value is "jaka ocena" or "podaj ocene" or "ile gwiazdek" or "jaka jest ocena")
            return new(CommandIntent.LightroomGetRating);
        if (value is "zwieksz ocene" or "dodaj gwiazdke")
            return new(CommandIntent.LightroomIncreaseRating);
        if (value is "zmniejsz ocene" or "odejmij gwiazdke")
            return new(CommandIntent.LightroomDecreaseRating);
        var rating = Regex.Match(value, @"^(?:ustaw )?(?:ocene(?: na)?|daj) (?<num>[0-5])(?: gwiazdek| gwiazdki| gwiazdke)?$");
        if (rating.Success)
            return new(CommandIntent.LightroomSetRating, rating.Groups["num"].Value);
        if (value is "jaka flaga" or "podaj flage" or "status flagi")
            return new(CommandIntent.LightroomGetFlag);
        if (value is "oznacz jako wybrane" or "flaga pick" or "ustaw flage pick")
            return new(CommandIntent.LightroomFlagPick);
        if (value is "oznacz jako odrzucone" or "flaga reject" or "ustaw flage reject")
            return new(CommandIntent.LightroomFlagReject);
        if (value is "usun flage" or "wyczysc flage" or "bez flagi")
            return new(CommandIntent.LightroomClearFlag);
        if (value is "kopiuj ustawienia develop" or "skopiuj ustawienia develop" or "kopiuj ustawienia lightrooma" or "skopiuj obrobke")
            return new(CommandIntent.LightroomCopyDevelopSettings);
        if (value is "wklej ustawienia develop" or "wklej ustawienia lightrooma" or "wklej obrobke" or "zastosuj skopiowane ustawienia")
            return new(CommandIntent.LightroomPasteDevelopSettings);
        if (value is "lightroom cofnij" or "cofnij w lightroomie" or "cofnij ostatnia zmiane w lightroomie")
            return new(CommandIntent.LightroomUndo);
        if (value is "lightroom ponow" or "ponow w lightroomie" or "ponow ostatnia zmiane w lightroomie")
            return new(CommandIntent.LightroomRedo);

        if (value is "jaki kat kadrowania" or "podaj kat kadrowania" or "jaki kat crop")
            return new(CommandIntent.LightroomGetCropAngle);
        var cropAngle = Regex.Match(value, @"^(?:ustaw )?(?:kat kadrowania|kat crop)(?: na)? (?<num>[+-]?\d+(?:\.\d+)?)$");
        if (cropAngle.Success)
            return new(CommandIntent.LightroomSetCropAngle, cropAngle.Groups["num"].Value);
        if (value is "resetuj kadrowanie" or "wyzeruj kadrowanie" or "reset crop")
            return new(CommandIntent.LightroomResetCrop);
        if (value is "ile masek" or "podaj liczbe masek" or "liczba masek")
            return new(CommandIntent.LightroomGetMaskCount);
        if (value is "utworz maske obiektu" or "utworz maske subject" or "maska obiektu")
            return new(CommandIntent.LightroomCreateSubjectMask);
        if (value is "utworz maske nieba" or "maska nieba")
            return new(CommandIntent.LightroomCreateSkyMask);
        if (value is "utworz maske tla" or "maska tla")
            return new(CommandIntent.LightroomCreateBackgroundMask);
        if (TryMaskOperation(value, "utworz maske ", CommandIntent.LightroomCreateMaskComponent) is { } createMask) return createMask;
        if (TryMaskOperation(value, "dodaj do maski ", CommandIntent.LightroomAddMaskComponent) is { } addMask) return addMask;
        if (TryMaskOperation(value, "odejmij od maski ", CommandIntent.LightroomSubtractMaskComponent) is { } subMask) return subMask;
        if (TryMaskOperation(value, "przetnij maske z ", CommandIntent.LightroomIntersectMaskComponent) is { } intMask) return intMask;
        if (value is "pokaz maski" or "ukryj maski" or "przelacz nakladke maski")
            return new(CommandIntent.LightroomToggleMaskOverlay);
        if (value is "usun wszystkie maski" or "wyczysc wszystkie maski" or "resetuj maski")
            return new(CommandIntent.LightroomResetMasks);

        if (value is "jakie narzedzie develop" or "jakie narzedzie lightrooma" or "aktywne narzedzie lightrooma") return new(CommandIntent.LightroomGetDevelopTool);
        if (value is "otworz kadrowanie" or "wlacz kadrowanie" or "narzedzie kadrowania") return new(CommandIntent.LightroomSelectDevelopTool, "crop");
        if (value is "otworz maskowanie" or "wlacz maskowanie" or "narzedzie maskowania") return new(CommandIntent.LightroomSelectDevelopTool, "masking");
        if (value is "wroc do lupy" or "narzedzie lupa" or "zamknij narzedzie develop") return new(CommandIntent.LightroomSelectDevelopTool, "loupe");
        if (value is "otworz point color" or "otworz kolor punktowy" or "narzedzie point color") return new(CommandIntent.LightroomSelectDevelopTool, "point_color");
        if (value is "jaki widok color grading" or "jaki widok grading") return new(CommandIntent.LightroomGetColorGradingView);
        var grading = new Dictionary<string,string> { ["trzy kola"]="3-way", ["cienie"]="shadow", ["poltony"]="midtone", ["swiatla"]="highlight", ["globalny"]="global" };
        foreach (var pair in grading) if (value == "ustaw grading " + pair.Key || value == "widok grading " + pair.Key) return new(CommandIntent.LightroomSetColorGradingView, pair.Value);
        if (value is "jaki bokeh rozmycia" or "jaki ksztalt bokeh") return new(CommandIntent.LightroomGetLensBlurBokeh);
        var bokeh = new Dictionary<string,string> { ["kolo"]="Circle", ["mydlana banka"]="SoapBubble", ["listki"]="Blade", ["pierscien"]="Ring", ["anamorficzny"]="Anamorphic" };
        foreach (var pair in bokeh) if (value == "ustaw bokeh " + pair.Key || value == "bokeh " + pair.Key) return new(CommandIntent.LightroomSetLensBlurBokeh, pair.Value);
        if (value is "otworz usuwanie" or "otworz remove") return new(CommandIntent.LightroomOpenRemove, "heal_patchmatch");
        if (value is "otworz leczenie" or "narzedzie heal") return new(CommandIntent.LightroomOpenRemove, "heal");
        if (value is "otworz klonowanie" or "narzedzie clone") return new(CommandIntent.LightroomOpenRemove, "clone");
        if (value is "resetuj usuwanie" or "wyczysc usuwanie" or "usun wszystkie poprawki remove") return new(CommandIntent.LightroomResetRemove);

        var get = Regex.Match(value,
            @"^(?:jaka jest|jaki jest|podaj|ile wynosi) (?<name>.+)$");
        if (get.Success && Resolve(get.Groups["name"].Value) is { } getParam)
            return new(CommandIntent.LightroomGetAdjustment, getParam);

        if (value is "resetuj cala obrobke" or "wyzeruj cala obrobke" or "resetuj develop")
            return new(CommandIntent.LightroomResetAllDevelop);
        if (value is "resetuj transformacje" or "wyzeruj transformacje" or "reset transform")
            return new(CommandIntent.LightroomResetTransforms);
        var reset = Regex.Match(value, @"^(?:wyzeruj|resetuj) (?<name>.+)$");
        if (reset.Success && Resolve(reset.Groups["name"].Value) is { } resetParam)
            return new(CommandIntent.LightroomResetAdjustment, resetParam);

        var set = Regex.Match(value,
            @"^(?:ustaw )?(?<name>[a-z ]+?)(?: na)? (?<num>[+-]?\d+(?:\.\d+)?)$");
        if (set.Success && Resolve(set.Groups["name"].Value) is { } setParam &&
            TryNumber(set.Groups["num"].Value, out var setValue))
            return new(CommandIntent.LightroomSetAdjustment, Pack(setParam, setValue));
        var delta = Regex.Match(value,
            @"^(?<verb>zwieksz|podnies|dodaj|zmniejsz|obniz) (?<name>.+?)(?: o)? (?<num>\d+(?:\.\d+)?)$");
        if (delta.Success && Resolve(delta.Groups["name"].Value) is { } deltaParam &&
            TryNumber(delta.Groups["num"].Value, out var amount))
        {
            var verb = delta.Groups["verb"].Value;
            if (verb is "zmniejsz" or "obniz") amount = -amount;
            return new(CommandIntent.LightroomAdjustAdjustment, Pack(deltaParam, amount));
        }

        var signed = Regex.Match(value,
            @"^(?<name>[a-z ]+?) (?<sign>plus|minus) (?<num>\d+(?:\.\d+)?)$");
        if (signed.Success && Resolve(signed.Groups["name"].Value) is { } signedParam &&
            TryNumber(signed.Groups["num"].Value, out var signedAmount))
        {
            if (signed.Groups["sign"].Value == "minus") signedAmount = -signedAmount;
            return new(CommandIntent.LightroomAdjustAdjustment, Pack(signedParam, signedAmount));
        }

        return null;
    }

    private static CommandRequest? TryMaskOperation(string value, string prefix, CommandIntent intent)
    {
        if (!value.StartsWith(prefix, StringComparison.Ordinal)) return null;
        var key = value[prefix.Length..].Trim();
        return MaskComponents.TryGetValue(key, out var spec) ? new(intent, spec) : null;
    }

    public static (string Parameter, double Value)? Unpack(string? packed)
    {
        if (string.IsNullOrWhiteSpace(packed)) return null;
        var parts = packed.Split('|', 2);
        return parts.Length == 2 && TryNumber(parts[1], out var value)
            ? (parts[0], value) : null;
    }

    private static string? Resolve(string name)
    {
        var key = name.Trim();
        return Adjustments.TryGetValue(key, out var parameter) ? parameter : null;
    }

    private static string Pack(string parameter, double value) =>
        parameter + "|" + value.ToString("0.####", CultureInfo.InvariantCulture);

    private static bool TryNumber(string text, out double value) =>
        double.TryParse(text.Replace(',', '.'), NumberStyles.Float,
            CultureInfo.InvariantCulture, out value);

    private static string Simplify(string text)
    {
        string[] prefixes = ["hej jarvis ", "jarvis ", "prosze ", "czy mozesz ", "mozesz ", "teraz "];
        var current = text;
        bool changed;
        do
        {
            changed = false;
            foreach (var prefix in prefixes)
                if (current.StartsWith(prefix, StringComparison.Ordinal))
                { current = current[prefix.Length..].Trim(); changed = true; break; }
        } while (changed);
        return current;
    }

    private static string NormalizePreservingNumbers(string value)
    {
        var formD = value.ToLowerInvariant().Replace(',', '.').Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(formD.Length);
        foreach (var ch in formD)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(ch) == UnicodeCategory.NonSpacingMark) continue;
            var c = ch == 'ł' ? 'l' : ch;
            sb.Append(char.IsLetterOrDigit(c) || c is '.' or '+' or '-' ? c : ' ');
        }
        return Regex.Replace(sb.ToString(), @"\s+", " ").Trim();
    }
}
