namespace Jarvis.ASR;

public sealed record AsrTranscript(
    string Text,
    string Language,
    double LanguageProbability,
    double InferenceMs);

public interface IAsrEngine : IAsyncDisposable
{
    string ModelName { get; }

    Task<AsrTranscript> TranscribeAsync(
        ReadOnlyMemory<short> pcm16,
        string language = "pl",
        CancellationToken cancellationToken = default);
}
