using Jarvis.ASR;
using Jarvis.WakeWord;

namespace Jarvis.Brain;

public sealed record VoicePipelineResult(
    VoiceInputDisposition Disposition,
    string Transcript,
    string? CommandText,
    double AsrInferenceMs);

public sealed class VoicePipelineProcessor
{
    private readonly IAsrEngine _asr;
    private readonly VoiceInteractionCoordinator _voice;

    public VoicePipelineProcessor(
        IAsrEngine asr,
        VoiceInteractionCoordinator voice)
    {
        _asr = asr;
        _voice = voice;
    }

    public async Task<VoicePipelineResult> ProcessSpeechAsync(
        ReadOnlyMemory<short> pcm16,
        DateTimeOffset now,
        CancellationToken cancellationToken = default)
    {
        var transcript = await _asr.TranscribeAsync(pcm16, "pl", cancellationToken);
        var decision = _voice.HandleTranscript(transcript.Text, now);
        string? command = decision.Disposition switch
        {
            VoiceInputDisposition.CommandAccepted => transcript.Text.Trim(),
            VoiceInputDisposition.WakeDetected => StripWakeToken(transcript.Text, decision.WakeMatch),
            _ => null
        };

        return new VoicePipelineResult(
            decision.Disposition,
            transcript.Text,
            string.IsNullOrWhiteSpace(command) ? null : command,
            transcript.InferenceMs);
    }

    private static string? StripWakeToken(string transcript, WakeWordMatch? match)
    {
        if (match is null) return null;
        var index = transcript.IndexOf(match.Heard, StringComparison.OrdinalIgnoreCase);
        if (index < 0) return null;
        var before = transcript[..index];
        var after = transcript[(index + match.Heard.Length)..];
        var remaining = (before + " " + after)
            .Trim(' ', ',', '.', '!', '?', ';', ':', '-');
        return remaining.Length == 0 ? null : remaining;
    }
}
