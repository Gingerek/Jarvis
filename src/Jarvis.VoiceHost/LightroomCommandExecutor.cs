using System.Globalization;
using Jarvis.Commands;
using Jarvis.Lightroom;

namespace Jarvis.VoiceHost;

internal sealed class LightroomCommandExecutor
{
    private readonly LightroomBridgeClient _client;

    private static readonly Dictionary<string, string> Labels = new(StringComparer.Ordinal)
    {
        ["Exposure2012"] = "ekspozycja",
        ["Contrast2012"] = "kontrast",
        ["Highlights2012"] = "światła",
        ["Shadows2012"] = "cienie",
        ["Whites2012"] = "biele",
        ["Blacks2012"] = "czernie",
        ["Texture"] = "tekstura",
        ["Clarity2012"] = "przejrzystość",
        ["Dehaze"] = "odmglenie",
        ["Vibrance"] = "wibracja",
        ["Saturation"] = "nasycenie",
        ["Temperature"] = "temperatura",
        ["Tint"] = "odcień"
    };

    private static string SetLabel(string parameter) => parameter switch
    {
        "Exposure2012" => "ekspozycję",
        "Texture" => "teksturę",
        "Vibrance" => "wibrację",
        "Temperature" => "temperaturę",
        _ => Label(parameter)
    };

    private static string Speak(string? value) => (value ?? string.Empty).Replace('.', ',');

    public LightroomCommandExecutor(LightroomBridgeClient client) => _client = client;

    public async Task<CommandExecutionOutcome?> ExecuteAsync(
        CommandRequest request,
        CancellationToken cancellationToken)
    {
        return request.Intent switch
        {
            CommandIntent.LightroomGetAdjustment => await GetAsync(request.Argument!, cancellationToken),
            CommandIntent.LightroomSetAdjustment => await SetAsync(request.Argument!, cancellationToken),
            CommandIntent.LightroomAdjustAdjustment => await AdjustAsync(request.Argument!, cancellationToken),
            CommandIntent.LightroomAutoTone => await ActionAsync("auto_tone", "Włączam Auto Tone w Lightroomie.", "LightroomAutoTone", cancellationToken),
            CommandIntent.LightroomAutoWhiteBalance => await ActionAsync("auto_wb", "Ustawiam automatyczny balans bieli.", "LightroomAutoWhiteBalance", cancellationToken),
            CommandIntent.LightroomNextPhoto => await ActionAsync("selection_next", "Przechodzę do następnego zdjęcia.", "LightroomNextPhoto", cancellationToken),
            CommandIntent.LightroomPreviousPhoto => await ActionAsync("selection_previous", "Wracam do poprzedniego zdjęcia.", "LightroomPreviousPhoto", cancellationToken),
            CommandIntent.LightroomGetRating => await GetRatingAsync(cancellationToken),
            CommandIntent.LightroomSetRating => await SetRatingAsync(request.Argument!, cancellationToken),
            CommandIntent.LightroomIncreaseRating => await RatingStepAsync("rating_up", cancellationToken),
            CommandIntent.LightroomDecreaseRating => await RatingStepAsync("rating_down", cancellationToken),
            CommandIntent.LightroomGetFlag => await GetFlagAsync(cancellationToken),
            CommandIntent.LightroomFlagPick => await ActionAsync("flag_pick", "Oznaczam zdjęcie jako wybrane.", "LightroomFlagPick", cancellationToken),
            CommandIntent.LightroomFlagReject => await ActionAsync("flag_reject", "Oznaczam zdjęcie jako odrzucone.", "LightroomFlagReject", cancellationToken),
            CommandIntent.LightroomClearFlag => await ActionAsync("flag_clear", "Usuwam flagę ze zdjęcia.", "LightroomClearFlag", cancellationToken),
            CommandIntent.LightroomCopyDevelopSettings => await ActionAsync("develop_copy_settings", "Kopiuję ustawienia obróbki.", "LightroomCopyDevelopSettings", cancellationToken),
            CommandIntent.LightroomPasteDevelopSettings => await ActionAsync("develop_paste_settings", "Wklejam skopiowane ustawienia obróbki.", "LightroomPasteDevelopSettings", cancellationToken),
            CommandIntent.LightroomUndo => await ActionAsync("undo", "Cofam ostatnią zmianę w Lightroomie.", "LightroomUndo", cancellationToken),
            CommandIntent.LightroomRedo => await ActionAsync("redo", "Ponawiam ostatnią zmianę w Lightroomie.", "LightroomRedo", cancellationToken),
            _ => null
        };
    }

