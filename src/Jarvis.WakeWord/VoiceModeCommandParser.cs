using System.Globalization;
using System.Text;

namespace Jarvis.WakeWord;

public enum VoiceModeCommand { None, EnableContinuous, Sleep }

public static class VoiceModeCommandParser
{
    public static VoiceModeCommand Parse(string? text)
    {
        var value = Normalize(text ?? string.Empty);
        if (value is "obudz sie" or "badz aktywny" or "zostan aktywny" or
            "nie zasypiaj" or "sluchaj caly czas" or "tryb ciagly" ||
            IsWakeApproximation(value))
            return VoiceModeCommand.EnableContinuous;

        if (value is "idz spac" or "spij" or "zasnij" or "mozesz spac" or
            "wylacz tryb ciagly")
            return VoiceModeCommand.Sleep;

        return VoiceModeCommand.None;
    }

    private static bool IsWakeApproximation(string value)
    {
        var tokens = value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (tokens.Length is < 1 or > 2) return false;
        if (tokens.Length == 2 && tokens[1] != "sie") return false;

        var token = tokens[0];        if (token == "obroz") return true; // observed Polish ASR confusion for "obudź"
        if (token.Length is < 4 or > 6 || !token.StartsWith("ob", StringComparison.Ordinal))
            return false;

        return Similarity(token, "obudz") >= 0.72;
    }

    private static double Similarity(string a, string b)
    {
        var prev = Enumerable.Range(0, b.Length + 1).ToArray();
        var cur = new int[b.Length + 1];
        for (var i = 1; i <= a.Length; i++)
        {
            cur[0] = i;
            for (var j = 1; j <= b.Length; j++)
                cur[j] = Math.Min(Math.Min(cur[j - 1] + 1, prev[j] + 1),
                    prev[j - 1] + (a[i - 1] == b[j - 1] ? 0 : 1));
            (prev, cur) = (cur, prev);
        }
        return 1.0 - prev[b.Length] / (double)Math.Max(a.Length, b.Length);
    }

    private static string Normalize(string value)
    {
        var d = value.ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(d.Length);
        foreach (var ch in d)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(ch) == UnicodeCategory.NonSpacingMark) continue;
            var c = ch == 'ł' ? 'l' : ch;
            sb.Append(char.IsLetterOrDigit(c) ? c : ' ');
        }
        return string.Join(' ', sb.ToString().Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }
}