using Jarvis.WakeWord;

namespace Jarvis.Core.Tests;

public sealed class VoiceInteractionCoordinatorTests
{
    [Fact]
    public void Sleeping_Ignores_Normal_Command()
    {
        var c = Create();
        var result = c.HandleTranscript("otwórz Lightroom", DateTimeOffset.UtcNow);
        Assert.Equal(VoiceInputDisposition.IgnoredWhileSleeping, result.Disposition);
        Assert.Equal(VoiceSessionState.Sleeping, c.State);
    }

    [Fact]
    public void Jarvis_Wakes_Then_Next_Command_Is_Accepted()
    {
        var c = Create();
        var now = DateTimeOffset.UtcNow;
        var wake = c.HandleTranscript("Jarvis", now);
        var command = c.HandleTranscript("otwórz Lightroom", now.AddSeconds(1));
        Assert.Equal(VoiceInputDisposition.WakeDetected, wake.Disposition);
        Assert.Equal(VoiceInputDisposition.CommandAccepted, command.Disposition);
        Assert.Equal(VoiceSessionState.Listening, c.State);
    }

    [Fact]
    public void Listening_Times_Out_After_Idle_Window()
    {
        var c = Create();
        var now = DateTimeOffset.UtcNow;
        c.HandleTranscript("Jarvis", now);
        var result = c.Tick(now.AddSeconds(21));
        Assert.Equal(VoiceInputDisposition.SessionTimedOut, result.Disposition);
        Assert.Equal(VoiceSessionState.Sleeping, c.State);
    }

    private static VoiceInteractionCoordinator Create() => new(
        new TranscriptWakeWordDetector("Jarvis"),
        new VoiceSessionStateMachine(TimeSpan.FromSeconds(20)));
}
