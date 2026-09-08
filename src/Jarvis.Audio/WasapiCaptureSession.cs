using NAudio.CoreAudioApi;
using NAudio.Wave;
using System.Diagnostics;

namespace Jarvis.Audio;

public sealed class WasapiCaptureSession : IDisposable
{
    private readonly WasapiCapture _capture;
    private readonly AudioRingBuffer _buffer;
    private readonly Stopwatch _clock = new();
    private readonly List<double> _callbackTimesMs = [];
    private long _frames;
    private long _bytes;
    private double _sumSquares;
    private double _peak;
    private long _samples;
    private long _sequence;
    private bool _started;

    public WasapiCaptureSession(string deviceId, int ringBufferBytes = 1_048_576)
    {
        using var enumerator = new MMDeviceEnumerator();
        var device = enumerator.GetDevice(deviceId);
        _capture = new WasapiCapture(device);
        _buffer = new AudioRingBuffer(ringBufferBytes);
        _capture.DataAvailable += OnDataAvailable;
        _capture.RecordingStopped += OnRecordingStopped;
    }

    public WaveFormat Format => _capture.WaveFormat;
    public AudioRingBuffer Buffer => _buffer;
    public event EventHandler<TimestampedAudioChunk>? ChunkAvailable;
    public event EventHandler<StoppedEventArgs>? CaptureStopped;

    public void Start()
    {
        if (_started) throw new InvalidOperationException("Capture is already running.");
        _started = true;
        _clock.Restart();
        _capture.StartRecording();
    }

    public async Task<AudioCaptureMetrics> CaptureForAsync(
        TimeSpan duration,
        CancellationToken cancellationToken = default)
    {
        Start();
        try
        {
            await Task.Delay(duration, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        return Stop();
    }

    public AudioCaptureMetrics Stop()
    {
        if (!_started) throw new InvalidOperationException("Capture is not running.");
        _capture.StopRecording();
        _clock.Stop();
        _started = false;

        var rms = _samples == 0 ? 0 : Math.Sqrt(_sumSquares / _samples);
        var first = _callbackTimesMs.Count == 0 ? 0 : _callbackTimesMs[0];
        var intervals = new List<double>();
        for (var i = 1; i < _callbackTimesMs.Count; i++)
            intervals.Add(_callbackTimesMs[i] - _callbackTimesMs[i - 1]);
        var mean = intervals.Count == 0 ? 0 : intervals.Average();
        var maxJitter = intervals.Count == 0 ? 0 : intervals.Max(x => Math.Abs(x - mean));

        return new AudioCaptureMetrics(
            _frames, _bytes, _peak, rms, _clock.Elapsed,
            first, mean, maxJitter);
    }

    private void OnDataAvailable(object? sender, WaveInEventArgs e)
    {
        var nowMs = _clock.Elapsed.TotalMilliseconds;
        _callbackTimesMs.Add(nowMs);
        _buffer.Write(e.Buffer.AsSpan(0, e.BytesRecorded));
        _bytes += e.BytesRecorded;
        _frames += e.BytesRecorded / Math.Max(1, Format.BlockAlign);
        AccumulateLevels(e.Buffer.AsSpan(0, e.BytesRecorded));
        var copy = e.Buffer.AsMemory(0, e.BytesRecorded).ToArray();
        ChunkAvailable?.Invoke(this, new TimestampedAudioChunk(++_sequence, DateTimeOffset.UtcNow, copy));
    }

    private void AccumulateLevels(ReadOnlySpan<byte> data)
    {
        if (Format.BitsPerSample == 32)
        {
            for (var i = 0; i + 3 < data.Length; i += 4)
            {
                var sample = BitConverter.ToSingle(data.Slice(i, 4));
                _peak = Math.Max(_peak, Math.Abs(sample));
                _sumSquares += sample * sample;
                _samples++;
            }
        }
        else if (Format.BitsPerSample == 16)
        {
            for (var i = 0; i + 1 < data.Length; i += 2)
            {
                var sample = BitConverter.ToInt16(data.Slice(i, 2)) / 32768.0;
                _peak = Math.Max(_peak, Math.Abs(sample));
                _sumSquares += sample * sample;
                _samples++;
            }
        }
    }

    private void OnRecordingStopped(object? sender, StoppedEventArgs e) =>
        CaptureStopped?.Invoke(this, e);

    public void Dispose()
    {
        _capture.DataAvailable -= OnDataAvailable;
        _capture.RecordingStopped -= OnRecordingStopped;
        _capture.Dispose();
    }
}
