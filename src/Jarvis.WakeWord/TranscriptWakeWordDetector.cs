using System.Globalization;
using System.Text;

namespace Jarvis.WakeWord;

public sealed record WakeWordMatch(string Heard, string Alias, double Score);

public sealed class TranscriptWakeWordDetector
{
    private readonly string[] _aliases;

    public TranscriptWakeWordDetector(params string[] aliases)
    {
        _aliases = (aliases.Length == 0 ? ["Jarvis"] : aliases)
            .Select(Normalize).Where(x => x.Length > 0).Distinct().ToArray();
    }

    public WakeWordMatch? Detect(string transcript, double threshold = 0.70)
    {
        var tokens = transcript.Split([' ', ',', '.', '!', '?', ';', ':', '-'],
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        WakeWordMatch? best = null;
        foreach (var raw in tokens)
        foreach (var alias in _aliases)
        {
            var heard = Normalize(raw);
            var score = Similarity(heard, alias);
            if (best is null || score > best.Score) best = new WakeWordMatch(raw, alias, score);
        }
        return best is { Score: var bestScore } && bestScore >= threshold ? best : null;
    }
    private static string Normalize(string value)
    {
        var d = value.ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(d.Length);
        foreach (var c in d)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark) continue;
            if (char.IsLetterOrDigit(c)) sb.Append(c);
        }
        return sb.ToString().Normalize(NormalizationForm.FormC);
    }

    private static double Similarity(string a, string b)
    {
        if (a == b) return 1;
        if (a.Length == 0 || b.Length == 0) return 0;
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
}

