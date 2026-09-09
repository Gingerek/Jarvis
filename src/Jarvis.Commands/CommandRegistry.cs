using System.Globalization;
using System.Text;

namespace Jarvis.Commands;

public enum CommandIntent
{
    OpenApplication,
    CloseApplication,
    OpenFolder,
    OpenSettings,
    OpenWebsite,
    SearchWeb,
    SearchYouTube,
    GetTime,
    GetDate,
    GetDayOfWeek,
    VolumeUp,
    VolumeDown,
    SetVolume,
    Mute,
    Unmute,
    MinimizeWindow,
    MaximizeWindow,
    RestoreWindow,
    CloseWindow,
    ShowDesktop,
    LockComputer,
    LightroomGetAdjustment,
    LightroomSetAdjustment,
    LightroomAdjustAdjustment,
    LightroomResetAdjustment,
    LightroomResetAllDevelop,
    LightroomResetTransforms,
    LightroomAutoTone,
    LightroomAutoWhiteBalance,
    LightroomNextPhoto,
    LightroomPreviousPhoto,
    LightroomGetRating,
    LightroomSetRating,
    LightroomIncreaseRating,
    LightroomDecreaseRating,
    LightroomGetFlag,
    LightroomFlagPick,
    LightroomFlagReject,
    LightroomClearFlag,
    LightroomGetCropAngle,
    LightroomSetCropAngle,
    LightroomResetCrop,
    LightroomGetMaskCount,
    LightroomCreateSubjectMask,
    LightroomCreateSkyMask,
    LightroomCreateBackgroundMask,
    LightroomCreateMaskComponent,
    LightroomAddMaskComponent,
    LightroomSubtractMaskComponent,
    LightroomIntersectMaskComponent,
    LightroomToggleMaskOverlay,
    LightroomResetMasks,
    LightroomGetDevelopTool,
    LightroomSelectDevelopTool,
    LightroomGetColorGradingView,
    LightroomSetColorGradingView,
    LightroomGetLensBlurBokeh,
    LightroomSetLensBlurBokeh,
    LightroomOpenRemove,
    LightroomResetRemove,
    LightroomCopyDevelopSettings,
    LightroomPasteDevelopSettings,
    LightroomUndo,
    LightroomRedo,
    BrowserNewTab,
    BrowserCloseTab,
    BrowserNextTab,
    BrowserPreviousTab,
    BrowserReload,
    BrowserBack,
    BrowserForward,
    BrowserScrollDown,
    BrowserScrollUp,
    BrowserScrollTop,
    BrowserScrollBottom,
    BrowserToggleMedia,
    BrowserMuteTab,
    BrowserUnmuteTab,
    BrowserDuplicateTab,
    BrowserContext,
    ObsStatus,
    ObsRecordStatus,
    ObsStreamStatus,
    ObsCurrentScene,
    ObsListScenes,
    ObsStartRecord,
    ObsStopRecord,
    ObsStartStream,
    ObsStopStream,
    ObsPauseRecord,
    ObsResumeRecord,
    ObsListInputs,
    ObsMuteInput,
    ObsUnmuteInput,
    ObsShowSource,
    ObsHideSource,
    ObsSetScene
}

public sealed record CommandRequest(
    CommandIntent Intent,
    string? Argument = null);

public sealed class CommandRegistry
{
    private readonly OpenApplicationCommandParser _open = new();
    private readonly CloseApplicationCommandParser _close = new();
    private readonly KnownFolderCommandParser _folder = new();
    private readonly WindowsSettingsCommandParser _settings = new();
    private readonly SystemCommandParser _system = new();
    private readonly LightroomCommandParser _lightroom = new();
    private readonly BrowserCommandParser _browser = new();
    private readonly ObsCommandParser _obs = new();
    private static readonly HashSet<string> TimePhrases = new(StringComparer.Ordinal)
    {
        "ktora godzina", "ktora jest godzina", "jaka jest godzina", "powiedz ktora godzina",
        "podaj godzine", "ile jest godzina", "jaki mamy czas"
    };

    private static readonly HashSet<string> DatePhrases = new(StringComparer.Ordinal)
    {
        "jaka jest data", "jaki dzisiaj dzien", "jaki mamy dzis dzien",
        "podaj date", "powiedz jaka jest data", "co dzisiaj za dzien"
    };

    private static readonly HashSet<string> DayPhrases = new(StringComparer.Ordinal)
    {
        "jaki dzis dzien tygodnia", "jaki mamy dzien tygodnia",
        "powiedz jaki dzis dzien tygodnia", "co dzisiaj za dzien tygodnia"
    };

