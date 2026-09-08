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
- "Jarvis, <command>" preserves command text in one utterance.
- ASR -> wake/session pipeline processor: implemented.

## Immediate objective
Run real microphone -> VAD -> ASR -> wake/session -> intent smoke test.

## Next actions
1. Get GitHub CI green after split UI/non-UI build fix.
2. Add live speech segment collector driven by VAD transitions.
3. Feed completed speech segments into VoicePipelineProcessor.
4. Validate wake false accept/reject behavior on live audio.
5. Add TTS provider abstraction and human Polish voice.
6. Add barge-in / echo suppression policy.
7. Close Phase 2 gate with end-to-end live test.
