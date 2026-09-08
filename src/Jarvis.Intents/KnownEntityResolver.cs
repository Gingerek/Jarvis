using System.Globalization;
using System.Text;

namespace Jarvis.Intents;

public sealed record KnownEntityMatch(string CanonicalName, string MatchedAlias, double Score);

public sealed class KnownEntityResolver
{
    private readonly List<(string Canonical, string Alias, string Normalized)> _entries = [];

    public void Add(string canonicalName, params string[] aliases)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(canonicalName);
        var all = new[] { canonicalName }.Concat(aliases ?? []);
        foreach (var alias in all.Where(x => !string.IsNullOrWhiteSpace(x)))
            _entries.Add((canonicalName, alias, Normalize(alias)));
    }

    public KnownEntityMatch? Resolve(string spoken, double minimumScore = 0.70)
    {
        var input = Normalize(spoken);
        if (input.Length == 0) return null;
        KnownEntityMatch? best = null;
        foreach (var entry in _entries)
        {
            var score = Similarity(input, entry.Normalized);
            if (best is null || score > best.Score)
                best = new KnownEntityMatch(entry.Canonical, entry.Alias, score);
        }
        return best is { Score: var s } && s >= minimumScore ? best : null;
    }
    private static string Normalize(string value)
    {
        var formD = value.ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(formD.Length);
        foreach (var ch in formD)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(ch) == UnicodeCategory.NonSpacingMark) continue;
            if (char.IsLetterOrDigit(ch)) sb.Append(ch);
        }
        return sb.ToString().Normalize(NormalizationForm.FormC);
    }

    private static double Similarity(string left, string right)
    {
        if (left == right) return 1.0;
        var distance = Levenshtein(left, right);
        return 1.0 - distance / (double)Math.Max(left.Length, right.Length);
    }

    private static int Levenshtein(string a, string b)
    {
        var previous = Enumerable.Range(0, b.Length + 1).ToArray();
        var current = new int[b.Length + 1];
        for (var i = 1; i <= a.Length; i++)
        {
            current[0] = i;
            for (var j = 1; j <= b.Length; j++)
                current[j] = Math.Min(Math.Min(current[j - 1] + 1, previous[j] + 1),
                    previous[j - 1] + (a[i - 1] == b[j - 1] ? 0 : 1));
            (previous, current) = (current, previous);
        }
        return previous[b.Length];
    }
}
