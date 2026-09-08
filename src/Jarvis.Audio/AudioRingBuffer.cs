namespace Jarvis.Audio;

public sealed class AudioRingBuffer
{
    private readonly byte[] _buffer;
    private int _writeIndex;
    private int _count;
    private readonly object _gate = new();

    public AudioRingBuffer(int capacityBytes)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(capacityBytes);
        _buffer = new byte[capacityBytes];
    }

    public int Capacity => _buffer.Length;
    public int Count { get { lock (_gate) return _count; } }

    public void Write(ReadOnlySpan<byte> data)
    {
        lock (_gate)
        {
            foreach (var value in data)
            {
                _buffer[_writeIndex] = value;
                _writeIndex = (_writeIndex + 1) % _buffer.Length;
                if (_count < _buffer.Length) _count++;
            }
        }
    }

    public byte[] Snapshot()
    {
        lock (_gate)
        {
            var result = new byte[_count];
            var start = (_writeIndex - _count + _buffer.Length) % _buffer.Length;
            for (var i = 0; i < _count; i++)
            {
                result[i] = _buffer[(start + i) % _buffer.Length];
            }
            return result;
        }
    }
}
