namespace Jarvis.Commands;

public sealed class ObsCommandParser
{
    private static readonly Dictionary<string, CommandIntent> Exact = new(StringComparer.Ordinal)
    {
        ["status obs"] = CommandIntent.ObsStatus,
        ["jaki jest status obs"] = CommandIntent.ObsStatus,
        ["czy obs nagrywa"] = CommandIntent.ObsRecordStatus,
        ["czy nagrywam"] = CommandIntent.ObsRecordStatus,
        ["status nagrywania"] = CommandIntent.ObsRecordStatus,
        ["czy obs streamuje"] = CommandIntent.ObsStreamStatus,
        ["czy streamuje"] = CommandIntent.ObsStreamStatus,
        ["status streamu"] = CommandIntent.ObsStreamStatus,
        ["jaka scena jest aktywna"] = CommandIntent.ObsCurrentScene,
        ["jaka scena jest w obs"] = CommandIntent.ObsCurrentScene,
        ["lista scen obs"] = CommandIntent.ObsListScenes,
        ["jakie mam sceny w obs"] = CommandIntent.ObsListScenes,
        ["zacznij nagrywanie"] = CommandIntent.ObsStartRecord,
        ["rozpocznij nagrywanie"] = CommandIntent.ObsStartRecord,
        ["wlacz nagrywanie"] = CommandIntent.ObsStartRecord,
        ["zatrzymaj nagrywanie"] = CommandIntent.ObsStopRecord,
        ["zakoncz nagrywanie"] = CommandIntent.ObsStopRecord,
        ["wylacz nagrywanie"] = CommandIntent.ObsStopRecord,
        ["zacznij stream"] = CommandIntent.ObsStartStream,
        ["rozpocznij stream"] = CommandIntent.ObsStartStream,
        ["wlacz stream"] = CommandIntent.ObsStartStream,
        ["zacznij transmisje"] = CommandIntent.ObsStartStream,
        ["zatrzymaj stream"] = CommandIntent.ObsStopStream,
        ["zakoncz stream"] = CommandIntent.ObsStopStream,
        ["wylacz stream"] = CommandIntent.ObsStopStream,
        ["zatrzymaj transmisje"] = CommandIntent.ObsStopStream,
        ["pauza nagrywania"] = CommandIntent.ObsPauseRecord,
        ["wstrzymaj nagrywanie"] = CommandIntent.ObsPauseRecord,
        ["wznow nagrywanie"] = CommandIntent.ObsResumeRecord,
        ["kontynuuj nagrywanie"] = CommandIntent.ObsResumeRecord,
        ["lista wejsc obs"] = CommandIntent.ObsListInputs,
        ["jakie mam wejscia w obs"] = CommandIntent.ObsListInputs
    };

    public CommandRequest? Parse(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        var normalized = CommandRegistry.Normalize(text);
        if (Exact.TryGetValue(normalized, out var intent))
            return new(intent);


        var mute = ExtractObsTarget(normalized, "wycisz ");
        if (mute is not null) return new(CommandIntent.ObsMuteInput, mute);
        var unmute = ExtractObsTarget(normalized, "odcisz ");
        if (unmute is not null) return new(CommandIntent.ObsUnmuteInput, unmute);

        foreach (var prefix in new[] { "pokaz zrodlo ", "wlacz zrodlo " })
            if (normalized.StartsWith(prefix, StringComparison.Ordinal) && normalized.Length > prefix.Length)
                return new(CommandIntent.ObsShowSource, normalized[prefix.Length..].Trim());
        foreach (var prefix in new[] { "ukryj zrodlo ", "wylacz zrodlo " })
            if (normalized.StartsWith(prefix, StringComparison.Ordinal) && normalized.Length > prefix.Length)
                return new(CommandIntent.ObsHideSource, normalized[prefix.Length..].Trim());

        foreach (var prefix in new[] { "przelacz na scene ", "ustaw scene ", "wlacz scene ", "scena " })
            if (normalized.StartsWith(prefix, StringComparison.Ordinal) && normalized.Length > prefix.Length)
                return new(CommandIntent.ObsSetScene, normalized[prefix.Length..].Trim());
        return null;
    }

    private static string? ExtractObsTarget(string text, string verb)
    {
        if (!text.StartsWith(verb, StringComparison.Ordinal)) return null;
        var rest = text[verb.Length..].Trim();
        if (!rest.EndsWith(" w obs", StringComparison.Ordinal)) return null;
        rest = rest[..^6].Trim();
        return rest.Length == 0 ? null : rest;
    }
}
