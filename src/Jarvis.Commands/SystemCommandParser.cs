using System.Text.RegularExpressions;

namespace Jarvis.Commands;

public sealed class SystemCommandParser
{
    private static readonly Dictionary<string, CommandIntent> Exact = new(StringComparer.Ordinal)
    {
        ["glosniej"] = CommandIntent.VolumeUp,
        ["podglos"] = CommandIntent.VolumeUp,
        ["zwieksz glosnosc"] = CommandIntent.VolumeUp,
        ["podnies glosnosc"] = CommandIntent.VolumeUp,
        ["ciszej"] = CommandIntent.VolumeDown,
        ["scisz"] = CommandIntent.VolumeDown,
        ["zmniejsz glosnosc"] = CommandIntent.VolumeDown,
        ["obniz glosnosc"] = CommandIntent.VolumeDown,
        ["wycisz"] = CommandIntent.Mute,
        ["wycisz dzwiek"] = CommandIntent.Mute,
        ["wylacz dzwiek"] = CommandIntent.Mute,
        ["mute"] = CommandIntent.Mute,
        ["odcisz"] = CommandIntent.Unmute,
        ["wlacz dzwiek"] = CommandIntent.Unmute,
        ["przywroc dzwiek"] = CommandIntent.Unmute,
        ["unmute"] = CommandIntent.Unmute,
    };
    private static readonly Dictionary<string, CommandIntent> Window = new(StringComparer.Ordinal)
    {
        ["minimalizuj okno"] = CommandIntent.MinimizeWindow,
        ["zminimalizuj okno"] = CommandIntent.MinimizeWindow,
        ["schowaj okno"] = CommandIntent.MinimizeWindow,
        ["maksymalizuj okno"] = CommandIntent.MaximizeWindow,
        ["zmaksymalizuj okno"] = CommandIntent.MaximizeWindow,
        ["powieksz okno"] = CommandIntent.MaximizeWindow,
        ["przywroc okno"] = CommandIntent.RestoreWindow,
        ["normalne okno"] = CommandIntent.RestoreWindow,
        ["zamknij okno"] = CommandIntent.CloseWindow,
        ["zamknij aktywne okno"] = CommandIntent.CloseWindow,
        ["pokaż pulpit"] = CommandIntent.ShowDesktop,
        ["pokaz pulpit"] = CommandIntent.ShowDesktop,
        ["przejdz na pulpit"] = CommandIntent.ShowDesktop,
        ["zablokuj komputer"] = CommandIntent.LockComputer,
        ["zablokuj ekran"] = CommandIntent.LockComputer,
        ["blokada komputera"] = CommandIntent.LockComputer,
    };

    public CommandRequest? Parse(string normalized)
    {
        if (Exact.TryGetValue(normalized, out var exactIntent))
            return new(exactIntent);
        if (Window.TryGetValue(normalized, out var windowIntent))
            return new(windowIntent);

        var match = Regex.Match(normalized,
            @"^(?:ustaw )?(?:glosnosc|dzwiek)(?: na)? (?<value>\d{1,3})(?: procent)?$");
        if (match.Success && int.TryParse(match.Groups["value"].Value, out var value))
            return new(CommandIntent.SetVolume, Math.Clamp(value, 0, 100).ToString());

        return null;
    }
}
