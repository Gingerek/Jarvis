using Jarvis.VAD;

namespace Jarvis.Core.Tests;

public sealed class VadSpeechGateTests
{
    [Fact]
    public void SustainedSpeech_ThenSilence_ProducesTransitions()
    {
        var gate = new VadSpeechGate(minimumSpeechMs: 64, minimumSilenceMs: 64);
        Assert.Equal(VadTransition.None, gate.Push(0.9f));
        Assert.Equal(VadTransition.SpeechStarted, gate.Push(0.9f));
        Assert.True(gate.IsSpeaking);
        Assert.Equal(VadTransition.None, gate.Push(0.1f));
        Assert.Equal(VadTransition.SpeechEnded, gate.Push(0.1f));
        Assert.False(gate.IsSpeaking);
    }

    [Fact]
    public void IsolatedSpike_DoesNotStartSpeech()
    {
        var gate = new VadSpeechGate(minimumSpeechMs: 96);
        Assert.Equal(VadTransition.None, gate.Push(0.9f));
        Assert.Equal(VadTransition.None, gate.Push(0.1f));
        Assert.False(gate.IsSpeaking);
    }
}
