using Jarvis.ASR;
using Jarvis.Brain;
using Jarvis.WakeWord;

namespace Jarvis.Core.Tests;

public sealed class VoicePipelineProcessorTests
{
    [Fact]
    public async Task Wake_And_Command_In_One_Transcript_Preserves_Command()
    {
        await using var asr = new FakeAsr("Jarvis, otwórz Lightroom");
        var voice = new VoiceInteractionCoordinator(
            new TranscriptWakeWordDetector("Jarvis"),
            new VoiceSessionStateMachine(TimeSpan.FromSeconds(20)));
        var processor = new VoicePipelineProcessor(asr, voice);

        var result = await processor.ProcessSpeechAsync(
            new short[1600], DateTimeOffset.UtcNow);

        Assert.Equal(VoiceInputDisposition.WakeDetected, result.Disposition);
        Assert.Equal("otwórz Lightroom", result.CommandText);
    }

    private sealed class FakeAsr(string text) : IAsrEngine
    {
        public string ModelName => "fake";
        public Task<AsrTranscript> TranscribeAsync(
            ReadOnlyMemory<short> pcm16,
            string language = "pl",
            CancellationToken cancellationToken = default) =>
            Task.FromResult(new AsrTranscript(text, language, 1.0, 1.0));

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
