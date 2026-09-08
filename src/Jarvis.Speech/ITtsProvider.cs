namespace Jarvis.Speech;

public sealed record TtsStreamInfo(
    string Provider,
    string VoiceId,
    int SampleRate,
    int Channels,
    int BitsPerSample);

public interface ITtsProvider : IAsyncDisposable
{
    string ProviderName { get; }
    string VoiceId { get; }

    IAsyncEnumerable<ReadOnlyMemory<byte>> StreamAsync(
        string text,
        CancellationToken cancellationToken = default);
}
