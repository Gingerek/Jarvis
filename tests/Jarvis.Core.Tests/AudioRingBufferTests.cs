using Jarvis.Audio;

namespace Jarvis.Core.Tests;

public sealed class AudioRingBufferTests
{
    [Fact]
    public void KeepsNewestBytesWhenCapacityExceeded()
    {
        var buffer = new AudioRingBuffer(4);
        buffer.Write([1, 2, 3]);
        buffer.Write([4, 5]);

        Assert.Equal(4, buffer.Count);
        Assert.Equal(new byte[] { 2, 3, 4, 5 }, buffer.Snapshot());
    }

    [Fact]
    public void SnapshotPreservesWriteOrder()
    {
        var buffer = new AudioRingBuffer(8);
        buffer.Write([10, 20, 30]);
        Assert.Equal(new byte[] { 10, 20, 30 }, buffer.Snapshot());
    }
}