    private static readonly Dictionary<string, string> Websites = new(StringComparer.Ordinal)
    {
        ["youtube"] = "https://www.youtube.com/",
        ["google"] = "https://www.google.com/",
        ["marktplaats"] = "https://www.marktplaats.nl/"
    };
    public CommandRequest? Parse(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;

        var lightroom = _lightroom.Parse(text);
        if (lightroom is not null) return lightroom;

        var normalized = Simplify(Normalize(text));

        if (TimePhrases.Contains(normalized))
            return new(CommandIntent.GetTime);
        if (DayPhrases.Contains(normalized))
            return new(CommandIntent.GetDayOfWeek);
        if (DatePhrases.Contains(normalized))
            return new(CommandIntent.GetDate);

        var system = _system.Parse(normalized);
        if (system is not null) return system;

        var obs = _obs.Parse(normalized);
        if (obs is not null) return obs;

        var browser = _browser.Parse(normalized);
        if (browser is not null) return browser;

        var settings = _settings.Parse(normalized);
        if (settings is not null) return settings;

        var folder = _folder.Parse(normalized);
        if (folder is not null) return folder;

        var searchYouTube = ExtractAfterPrefix(normalized,
            "wyszukaj na youtube ", "znajdz na youtube ", "youtube wyszukaj ", "youtube szukaj ");
        if (searchYouTube is not null)
            return new(CommandIntent.SearchYouTube, searchYouTube);

        var searchWeb = ExtractAfterPrefix(normalized,
            "wyszukaj w google ", "znajdz w google ", "wyszukaj w internecie ", "google wyszukaj ");
        if (searchWeb is not null)
            return new(CommandIntent.SearchWeb, searchWeb);
        var open = _open.Parse(normalized);
        if (open is not null)
        {
            var websiteKey = Normalize(open.RequestedName);
            if (Websites.TryGetValue(websiteKey, out var url))
                return new(CommandIntent.OpenWebsite, url);
            return new(CommandIntent.OpenApplication, open.RequestedName);
        }

        var close = _close.Parse(normalized);
        if (close is not null)
            return new(CommandIntent.CloseApplication, close.RequestedName);

        return null;
    }

    private static string Simplify(string text)
    {
        string[] prefixes = ["hej jarvis ", "jarvis ", "prosze cie ", "prosze ", "czy mozesz ", "mozesz ", "chce zebys ", "sprobuj ", "teraz ", "dobrze ", "okej "];
        string[] suffixes = [" jesli mozesz", " od razu", " prosze", " teraz"];
        var current = text;
        bool changed;
        do
        {
            changed = false;
            foreach (var prefix in prefixes)
                if (current.StartsWith(prefix, StringComparison.Ordinal)) { current = current[prefix.Length..].Trim(); changed = true; break; }
        } while (changed);
        do
        {
            changed = false;
            foreach (var suffix in suffixes)
                if (current.EndsWith(suffix, StringComparison.Ordinal)) { current = current[..^suffix.Length].Trim(); changed = true; break; }
        } while (changed);
        return current;
    }
    private static string? ExtractAfterPrefix(string text, params string[] prefixes)
    {
        foreach (var prefix in prefixes)
            if (text.StartsWith(prefix, StringComparison.Ordinal))
                return text[prefix.Length..].Trim() is { Length: > 0 } value ? value : null;
        return null;
    }
    public static string Normalize(string value)
    {
        var formD = value.ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(formD.Length);
        foreach (var ch in formD)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(ch) == UnicodeCategory.NonSpacingMark) continue;
            var c = ch == '\u0142' ? 'l' : ch;
            sb.Append(char.IsLetterOrDigit(c) ? c : ' ');
        }
        return string.Join(' ', sb.ToString()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
    }
}

public sealed record CloseApplicationCommand(string RequestedName);

public sealed class CloseApplicationCommandParser
{
    private static readonly string[] Verbs = ["zamknij", "zamknac", "wylacz", "wylaczyc", "zakoncz", "zakonczyc", "close"];
    public CloseApplicationCommand? Parse(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        var normalized = CommandRegistry.Normalize(text);
        foreach (var verb in Verbs)
        {
            if (!normalized.StartsWith(verb + " ", StringComparison.Ordinal)) continue;
            var requested = normalized[(verb.Length + 1)..].Trim();
            if (requested.StartsWith("mi ", StringComparison.Ordinal)) requested = requested[3..].Trim();
            if (requested.StartsWith("dla mnie ", StringComparison.Ordinal)) requested = requested[9..].Trim();
            return requested.Length == 0 ? null : new CloseApplicationCommand(requested);
        }
        return null;
    }
}
