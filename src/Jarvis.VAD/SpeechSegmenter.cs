namespace Jarvis.VAD;

public sealed record SpeechSegmentResult(
    short[] Samples,
    TimeSpan Duration,
    float PeakProbability);

public sealed class SpeechSegmenter
{
    private readonly SileroVadEngine _vad;
    private readonly VadSpeechGate _gate;
    private readonly int _preRollFrames;
    private readonly int _maxSpeechSamples;
    private readonly Queue<short[]> _preRoll = new();
    private readonly List<short> _pending = [];
    private readonly List<short> _speech = [];
    private float _peak;

    public SpeechSegmenter(
        SileroVadEngine vad,
        VadSpeechGate? gate = null,
        int preRollMs = 320,
        int maxSpeechMs = 12000)
    {
        _vad = vad;
        _gate = gate ?? new VadSpeechGate();
        _preRollFrames = Math.Max(1, (int)Math.Ceiling(preRollMs / 32.0));
        _maxSpeechSamples = SileroVadEngine.SampleRate * maxSpeechMs / 1000;
    }
    public SpeechSegmentResult? Push(ReadOnlySpan<short> pcm16)
    {
        foreach (var sample in pcm16) _pending.Add(sample);

        while (_pending.Count >= SileroVadEngine.FrameSamples)
        {
            var frame = _pending.Take(SileroVadEngine.FrameSamples).ToArray();
            _pending.RemoveRange(0, SileroVadEngine.FrameSamples);
            var result = _vad.Process(frame);
            _peak = Math.Max(_peak, result.SpeechProbability);
            var transition = _gate.Push(result.SpeechProbability);

            if (transition == VadTransition.SpeechEnded)
            {
                _speech.AddRange(frame);
                return Complete();
            }

            if (!_gate.IsSpeaking && transition != VadTransition.SpeechStarted)
            {
                _preRoll.Enqueue(frame);
                while (_preRoll.Count > _preRollFrames) _preRoll.Dequeue();
                continue;
            }

            if (transition == VadTransition.SpeechStarted)
            {
                _speech.Clear();
                foreach (var pre in _preRoll) _speech.AddRange(pre);
                _preRoll.Clear();
            }
            _speech.AddRange(frame);
            if (_speech.Count >= _maxSpeechSamples)
                return Complete();
        }

        return null;
    }

    private SpeechSegmentResult Complete()
    {
        var samples = _speech.ToArray();
        var result = new SpeechSegmentResult(
            samples,
            TimeSpan.FromSeconds(samples.Length / (double)SileroVadEngine.SampleRate),
            _peak);
        Reset();
        return result;
    }

    public void Reset()
    {
        _pending.Clear();
        _speech.Clear();
        _preRoll.Clear();
        _peak = 0;
        _gate.Reset();
        _vad.Reset();
    }
}
