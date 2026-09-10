using Jarvis.Commands;

namespace Jarvis.Core.Tests;

public sealed class CommandCorpusCorrectorTests
{
    [Fact]
    public void Corrects_Common_Asr_Error_For_Time()
    {
        var path = CreateCorpus();
        try
        {
            var corrector = CommandCorpusCorrector.Load(path);
            var match = corrector.Correct("która jest wedzina");
            Assert.NotNull(match);
            Assert.Equal(CommandIntent.GetTime, match.Request.Intent);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void Corrects_Broken_Lightroom_Name()
    {
        var path = CreateCorpus();
        try
        {
            var corrector = CommandCorpusCorrector.Load(path);
            var match = corrector.Correct("otwórz ligh atrom");
            Assert.NotNull(match);
            Assert.Equal(CommandIntent.OpenApplication, match.Request.Intent);
            Assert.Equal("lightroom", match.Request.Argument);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void Does_Not_Force_Unrelated_Text_Into_Command()
    {
        var path = CreateCorpus();
        try
        {
            var corrector = CommandCorpusCorrector.Load(path);
            Assert.Null(corrector.Correct("dzisiaj zrobiłem dobre zdjęcie na spacerze"));
        }
        finally { File.Delete(path); }
    }

    private static string CreateCorpus()
    {
        var path = Path.Combine(Path.GetTempPath(), $"jarvis-corpus-{Guid.NewGuid():N}.jsonl");
        File.WriteAllLines(path,
        [
            "{\"intent\":\"GetTime\",\"argument\":null,\"utterance\":\"która jest godzina\"}",
            "{\"intent\":\"OpenApplication\",\"argument\":\"lightroom\",\"utterance\":\"otwórz Lightroom\"}",
            "{\"intent\":\"GetDate\",\"argument\":null,\"utterance\":\"jaka jest data\"}"
        ]);
        return path;
    }
}
