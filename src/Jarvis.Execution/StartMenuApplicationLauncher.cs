using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace Jarvis.Execution;

public sealed record DynamicApplicationLaunchResult(bool Success, string DisplayName, string? Error = null);

public sealed class StartMenuApplicationLauncher
{
    private sealed record Shortcut(string Name, string Normalized, string Path);
    private readonly Shortcut[] _shortcuts;
    private static readonly Dictionary<string, string> Aliases = new(StringComparer.Ordinal)
    {
        ["eksplorator"] = "file explorer",
        ["eksplorator plikow"] = "file explorer",
        ["panel sterowania"] = "control panel",
        ["wiersz polecen"] = "command prompt",
        ["cmd"] = "command prompt",
        ["powershell"] = "windows powershell"
    };

    public StartMenuApplicationLauncher()
    {
        var roots = new[]
        {
            Environment.GetFolderPath(Environment.SpecialFolder.StartMenu),
            Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu)
        };
        var options = new EnumerationOptions
        {
            RecurseSubdirectories = true,
            IgnoreInaccessible = true,
            AttributesToSkip = FileAttributes.ReparsePoint
        };
        _shortcuts = roots.Where(Directory.Exists)
            .SelectMany(root => Directory.EnumerateFiles(root, "*", options))
            .Where(path => path.EndsWith(".lnk", StringComparison.OrdinalIgnoreCase) ||
                           path.EndsWith(".url", StringComparison.OrdinalIgnoreCase) ||
                           path.EndsWith(".appref-ms", StringComparison.OrdinalIgnoreCase))
            .Select(path => new Shortcut(Path.GetFileNameWithoutExtension(path), Normalize(Path.GetFileNameWithoutExtension(path)), path))
            .Where(x => !x.Normalized.Contains("uninstall", StringComparison.Ordinal))
            .GroupBy(x => x.Normalized, StringComparer.Ordinal)
            .Select(g => g.First()).ToArray();
    }

    public DynamicApplicationLaunchResult TryLaunch(string requestedName)
    {
        var requested = Normalize(requestedName);
        if (Aliases.TryGetValue(requested, out var alias)) requested = alias;
        var best = _shortcuts.Select(x => (item: x, score: Score(requested, x.Normalized)))
            .OrderByDescending(x => x.score).FirstOrDefault();
        if (best.item is null || best.score < 0.72)
            return new(false, requestedName, "No matching Start Menu application.");
        try
        {
            Process.Start(new ProcessStartInfo { FileName = best.item.Path, UseShellExecute = true });
            return new(true, best.item.Name);
        }
        catch (Exception ex) { return new(false, best.item.Name, ex.Message); }
    }

    private static double Score(string a, string b)
    {
        if (a == b) return 1.0;
        if (a.Length >= 4 && (b.Contains(a, StringComparison.Ordinal) || a.Contains(b, StringComparison.Ordinal))) return 0.90;
        return Similarity(a, b);
    }

    private static double Similarity(string a, string b)
    {
        var prev = Enumerable.Range(0, b.Length + 1).ToArray(); var cur = new int[b.Length + 1];
        for (var i = 1; i <= a.Length; i++) { cur[0] = i; for (var j = 1; j <= b.Length; j++)
            cur[j] = Math.Min(Math.Min(cur[j - 1] + 1, prev[j] + 1), prev[j - 1] + (a[i - 1] == b[j - 1] ? 0 : 1)); (prev, cur) = (cur, prev); }
        return 1.0 - prev[b.Length] / (double)Math.Max(a.Length, b.Length);
    }

    private static string Normalize(string value)
    {
        var d = value.ToLowerInvariant().Normalize(NormalizationForm.FormD); var sb = new StringBuilder(d.Length);
        foreach (var ch in d) { if (CharUnicodeInfo.GetUnicodeCategory(ch) == UnicodeCategory.NonSpacingMark) continue; var c = ch == 'ł' ? 'l' : ch; sb.Append(char.IsLetterOrDigit(c) ? c : ' '); }
        return string.Join(' ', sb.ToString().Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }
}
