using System.Text.RegularExpressions;

namespace Jarvis.Commands;

public sealed class SystemCommandParser
{
    private static readonly Dictionary<string, CommandIntent> Exact = new(StringComparer.Ordinal)
    {
        ["glosniej"] = CommandIntent.VolumeUp, ["podglos"] = CommandIntent.VolumeUp,
        ["zwieksz glosnosc"] = CommandIntent.VolumeUp, ["podnies glosnosc"] = CommandIntent.VolumeUp,
        ["ciszej"] = CommandIntent.VolumeDown, ["scisz"] = CommandIntent.VolumeDown,
        ["zmniejsz glosnosc"] = CommandIntent.VolumeDown, ["obniz glosnosc"] = CommandIntent.VolumeDown,
        ["wycisz"] = CommandIntent.Mute, ["wycisz dzwiek"] = CommandIntent.Mute,
        ["wylacz dzwiek"] = CommandIntent.Mute, ["mute"] = CommandIntent.Mute,
        ["odcisz"] = CommandIntent.Unmute, ["wlacz dzwiek"] = CommandIntent.Unmute,
        ["przywroc dzwiek"] = CommandIntent.Unmute, ["unmute"] = CommandIntent.Unmute,
        ["jaka jest glosnosc"] = CommandIntent.GetVolume, ["ile jest glosnosci"] = CommandIntent.GetVolume,
        ["podaj glosnosc"] = CommandIntent.GetVolume, ["stan glosnosci"] = CommandIntent.GetVolume,
        ["ile mam baterii"] = CommandIntent.GetBattery, ["stan baterii"] = CommandIntent.GetBattery,
        ["jaki jest poziom baterii"] = CommandIntent.GetBattery, ["poziom baterii"] = CommandIntent.GetBattery,
        ["ile miejsca na dysku"] = CommandIntent.GetDiskSpace, ["wolne miejsce na dysku"] = CommandIntent.GetDiskSpace,
        ["ile mam wolnego miejsca"] = CommandIntent.GetDiskSpace, ["stan dysku"] = CommandIntent.GetDiskSpace,
        ["jak dlugo komputer dziala"] = CommandIntent.GetUptime, ["czas pracy komputera"] = CommandIntent.GetUptime,
        ["uptime"] = CommandIntent.GetUptime, ["jakie okno jest aktywne"] = CommandIntent.GetActiveWindow,
        ["co mam teraz otwarte"] = CommandIntent.GetActiveWindow, ["aktywne okno"] = CommandIntent.GetActiveWindow,
        ["nastepne okno"] = CommandIntent.SwitchWindow, ["przelacz okno"] = CommandIntent.SwitchWindow,
        ["alt tab"] = CommandIntent.SwitchWindow, ["play pause"] = CommandIntent.MediaPlayPause,
        ["pauza muzyki"] = CommandIntent.MediaPlayPause, ["wznow muzyke"] = CommandIntent.MediaPlayPause,
        ["nastepny utwor"] = CommandIntent.MediaNextTrack, ["nastepna piosenka"] = CommandIntent.MediaNextTrack,
        ["poprzedni utwor"] = CommandIntent.MediaPreviousTrack, ["poprzednia piosenka"] = CommandIntent.MediaPreviousTrack,
    };
    private static readonly Dictionary<string, CommandIntent> Window = new(StringComparer.Ordinal)
    {
        ["minimalizuj okno"] = CommandIntent.MinimizeWindow, ["zminimalizuj okno"] = CommandIntent.MinimizeWindow,
        ["schowaj okno"] = CommandIntent.MinimizeWindow, ["maksymalizuj okno"] = CommandIntent.MaximizeWindow,
        ["zmaksymalizuj okno"] = CommandIntent.MaximizeWindow, ["powieksz okno"] = CommandIntent.MaximizeWindow,
        ["przywroc okno"] = CommandIntent.RestoreWindow, ["normalne okno"] = CommandIntent.RestoreWindow,
        ["zamknij okno"] = CommandIntent.CloseWindow, ["zamknij aktywne okno"] = CommandIntent.CloseWindow,
        ["pokaz pulpit"] = CommandIntent.ShowDesktop, ["przejdz na pulpit"] = CommandIntent.ShowDesktop,
        ["zablokuj komputer"] = CommandIntent.LockComputer, ["zablokuj ekran"] = CommandIntent.LockComputer,
    };
    private static readonly Dictionary<string, string> Shortcuts = new(StringComparer.Ordinal)
    {
        ["kopiuj"]="copy", ["skopiuj"]="copy", ["wklej"]="paste", ["wytnij"]="cut",
        ["cofnij"]="undo", ["ponow"]="redo", ["zapisz"]="save", ["zapisz plik"]="save",
        ["zaznacz wszystko"]="select_all", ["znajdz"]="find", ["szukaj w dokumencie"]="find",
        ["nowy dokument"]="new", ["drukuj"]="print", ["escape"]="escape", ["nacisnij escape"]="escape",
        ["enter"]="enter", ["nacisnij enter"]="enter", ["delete"]="delete", ["usun"]="delete"
    };

    public CommandRequest? Parse(string normalized)
    {
        if (Exact.TryGetValue(normalized, out var exactIntent)) return new(exactIntent);
        if (Window.TryGetValue(normalized, out var windowIntent)) return new(windowIntent);
        if (Shortcuts.TryGetValue(normalized, out var shortcut)) return new(CommandIntent.KeyboardShortcut, shortcut);
        foreach (var prefix in new[] { "wpisz ", "napisz ", "wprowadz " })
            if (normalized.StartsWith(prefix, StringComparison.Ordinal) && normalized.Length > prefix.Length)
                return new(CommandIntent.TypeText, normalized[prefix.Length..].Trim());
        var match = Regex.Match(normalized, @"^(?:ustaw )?(?:glosnosc|dzwiek)(?: na)? (?<value>\d{1,3})(?: procent)?$");
        if (match.Success && int.TryParse(match.Groups["value"].Value, out var value))
            return new(CommandIntent.SetVolume, Math.Clamp(value, 0, 100).ToString());
        return null;
    }
}
