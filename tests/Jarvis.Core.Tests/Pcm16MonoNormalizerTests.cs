using Jarvis.Audio;

namespace Jarvis.Core.Tests;

public sealed class Pcm16MonoNormalizerTests
{
    [Fact]
    public void Converts48kStereoFloatTo16kMono()
    {
        var samples = new float[480 * 2];
        Array.Fill(samples, 0.5f);
        var bytes = new byte[samples.Length * sizeof(float)];
        Buffer.BlockCopy(samples, 0, bytes, 0, bytes.Length);

        var output = Pcm16MonoNormalizer.NormalizeFloat32(bytes, 48000, 2);

        Assert.Equal(160, output.Length);
        Assert.All(output, sample => Assert.InRange(sample, (short)16382, (short)16385));
    }

    [Fact]
    public void MixesStereoChannelsToMono()
    {
        var samples = new float[] { 1f, -1f, 1f, -1f, 1f, -1f };
        var bytes = new byte[samples.Length * sizeof(float)];
        Buffer.BlockCopy(samples, 0, bytes, 0, bytes.Length);
        Assert.All(Pcm16MonoNormalizer.NormalizeFloat32(bytes, 48000, 2), sample => Assert.Equal(0, sample));
    }
}
