namespace Jarvis.Commands;

public sealed class BrowserCommandParser
{
    private static readonly Dictionary<string, CommandIntent> Exact = new(StringComparer.Ordinal)
    {
        ["nowa karta"] = CommandIntent.BrowserNewTab,
        ["otworz nowa karte"] = CommandIntent.BrowserNewTab,
        ["nowa zakladka"] = CommandIntent.BrowserNewTab,
        ["otworz nowa zakladke"] = CommandIntent.BrowserNewTab,
        ["zamknij karte"] = CommandIntent.BrowserCloseTab,
        ["zamknij zakladke"] = CommandIntent.BrowserCloseTab,
        ["zamknij aktywna karte"] = CommandIntent.BrowserCloseTab,
        ["nastepna karta"] = CommandIntent.BrowserNextTab,
        ["nastepna zakladka"] = CommandIntent.BrowserNextTab,
        ["poprzednia karta"] = CommandIntent.BrowserPreviousTab,
        ["poprzednia zakladka"] = CommandIntent.BrowserPreviousTab,
        ["odswiez"] = CommandIntent.BrowserReload,
        ["odswiez strone"] = CommandIntent.BrowserReload,
        ["przeladuj strone"] = CommandIntent.BrowserReload,
        ["wstecz"] = CommandIntent.BrowserBack,
        ["cofnij strone"] = CommandIntent.BrowserBack,
        ["dalej"] = CommandIntent.BrowserForward,
        ["do przodu"] = CommandIntent.BrowserForward,
        ["duplikuj karte"] = CommandIntent.BrowserDuplicateTab,
        ["duplikuj zakladke"] = CommandIntent.BrowserDuplicateTab,
        ["przewin w dol"] = CommandIntent.BrowserScrollDown,
        ["przewijaj w dol"] = CommandIntent.BrowserScrollDown,
        ["nizej"] = CommandIntent.BrowserScrollDown,
        ["przewin w gore"] = CommandIntent.BrowserScrollUp,
        ["przewijaj w gore"] = CommandIntent.BrowserScrollUp,
        ["wyzej"] = CommandIntent.BrowserScrollUp,
        ["na gore strony"] = CommandIntent.BrowserScrollTop,
        ["poczatek strony"] = CommandIntent.BrowserScrollTop,
        ["na dol strony"] = CommandIntent.BrowserScrollBottom,
        ["koniec strony"] = CommandIntent.BrowserScrollBottom,
        ["play pause"] = CommandIntent.BrowserToggleMedia,
        ["pauza"] = CommandIntent.BrowserToggleMedia,
        ["wznow odtwarzanie"] = CommandIntent.BrowserToggleMedia,
        ["zatrzymaj odtwarzanie"] = CommandIntent.BrowserToggleMedia,
        ["wycisz karte"] = CommandIntent.BrowserMuteTab,
        ["wycisz zakladke"] = CommandIntent.BrowserMuteTab,
        ["odcisz karte"] = CommandIntent.BrowserUnmuteTab,
        ["odcisz zakladke"] = CommandIntent.BrowserUnmuteTab,
        ["wlacz dzwiek karty"] = CommandIntent.BrowserUnmuteTab,
        ["wlacz dzwiek zakladki"] = CommandIntent.BrowserUnmuteTab,
        ["jaka strona jest otwarta"] = CommandIntent.BrowserContext,
        ["co mam otwarte w przegladarce"] = CommandIntent.BrowserContext,
        ["jaka jest aktywna karta"] = CommandIntent.BrowserContext,
        ["jaka jest aktywna zakladka"] = CommandIntent.BrowserContext
    };

    public CommandRequest? Parse(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        var normalized = CommandRegistry.Normalize(text);
        return Exact.TryGetValue(normalized, out var intent)
            ? new CommandRequest(intent)
            : null;
    }
}
