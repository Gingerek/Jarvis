using Jarvis.WakeWord;

namespace Jarvis.Core.Tests;

public sealed class VoiceSessionStateMachineTests
{
    [Fact]
    public void Wake_Enters_Listening()
    {
        var sm = new VoiceSessionStateMachine(TimeSpan.FromSeconds(20));
        var now = DateTimeOffset.UtcNow;
        var transition = sm.Wake(now);
        Assert.Equal(VoiceSessionTransition.Woke, transition);
        Assert.Equal(VoiceSessionState.Listening, sm.State);
        Assert.Equal(now, sm.LastActivity);
    }

    [Fact]
    public void Tick_Times_Out_After_Idle_Window()
    {
        var sm = new VoiceSessionStateMachine(TimeSpan.FromSeconds(20));
        var now = DateTimeOffset.UtcNow;
        sm.Wake(now);
        Assert.Equal(VoiceSessionTransition.None, sm.Tick(now.AddSeconds(19)));
        Assert.Equal(VoiceSessionTransition.TimedOut, sm.Tick(now.AddSeconds(21)));
        Assert.Equal(VoiceSessionState.Sleeping, sm.State);
    }
}
