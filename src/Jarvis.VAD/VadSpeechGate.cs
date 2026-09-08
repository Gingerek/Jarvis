namespace Jarvis.VAD;

public enum VadTransition
{
    None,
    SpeechStarted,
    SpeechEnded
}

public sealed class VadSpeechGate
{
    private readonly int _startFrames;
    private readonly int _endFrames;
    private int _speechRun;
    private int _silenceRun;

    public VadSpeechGate(
        float startThreshold = 0.5f,
        float endThreshold = 0.35f,
        int minimumSpeechMs = 96,
        int minimumSilenceMs = 480)
    {
        StartThreshold = startThreshold;
        EndThreshold = endThreshold;
        _startFrames = Math.Max(1, (int)Math.Ceiling(minimumSpeechMs / 32.0));
        _endFrames = Math.Max(1, (int)Math.Ceiling(minimumSilenceMs / 32.0));
    }

    public float StartThreshold { get; }
    public float EndThreshold { get; }
    public bool IsSpeaking { get; private set; }

    public VadTransition Push(float probability)
    {
        if (!IsSpeaking)
        {
            _speechRun = probability >= StartThreshold ? _speechRun + 1 : 0;
            if (_speechRun >= _startFrames)
            {
                IsSpeaking = true;
                _speechRun = 0;
                _silenceRun = 0;
                return VadTransition.SpeechStarted;
            }
            return VadTransition.None;
        }

        _silenceRun = probability < EndThreshold ? _silenceRun + 1 : 0;
        if (_silenceRun < _endFrames) return VadTransition.None;

        IsSpeaking = false;
        _speechRun = 0;
        _silenceRun = 0;
        return VadTransition.SpeechEnded;
    }

    public void Reset()
    {
        IsSpeaking = false;
        _speechRun = 0;
        _silenceRun = 0;
    }
}