    private async Task<CommandExecutionOutcome> GetAsync(string parameter, CancellationToken ct)
    {
        var result = await _client.SendAsync("develop_get", parameter, cancellationToken: ct);
        if (!result.Success) return Failed(result);
        var label = Label(parameter);
        var verb = parameter is "Highlights2012" or "Shadows2012" or "Whites2012" or "Blacks2012" ? "wynoszą" : "wynosi";
        return new($"{label} {verb} {Speak(result.Value)}.", "LightroomGetAdjustment", parameter);
    }

    private async Task<CommandExecutionOutcome> SetAsync(string packed, CancellationToken ct)
    {
        var parsed = LightroomCommandParser.Unpack(packed);
        if (parsed is null) return new("Nie rozumiem wartości Lightrooma.", "LightroomBadValue");
        var (parameter, value) = parsed.Value;
        var result = await _client.SendAsync("develop_set", parameter,
            value.ToString("0.####", CultureInfo.InvariantCulture), ct);
        if (!result.Success) return Failed(result);
        return new($"Ustawiam {SetLabel(parameter)} na {Speak(result.Value)}.",
            "LightroomSetAdjustment", parameter);
    }

    private async Task<CommandExecutionOutcome> AdjustAsync(string packed, CancellationToken ct)
    {
        var parsed = LightroomCommandParser.Unpack(packed);
        if (parsed is null) return new("Nie rozumiem zmiany Lightrooma.", "LightroomBadDelta");
        var (parameter, delta) = parsed.Value;
        var current = await _client.SendAsync("develop_get", parameter, cancellationToken: ct);
        if (!current.Success || !double.TryParse(current.Value,
            NumberStyles.Float, CultureInfo.InvariantCulture, out var currentValue))
            return Failed(current);

        var target = currentValue + delta;
        var set = await _client.SendAsync("develop_set", parameter,
            target.ToString("0.####", CultureInfo.InvariantCulture), ct);
        if (!set.Success) return Failed(set);
        return new($"Zmieniam {SetLabel(parameter)} z {Speak(current.Value)} na {Speak(set.Value)}.",
            "LightroomAdjustAdjustment", parameter);
    }

    private async Task<CommandExecutionOutcome> GetRatingAsync(CancellationToken ct)
    {
        var result = await _client.SendAsync("rating_get", cancellationToken: ct);
        if (!result.Success) return Failed(result);
        return new($"Ocena zdjęcia: {Speak(result.Value)} z 5.", "LightroomGetRating");
    }

    private async Task<CommandExecutionOutcome> SetRatingAsync(string rating, CancellationToken ct)
    {
        var result = await _client.SendAsync("rating_set", rating, cancellationToken: ct);
        if (!result.Success) return Failed(result);
        return new($"Ustawiam ocenę na {Speak(result.Value)} z 5.", "LightroomSetRating", result.Value);
    }

    private async Task<CommandExecutionOutcome> RatingStepAsync(string command, CancellationToken ct)
    {
        var result = await _client.SendAsync(command, cancellationToken: ct);
        if (!result.Success) return Failed(result);
        return new($"Ocena zdjęcia: {Speak(result.Value)} z 5.", "LightroomRatingStep", result.Value);
    }

    private async Task<CommandExecutionOutcome> GetFlagAsync(CancellationToken ct)
    {
        var result = await _client.SendAsync("flag_get", cancellationToken: ct);
        if (!result.Success) return Failed(result);
        var text = result.Value switch
        {
            "1" => "Zdjęcie jest oznaczone jako wybrane.",
            "-1" => "Zdjęcie jest oznaczone jako odrzucone.",
            _ => "Zdjęcie nie ma flagi."
        };
        return new(text, "LightroomGetFlag", result.Value);
    }

    private async Task<CommandExecutionOutcome> ActionAsync(
        string command, string reply, string status, CancellationToken ct)
    {
        var result = await _client.SendAsync(command, cancellationToken: ct);
        return result.Success ? new(reply, status) : Failed(result);
    }

    private static string Label(string parameter) =>
        Labels.TryGetValue(parameter, out var label) ? label : parameter;

    private static CommandExecutionOutcome Failed(LightroomCommandResult result) =>
        new(result.Error is null
            ? "Lightroom nie wykonał polecenia."
            : $"Lightroom: {result.Error}",
            "LightroomUnavailable");
}
