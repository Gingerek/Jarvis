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
        ["odcien"] = "Tint"
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

        var get = Regex.Match(value,
            @"^(?:jaka jest|jaki jest|podaj|ile wynosi) (?<name>.+)$");
        if (get.Success && Resolve(get.Groups["name"].Value) is { } getParam)
            return new(CommandIntent.LightroomGetAdjustment, getParam);

        var reset = Regex.Match(value, @"^(?:wyzeruj|resetuj) (?<name>.+)$");
        if (reset.Success && Resolve(reset.Groups["name"].Value) is { } resetParam)
            return new(CommandIntent.LightroomSetAdjustment, Pack(resetParam, 0));

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
