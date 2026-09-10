using System.Text.Json;

namespace Jarvis.Commands;

public sealed record CommandCorrection(CommandRequest Request, string Utterance, double Score);

public sealed class CommandCorpusCorrector
{
    private sealed record Entry(CommandRequest Request, string Utterance, string Normalized, string[] Tokens, string[] Numbers);
    private readonly Entry[] _entries;
    private readonly Dictionary<string, Entry> _exact;

    private CommandCorpusCorrector(IEnumerable<Entry> entries)
    {
        _entries = entries.GroupBy(x => x.Normalized, StringComparer.Ordinal)
            .Select(x => x.First()).ToArray();
        _exact = _entries.ToDictionary(x => x.Normalized, StringComparer.Ordinal);
    }

    public static CommandCorpusCorrector Load(string path)
    {
        var entries = new List<Entry>();
        if (!File.Exists(path)) return new CommandCorpusCorrector(entries);
        foreach (var line in File.ReadLines(path))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            try
            {
                using var json = JsonDocument.Parse(line);
                var root = json.RootElement;
                var utterance = root.GetProperty("utterance").GetString();
                var intentText = root.GetProperty("intent").GetString();
                if (string.IsNullOrWhiteSpace(utterance) || string.IsNullOrWhiteSpace(intentText)) continue;
                if (!Enum.TryParse<CommandIntent>(intentText, out var intent)) continue;
                var argument = root.TryGetProperty("argument", out var arg) && arg.ValueKind != JsonValueKind.Null
                    ? arg.GetString() : null;
                var normalized = CommandRegistry.Normalize(utterance);
                if (normalized.Length == 0) continue;
                var tokens = Tokens(normalized);
                entries.Add(new Entry(new CommandRequest(intent, argument), utterance, normalized, tokens, Numbers(tokens)));
            }
            catch (JsonException) { }
        }
        return new CommandCorpusCorrector(entries);
    }

    public CommandCorrection? Correct(string? text)
    {
        var normalized = CommandRegistry.Normalize(text ?? string.Empty);
        if (normalized.Length == 0) return null;
        if (_exact.TryGetValue(normalized, out var exact))
            return new CommandCorrection(exact.Request, exact.Utterance, 1.0);

        var tokens = Tokens(normalized);
        var numbers = Numbers(tokens);
        Entry? best = null;
        var bestScore = 0.0;
        foreach (var entry in _entries)
        {
            if (Math.Abs(entry.Tokens.Length - tokens.Length) > 1) continue;
            if (Math.Abs(entry.Normalized.Length - normalized.Length) > Math.Max(4, normalized.Length / 3)) continue;
            if (!entry.Numbers.SequenceEqual(numbers)) continue;
            var score = Similarity(normalized, entry.Normalized);
            if (score <= bestScore) continue;
            bestScore = score;
            best = entry;
        }

        var threshold = normalized.Length <= 8 ? 0.88 : normalized.Length <= 14 ? 0.82 : 0.78;
        return best is not null && bestScore >= threshold
            ? new CommandCorrection(best.Request, best.Utterance, bestScore)
            : null;
    }

    private static string[] Tokens(string value) => value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
    private static string[] Numbers(string[] tokens) => tokens.Where(x => x.All(char.IsDigit)).ToArray();

    private static double Similarity(string a, string b)
    {
        if (a == b) return 1.0;
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
