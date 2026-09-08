namespace Jarvis.Commands;

public sealed class KnownFolderCommandParser
{
    private static readonly Dictionary<string, string> Aliases = new(StringComparer.Ordinal)
    {
        ["pobrane"] = "downloads",
        ["pobierane"] = "downloads",
        ["downloads"] = "downloads",
        ["dokumenty"] = "documents",
        ["moje dokumenty"] = "documents",
        ["documents"] = "documents",
        ["zdjecia"] = "pictures",
        ["obrazy"] = "pictures",
        ["pictures"] = "pictures",
        ["muzyka"] = "music",
        ["music"] = "music",
        ["wideo"] = "videos",
        ["filmy"] = "videos",
        ["videos"] = "videos",
        ["pulpit"] = "desktop",
        ["desktop"] = "desktop",
        ["jarvis"] = "jarvis",
        ["folder jarvis"] = "jarvis",
        ["projekt jarvis"] = "jarvis"
    };

    private static readonly string[] Prefixes =
        ["otworz ", "otworzyc ", "pokaz ", "pokaz mi ", "przejdz do ", "wejdz do "];
    public CommandRequest? Parse(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        var normalized = CommandRegistry.Normalize(text);

        foreach (var prefix in Prefixes)
        {
            if (!normalized.StartsWith(prefix, StringComparison.Ordinal)) continue;
            var target = normalized[prefix.Length..].Trim();
            if (target.StartsWith("folder ", StringComparison.Ordinal))
                target = target[7..].Trim();

            if (Aliases.TryGetValue(target, out var key))
                return new CommandRequest(CommandIntent.OpenFolder, key);
        }

        return null;
    }
}
