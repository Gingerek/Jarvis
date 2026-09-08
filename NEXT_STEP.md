# Next Step

## Current phase
Phase 2 — Audio & Voice.

## Completed
- WASAPI capture + Nor-Tec selection: PASS.
- 16 kHz mono PCM16 normalization: PASS.
- Silero VAD streaming + benchmark: PASS.
- faster-whisper ASR benchmark: PASS.
- command fast-path ASR selected: base / CPU int8.
- persistent C# -> Python ASR worker: PASS.
- fuzzy known-entity resolver added for application names.

## Immediate objective
Implement and benchmark wake-word strategy, then TTS output and barge-in behavior.

## Next actions
1. Resolve wake-word engine/license gate.
2. Implement wake/listen state machine.
3. Benchmark false accepts/false rejects for Jarvis trigger.
4. Implement TTS provider abstraction and local/cloud fallback.
5. Add echo suppression/barge-in policy.
6. Run Phase 2 end-to-end microphone -> VAD -> ASR -> intent smoke test.

## Known environment issue
Local Windows Application Control intermittently blocks freshly built test DLL copies under tests/bin. Do not disable protection; use CI as independent test gate.
