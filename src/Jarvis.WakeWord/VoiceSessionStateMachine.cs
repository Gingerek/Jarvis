namespace Jarvis.WakeWord;

public enum VoiceSessionState
{
    Sleeping,
    Listening
}

public enum VoiceSessionTransition
{
    None,
    Woke,
    ActivityRefreshed,
    TimedOut,
    Slept
}

public sealed class VoiceSessionStateMachine
{
    private readonly TimeSpan _idleTimeout;
    private DateTimeOffset? _lastActivity;

    public VoiceSessionStateMachine(TimeSpan? idleTimeout = null)
    {
        _idleTimeout = idleTimeout ?? TimeSpan.FromSeconds(20);
        if (_idleTimeout <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(idleTimeout));
    }

    public VoiceSessionState State { get; private set; } = VoiceSessionState.Sleeping;
    public DateTimeOffset? LastActivity => _lastActivity;
    public VoiceSessionTransition Wake(DateTimeOffset now)
    {
        State = VoiceSessionState.Listening;
        _lastActivity = now;
        return VoiceSessionTransition.Woke;
    }

    public VoiceSessionTransition RegisterActivity(DateTimeOffset now)
    {
        if (State != VoiceSessionState.Listening) return VoiceSessionTransition.None;
        _lastActivity = now;
        return VoiceSessionTransition.ActivityRefreshed;
    }

    public VoiceSessionTransition Tick(DateTimeOffset now)
    {
        if (State != VoiceSessionState.Listening || _lastActivity is null)
            return VoiceSessionTransition.None;
        if (now - _lastActivity.Value < _idleTimeout)
            return VoiceSessionTransition.None;
        State = VoiceSessionState.Sleeping;
        _lastActivity = null;
        return VoiceSessionTransition.TimedOut;
    }

    public VoiceSessionTransition Sleep()
    {
        State = VoiceSessionState.Sleeping;
        _lastActivity = null;
        return VoiceSessionTransition.Slept;
    }
}
