using System.Text.Json;
using Jarvis.Commands;
using Jarvis.OBS;

namespace Jarvis.VoiceHost;

internal sealed class ObsCommandExecutor
{
    private readonly ObsWebSocketClient _client;

    public ObsCommandExecutor(ObsWebSocketClient client) => _client = client;

    public async Task<CommandExecutionOutcome?> ExecuteAsync(
        CommandRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return request.Intent switch
            {
                CommandIntent.ObsStatus => await StatusAsync(cancellationToken),
                CommandIntent.ObsRecordStatus => await RecordStatusAsync(cancellationToken),
                CommandIntent.ObsStreamStatus => await StreamStatusAsync(cancellationToken),
                CommandIntent.ObsCurrentScene => await CurrentSceneAsync(cancellationToken),
                CommandIntent.ObsListScenes => await SceneListAsync(cancellationToken),
                CommandIntent.ObsStartRecord => await ActionAsync("StartRecord", "Rozpoczynam nagrywanie.", "ObsStartRecord", cancellationToken),
                CommandIntent.ObsStopRecord => await ActionAsync("StopRecord", "Zatrzymuję nagrywanie.", "ObsStopRecord", cancellationToken),
                CommandIntent.ObsStartStream => await ActionAsync("StartStream", "Rozpoczynam transmisję.", "ObsStartStream", cancellationToken),
                CommandIntent.ObsStopStream => await ActionAsync("StopStream", "Zatrzymuję transmisję.", "ObsStopStream", cancellationToken),
                CommandIntent.ObsPauseRecord => await ActionAsync("PauseRecord", "Wstrzymuję nagrywanie.", "ObsPauseRecord", cancellationToken),
                CommandIntent.ObsResumeRecord => await ActionAsync("ResumeRecord", "Wznawiam nagrywanie.", "ObsResumeRecord", cancellationToken),
                CommandIntent.ObsListInputs => await ListInputsAsync(cancellationToken),
                CommandIntent.ObsMuteInput => await SetInputMuteAsync(request.Argument!, true, cancellationToken),
                CommandIntent.ObsUnmuteInput => await SetInputMuteAsync(request.Argument!, false, cancellationToken),
                CommandIntent.ObsShowSource => await SetSourceVisibilityAsync(request.Argument!, true, cancellationToken),
                CommandIntent.ObsHideSource => await SetSourceVisibilityAsync(request.Argument!, false, cancellationToken),
                CommandIntent.ObsSetScene => await SetSceneAsync(request.Argument!, cancellationToken),
                _ => null
            };
        }
        catch (Exception)
        {
            return new("OBS nie odpowiada. Sprawdź, czy jest uruchomiony.", "ObsUnavailable");
        }
    }

    private async Task<CommandExecutionOutcome> StatusAsync(CancellationToken ct)
    {
        var record = await _client.RequestAsync("GetRecordStatus", cancellationToken: ct);
        var stream = await _client.RequestAsync("GetStreamStatus", cancellationToken: ct);
        var scene = await _client.RequestAsync("GetCurrentProgramScene", cancellationToken: ct);
        if (!record.Success || !stream.Success || !scene.Success) return Failed(record, stream, scene);
        var recording = Bool(record.Data, "outputActive");
        var streaming = Bool(stream.Data, "outputActive");
        var sceneName = String(scene.Data, "sceneName") ?? "nieznana";
        return new($"OBS działa. Scena {sceneName}. Nagrywanie {(recording ? "włączone" : "wyłączone")}, stream {(streaming ? "włączony" : "wyłączony")}.", "ObsStatus", sceneName);
    }

    private async Task<CommandExecutionOutcome> RecordStatusAsync(CancellationToken ct)
    {
        var result = await _client.RequestAsync("GetRecordStatus", cancellationToken: ct);
        if (!result.Success) return Failed(result);
        var active = Bool(result.Data, "outputActive");
        return new(active ? "OBS teraz nagrywa." : "OBS teraz nie nagrywa.", "ObsRecordStatus");
    }

    private async Task<CommandExecutionOutcome> StreamStatusAsync(CancellationToken ct)
    {
        var result = await _client.RequestAsync("GetStreamStatus", cancellationToken: ct);
        if (!result.Success) return Failed(result);
        var active = Bool(result.Data, "outputActive");
        return new(active ? "Transmisja jest aktywna." : "Transmisja jest wyłączona.", "ObsStreamStatus");
    }

    private async Task<CommandExecutionOutcome> CurrentSceneAsync(CancellationToken ct)
    {
        var result = await _client.RequestAsync("GetCurrentProgramScene", cancellationToken: ct);
        if (!result.Success) return Failed(result);
        var scene = String(result.Data, "sceneName") ?? "nieznana";
        return new($"Aktywna scena to {scene}.", "ObsCurrentScene", scene);
    }

    private async Task<CommandExecutionOutcome> SceneListAsync(CancellationToken ct)
    {
        var result = await _client.RequestAsync("GetSceneList", cancellationToken: ct);
        if (!result.Success) return Failed(result);
        var scenes = result.Data?.GetProperty("scenes").EnumerateArray()
            .Select(x => x.GetProperty("sceneName").GetString())
            .Where(x => !string.IsNullOrWhiteSpace(x)).Cast<string>().ToArray() ?? [];
        if (scenes.Length == 0) return new("Nie znalazłem żadnych scen w OBS.", "ObsListScenes");
        var spoken = scenes.Length <= 5 ? string.Join(", ", scenes) : $"{scenes.Length} scen";
        return new($"Sceny OBS: {spoken}.", "ObsListScenes", scenes.Length.ToString());
    }

    private async Task<CommandExecutionOutcome> SetSceneAsync(string requested, CancellationToken ct)
    {
        var list = await _client.RequestAsync("GetSceneList", cancellationToken: ct);
        if (!list.Success) return Failed(list);
        var scenes = list.Data?.GetProperty("scenes").EnumerateArray()
            .Select(x => x.GetProperty("sceneName").GetString())
            .Where(x => !string.IsNullOrWhiteSpace(x)).Cast<string>().ToArray() ?? [];
        var target = scenes.FirstOrDefault(x => CommandRegistry.Normalize(x) == CommandRegistry.Normalize(requested));
        if (target is null)
            return new($"Nie znalazłem sceny {requested}.", "ObsSceneNotFound", requested);

        var result = await _client.RequestAsync(
            "SetCurrentProgramScene",
            new { sceneName = target },
            ct);
        return result.Success
            ? new($"Przełączam na scenę {target}.", "ObsSetScene", target)
            : Failed(result);
    }

    private async Task<CommandExecutionOutcome> ListInputsAsync(CancellationToken ct)
    {
        var result = await _client.RequestAsync("GetInputList", cancellationToken: ct);
        if (!result.Success) return Failed(result);
        var inputs = result.Data?.GetProperty("inputs").EnumerateArray()
            .Select(x => x.GetProperty("inputName").GetString())
            .Where(x => !string.IsNullOrWhiteSpace(x)).Cast<string>().ToArray() ?? [];
        if (inputs.Length == 0) return new("Nie znalazłem wejść w OBS.", "ObsListInputs");
        var spoken = inputs.Length <= 5 ? string.Join(", ", inputs) : $"{inputs.Length} wejść";
        return new($"Wejścia OBS: {spoken}.", "ObsListInputs", inputs.Length.ToString());
    }

    private async Task<CommandExecutionOutcome> SetInputMuteAsync(string requested, bool muted, CancellationToken ct)
    {
        var list = await _client.RequestAsync("GetInputList", cancellationToken: ct);
        if (!list.Success) return Failed(list);
        var names = list.Data?.GetProperty("inputs").EnumerateArray()
            .Select(x => x.GetProperty("inputName").GetString())
            .Where(x => !string.IsNullOrWhiteSpace(x)).Cast<string>().ToArray() ?? [];
        var target = ResolveName(names, requested);
        if (target is null) return new($"Nie znalazłem wejścia {requested} w OBS.", "ObsInputNotFound", requested);
        var result = await _client.RequestAsync("SetInputMute", new { inputName = target, inputMuted = muted }, ct);
        return result.Success
            ? new(muted ? $"Wyciszam {target} w OBS." : $"Odciszam {target} w OBS.", muted ? "ObsMuteInput" : "ObsUnmuteInput", target)
            : Failed(result);
    }

    private async Task<CommandExecutionOutcome> SetSourceVisibilityAsync(string requested, bool enabled, CancellationToken ct)
    {
        var sceneResult = await _client.RequestAsync("GetCurrentProgramScene", cancellationToken: ct);
        if (!sceneResult.Success) return Failed(sceneResult);
        var sceneName = String(sceneResult.Data, "sceneName")!;
        var list = await _client.RequestAsync("GetSceneItemList", new { sceneName }, ct);
        if (!list.Success) return Failed(list);
        var items = list.Data?.GetProperty("sceneItems").EnumerateArray()
            .Select(x => (Name: x.GetProperty("sourceName").GetString()!, Id: x.GetProperty("sceneItemId").GetInt32()))
            .Where(x => !string.IsNullOrWhiteSpace(x.Name)).ToArray() ?? [];
        var targetName = ResolveName(items.Select(x => x.Name), requested);
        if (targetName is null) return new($"Nie znalazłem źródła {requested} w aktywnej scenie.", "ObsSourceNotFound", requested);
        var item = items.First(x => x.Name == targetName);
        var result = await _client.RequestAsync("SetSceneItemEnabled", new { sceneName, sceneItemId = item.Id, sceneItemEnabled = enabled }, ct);
        return result.Success
            ? new(enabled ? $"Pokazuję źródło {targetName}." : $"Ukrywam źródło {targetName}.", enabled ? "ObsShowSource" : "ObsHideSource", targetName)
            : Failed(result);
    }

    private static string? ResolveName(IEnumerable<string> candidates, string requested)
    {
        var names = candidates.ToArray();
        var key = CommandRegistry.Normalize(requested);
        var exact = names.FirstOrDefault(x => CommandRegistry.Normalize(x) == key);
        if (exact is not null) return exact;
        var contains = names.Where(x => CommandRegistry.Normalize(x).Contains(key, StringComparison.Ordinal)).ToArray();
        if (contains.Length == 1) return contains[0];
        if (!key.StartsWith("kamera", StringComparison.Ordinal)) return null;
        var suffix = key["kamera".Length..].Trim();
        var video = names.Where(x => CommandRegistry.Normalize(x).Contains("przechwytywania wideo", StringComparison.Ordinal)).ToArray();
        if (suffix.Length == 0) return video.Length == 1 ? video[0] : null;
        if (suffix == "1")
            return video.FirstOrDefault(x => CommandRegistry.Normalize(x).EndsWith("przechwytywania wideo", StringComparison.Ordinal));
        return video.FirstOrDefault(x => CommandRegistry.Normalize(x).EndsWith(" " + suffix, StringComparison.Ordinal));
    }

    private async Task<CommandExecutionOutcome> ActionAsync(
        string requestType, string reply, string status, CancellationToken ct)
    {
        var result = await _client.RequestAsync(requestType, cancellationToken: ct);
        return result.Success ? new(reply, status) : Failed(result);
    }

    private static bool Bool(JsonElement? data, string name) =>
        data is { } value && value.TryGetProperty(name, out var p) && p.GetBoolean();

    private static string? String(JsonElement? data, string name) =>
        data is { } value && value.TryGetProperty(name, out var p) ? p.GetString() : null;

    private static CommandExecutionOutcome Failed(params ObsRequestResult[] results)
    {
        var error = results.FirstOrDefault(x => !x.Success)?.Error;
        return new(error?.Contains("unavailable", StringComparison.OrdinalIgnoreCase) == true
            ? "OBS nie jest uruchomiony albo serwer WebSocket jest wyłączony."
            : error is null ? "OBS nie wykonał polecenia." : $"OBS: {error}", "ObsFailed");
    }
}
