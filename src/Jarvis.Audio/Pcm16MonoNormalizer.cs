namespace Jarvis.Audio;

public static class Pcm16MonoNormalizer
{
    public const int TargetSampleRate = 16000;

    public static short[] NormalizeFloat32(
        ReadOnlySpan<byte> input,
        int sourceSampleRate,
        int channels)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(sourceSampleRate);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(channels);
        var sourceFrames = input.Length / (sizeof(float) * channels);
        if (sourceFrames == 0) return [];

        var mono = new float[sourceFrames];
        for (var frame = 0; frame < sourceFrames; frame++)
        {
            double sum = 0;
            for (var channel = 0; channel < channels; channel++)
            {
                var offset = (frame * channels + channel) * sizeof(float);
                sum += BitConverter.ToSingle(input.Slice(offset, sizeof(float)));
            }
            mono[frame] = (float)(sum / channels);
        }

        var outputFrames = Math.Max(1, (int)Math.Round(
            sourceFrames * (double)TargetSampleRate / sourceSampleRate));
        var output = new short[outputFrames];
        var ratio = (double)sourceSampleRate / TargetSampleRate;

        for (var i = 0; i < outputFrames; i++)
        {
            var sourcePosition = i * ratio;
            var left = Math.Min((int)sourcePosition, sourceFrames - 1);
            var right = Math.Min(left + 1, sourceFrames - 1);
            var fraction = sourcePosition - left;
            var sample = mono[left] + (mono[right] - mono[left]) * fraction;
            sample = Math.Clamp(sample, -1f, 1f);
            output[i] = (short)Math.Round(sample * short.MaxValue);
        }

        return output;
    }
}
