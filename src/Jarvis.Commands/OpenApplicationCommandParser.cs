using System.Globalization;
using System.Text;

namespace Jarvis.Commands;

public sealed record OpenApplicationCommand(string RequestedName);

public sealed class OpenApplicationCommandParser
{
    // Includes empirically observed ASR confusions from the real Nor-Tec/Whisper pipeline.
    private static readonly string[] Verbs =
        ["otworz", "otworzyc", "wlacz", "wlaczyc", "uruchom", "uruchomic", "odpal", "odpalic", "startuj", "start", "open", "rozom"];

    public OpenApplicationCommand? Parse(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        var normalized = Normalize(text);
        foreach (var verb in Verbs)
        {
            if (!normalized.StartsWith(verb + " ", StringComparison.Ordinal)) continue;
            var requested = normalized[(verb.Length + 1)..].Trim();
            if (requested.StartsWith("mi ", StringComparison.Ordinal)) requested = requested[3..].Trim();
            if (requested.StartsWith("dla mnie ", StringComparison.Ordinal)) requested = requested[9..].Trim();
            return requested.Length == 0 ? null : new OpenApplicationCommand(requested);
        }
        return null;
    }
    private static string Normalize(string value)
    {
        var formD = value.ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(formD.Length);
        foreach (var ch in formD)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(ch) == UnicodeCategory.NonSpacingMark) continue;
            var normalizedChar = ch == 'ł' ? 'l' : ch;
            sb.Append(char.IsLetterOrDigit(normalizedChar) ? normalizedChar : ' ');
        }
        return string.Join(' ', sb.ToString()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
    }
}
