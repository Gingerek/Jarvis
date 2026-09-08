using Jarvis.VAD;

namespace Jarvis.Core.Tests;

public sealed class VadSpeechGateTests
{
    [Fact]
    public void Starts_After_Required_Consecutive_Speech_Frames()
    {
        var gate = new VadSpeechGate(minimumSpeechMs: 96);
        Assert.Equal(VadTransition.None, gate.Push(0.8f));
        Assert.Equal(VadTransition.None, gate.Push(0.8f));
        Assert.Equal(VadTransition.SpeechStarted, gate.Push(0.8f));
        Assert.True(gate.IsSpeaking);
    }

    [Fact]
    public void Brief_Dip_Does_Not_End_Speech()
    {
        var gate = new VadSpeechGate(minimumSpeechMs: 32, minimumSilenceMs: 96);
        Assert.Equal(VadTransition.SpeechStarted, gate.Push(0.8f));
        Assert.Equal(VadTransition.None, gate.Push(0.1f));
        Assert.Equal(VadTransition.None, gate.Push(0.8f));
        Assert.True(gate.IsSpeaking);
    }

    [Fact]
    public void Ends_After_Required_Consecutive_Silence_Frames()
    {
        var gate = new VadSpeechGate(minimumSpeechMs: 32, minimumSilenceMs: 96);
        gate.Push(0.8f);
        Assert.Equal(VadTransition.None, gate.Push(0.1f));
        Assert.Equal(VadTransition.None, gate.Push(0.1f));
        Assert.Equal(VadTransition.SpeechEnded, gate.Push(0.1f));
        Assert.False(gate.IsSpeaking);
    }
}
