# Next Step

## Current phase
Phase 2 — Audio & Voice.

## Completed
- WASAPI capture + Nor-Tec selection: PASS.
- 16 kHz mono PCM16 normalization: PASS.
- Silero VAD streaming + benchmark: PASS.
- faster-whisper base / CPU int8 fast path: PASS.
- persistent C# -> Python ASR worker: PASS.
- transcript wake-word detector for "Jarvis": implemented.
- Sleeping -> Listening -> idle timeout state machine: implemented.
- active session accepts follow-up commands without repeating "Jarvis".
- live VoiceHost + speech segmentation: implemented.
- deterministic app launch fast path: PASS for Lightroom Classic.
- ElevenLabs streaming TTS provider: implemented.
- SpeechOutputManager with interruption support: implemented.
- Windows Credential Manager secret store: implemented.
- TTS probe: implemented.

## Current facts
- Live Nor-Tec -> VAD -> ASR works.
- Latest controlled launch `otwórz Lightroom` starts Lightroom Classic.
- Live wake phrase has not yet earned PASS in a controlled microphone run.
- ElevenLabs API key is not configured yet.
- TtsVoiceId is not selected yet.

## Immediate objective
Configure one approved ElevenLabs Polish voice securely, run live TTS, then connect spoken confirmations to VoiceHost.
