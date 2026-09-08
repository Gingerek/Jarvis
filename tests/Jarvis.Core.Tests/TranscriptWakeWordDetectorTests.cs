using Jarvis.WakeWord;

namespace Jarvis.Core.Tests;

public sealed class TranscriptWakeWordDetectorTests
{
    [Theory]
    [InlineData("Jarvis")]
    [InlineData("jarvis otwórz lightroom")]
    [InlineData("hej Jarvis")]
    public void Detect_Accepts_Jarvis(string transcript)
    {
        var detector = new TranscriptWakeWordDetector("Jarvis");
        Assert.NotNull(detector.Detect(transcript, 0.70));
    }

    [Theory]
    [InlineData("jaki jest dzisiaj dzień")]
    [InlineData("włącz youtube")]
    [InlineData("lightroom")]
    public void Detect_Rejects_Unrelated_Text(string transcript)
    {
        var detector = new TranscriptWakeWordDetector("Jarvis");
        Assert.Null(detector.Detect(transcript, 0.80));
    }
}
