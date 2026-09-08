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
- ElevenLabs streaming TTS provider: PASS.
- Windows Credential Manager secret store: PASS.
- selected Jarvis voice: Marcin — Deep & Cinematic Polish Narrator.
- selected voice_id: B5tmTXp0L7DzqmlGqIMJ.
- TTS live probe: PASS.
- VoiceHost spoken execution confirmations: implemented.

## Current facts
- Live Nor-Tec -> VAD -> ASR works.
- Controlled `otwórz Lightroom` starts Lightroom Classic.
- VoiceHost now pauses capture while speaking to avoid self-triggering.
- Live wake phrase still needs a clean controlled PASS.

## Immediate objective
Run controlled live `Jarvis, otwórz Lightroom` end-to-end with spoken confirmation, then implement barge-in and session hardening.