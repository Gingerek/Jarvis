using System.Text.Json;
using Jarvis.Browser;
using Jarvis.Commands;

namespace Jarvis.VoiceHost;

internal sealed class BrowserCommandExecutor
{
    private readonly BrowserCommandClient _client;

    public BrowserCommandExecutor(BrowserCommandClient client)
    {
        _client = client;
    }

    public async Task<CommandExecutionOutcome?> ExecuteAsync(
        CommandRequest request,
        CancellationToken cancellationToken)
    {
        var action = ActionFor(request.Intent);
        if (action is null) return null;

        var result = await _client.SendAsync(action, request.Argument, cancellationToken);
        if (!result.Success)
            return new("Przeglądarka nie jest połączona z Jarvisem.", "BrowserUnavailable");

        if (request.Intent == CommandIntent.BrowserContext)
            return ContextOutcome(result.Data);
        if (request.Intent == CommandIntent.BrowserToggleMedia)
            return MediaOutcome(result.Data);

        return new(ReplyFor(request.Intent), request.Intent.ToString());
    }

    private static string? ActionFor(CommandIntent intent) => intent switch
    {
        CommandIntent.BrowserNewTab => "new_tab",
        CommandIntent.BrowserCloseTab => "close_tab",
        CommandIntent.BrowserNextTab => "next_tab",
        CommandIntent.BrowserPreviousTab => "previous_tab",
        CommandIntent.BrowserReload => "reload",
        CommandIntent.BrowserBack => "back",
        CommandIntent.BrowserForward => "forward",
        CommandIntent.BrowserDuplicateTab => "duplicate_tab",
        CommandIntent.BrowserScrollDown => "scroll_down",
        CommandIntent.BrowserScrollUp => "scroll_up",
        CommandIntent.BrowserScrollTop => "scroll_top",
        CommandIntent.BrowserScrollBottom => "scroll_bottom",
        CommandIntent.BrowserToggleMedia => "toggle_media",
        CommandIntent.BrowserMuteTab => "mute_tab",
        CommandIntent.BrowserUnmuteTab => "unmute_tab",
        CommandIntent.BrowserContext => "context",
        _ => null
    };

    private static string ReplyFor(CommandIntent intent) => intent switch
    {
        CommandIntent.BrowserNewTab => "Otwieram nową kartę.",
        CommandIntent.BrowserCloseTab => "Zamykam kartę.",
        CommandIntent.BrowserNextTab => "Przechodzę do następnej karty.",
        CommandIntent.BrowserPreviousTab => "Przechodzę do poprzedniej karty.",
        CommandIntent.BrowserReload => "Odświeżam stronę.",
        CommandIntent.BrowserBack => "Cofam stronę.",
        CommandIntent.BrowserForward => "Przechodzę dalej.",
        CommandIntent.BrowserDuplicateTab => "Duplikuję kartę.",
        CommandIntent.BrowserScrollDown => "Przewijam w dół.",
        CommandIntent.BrowserScrollUp => "Przewijam w górę.",
        CommandIntent.BrowserScrollTop => "Przechodzę na początek strony.",
        CommandIntent.BrowserScrollBottom => "Przechodzę na koniec strony.",
        CommandIntent.BrowserMuteTab => "Wyciszam kartę.",
        CommandIntent.BrowserUnmuteTab => "Włączam dźwięk karty.",
        _ => "Gotowe."
    };

    private static CommandExecutionOutcome ContextOutcome(JsonElement? data)
    {
        if (data is not JsonElement value || value.ValueKind != JsonValueKind.Object)
            return new("Nie mogę odczytać aktywnej karty.", "BrowserContextFailed");

        var title = value.TryGetProperty("title", out var titleEl) ? titleEl.GetString() : null;
        var url = value.TryGetProperty("url", out var urlEl) ? urlEl.GetString() : null;
        if (!string.IsNullOrWhiteSpace(title))
            return new($"Aktywna karta to {title}.", "BrowserContext", title);
        if (!string.IsNullOrWhiteSpace(url))
            return new($"Aktywna strona to {url}.", "BrowserContext", url);
        return new("Nie mogę odczytać nazwy aktywnej karty.", "BrowserContextFailed");
    }

    private static CommandExecutionOutcome MediaOutcome(JsonElement? data)
    {
        if (data is JsonElement value && value.ValueKind == JsonValueKind.Object &&
            value.TryGetProperty("found", out var found) && found.GetBoolean())
            return new("Przełączam odtwarzanie.", "BrowserToggleMedia");
        return new("Nie znalazłem odtwarzacza na tej stronie.", "BrowserMediaNotFound");
    }
}
