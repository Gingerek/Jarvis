using Jarvis.ASR;
using Jarvis.Brain;
using Jarvis.Commands;
using Jarvis.WakeWord;

namespace Jarvis.Core.Tests;

public sealed class ContinuousVoiceScenarioTests
{
    [Fact]
    public async Task Wake_EnableContinuous_Then_Time_Works_After_One_Hour()
    {
        var now = DateTimeOffset.UtcNow;
        await using var asr = new SequenceAsr("Jarvis, obudź się", "Która jest godzina.");
        var voice = new VoiceInteractionCoordinator(
            new TranscriptWakeWordDetector("Jarvis"),
            new VoiceSessionStateMachine(TimeSpan.FromSeconds(20)));
        var pipeline = new VoicePipelineProcessor(asr, voice);

        var wake = await pipeline.ProcessSpeechAsync(new short[1600], now);
        Assert.Equal(VoiceInputDisposition.WakeDetected, wake.Disposition);
        Assert.Equal(VoiceModeCommand.EnableContinuous, VoiceModeCommandParser.Parse(wake.CommandText));
        voice.EnableContinuousListening(now);
        Assert.True(voice.ContinuousListening);
        Assert.NotEqual(VoiceInputDisposition.SessionTimedOut, voice.Tick(now.AddHours(1)).Disposition);
        var time = await pipeline.ProcessSpeechAsync(new short[1600], now.AddHours(1));
        Assert.Equal(VoiceInputDisposition.CommandAccepted, time.Disposition);
        Assert.Equal("Która jest godzina.", time.CommandText);

        var request = new CommandRegistry().Parse(time.CommandText!);
        Assert.NotNull(request);
        Assert.Equal(CommandIntent.GetTime, request.Intent);
    }

    private sealed class SequenceAsr(params string[] texts) : IAsrEngine
    {
        private readonly Queue<string> _texts = new(texts);
        public string ModelName => "fake";

        public Task<AsrTranscript> TranscribeAsync(
            ReadOnlyMemory<short> pcm16,
            string language = "pl",
            CancellationToken cancellationToken = default)
        {
            var text = _texts.Dequeue();
            return Task.FromResult(new AsrTranscript(text, language, 1.0, 1.0));
        }

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
