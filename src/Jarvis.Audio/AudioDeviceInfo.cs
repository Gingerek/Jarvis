namespace Jarvis.Audio;

public sealed record AudioDeviceInfo(
    string Id,
    string Name,
    bool IsDefault,
    int SampleRate,
    int Channels,
    int BitsPerSample);

public sealed record AudioCaptureMetrics(
    long Frames,
    long Bytes,
    double Peak,
    double Rms,
    TimeSpan Duration,
    double FirstFrameLatencyMs,
    double MeanCallbackIntervalMs,
    double MaxCallbackJitterMs);

public sealed record TimestampedAudioChunk(
    long Sequence,
    DateTimeOffset Timestamp,
    ReadOnlyMemory<byte> Data);
