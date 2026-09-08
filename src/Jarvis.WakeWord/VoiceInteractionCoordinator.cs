namespace Jarvis.WakeWord;

public enum VoiceInputDisposition
{
    IgnoredWhileSleeping,
    WakeDetected,
    CommandAccepted,
    SessionTimedOut
}

public sealed record VoiceInputDecision(
    VoiceInputDisposition Disposition,
    string Transcript,
    WakeWordMatch? WakeMatch = null);

public sealed class VoiceInteractionCoordinator
{
    private readonly TranscriptWakeWordDetector _detector;
    private readonly VoiceSessionStateMachine _session;

    public VoiceInteractionCoordinator(
        TranscriptWakeWordDetector detector,
        VoiceSessionStateMachine session)
    {
        _detector = detector;
        _session = session;
    }

    public VoiceSessionState State => _session.State;
    public VoiceInputDecision HandleTranscript(string transcript, DateTimeOffset now)
    {
        transcript ??= string.Empty;

        if (_session.State == VoiceSessionState.Sleeping)
        {
            var match = _detector.Detect(transcript, 0.80);
            if (match is null)
                return new VoiceInputDecision(
                    VoiceInputDisposition.IgnoredWhileSleeping, transcript);

            _session.Wake(now);
            return new VoiceInputDecision(
                VoiceInputDisposition.WakeDetected, transcript, match);
        }

        _session.RegisterActivity(now);
        return new VoiceInputDecision(
            VoiceInputDisposition.CommandAccepted, transcript);
    }

    public VoiceInputDecision Tick(DateTimeOffset now)
    {
        var transition = _session.Tick(now);
        return new VoiceInputDecision(
            transition == VoiceSessionTransition.TimedOut
                ? VoiceInputDisposition.SessionTimedOut
                : VoiceInputDisposition.IgnoredWhileSleeping,
            string.Empty);
    }
}
