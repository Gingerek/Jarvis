# Next Step

## Current phase
Phase 2 — Audio & Voice foundation.

## Immediate objective
Implement reliable microphone discovery and WASAPI capture on this machine before choosing final ASR or wake-word engines.

## Required next actions
1. Add NAudio 2.3.0 to Jarvis.Audio.
2. Enumerate active capture/render endpoints and identify the actual default microphone.
3. Persist selected microphone by stable device identifier, not friendly name only.
4. Implement WASAPI capture with cancellation and deterministic disposal.
5. Add PCM format normalization needed by VAD/ASR.
6. Add ring buffer and timestamped audio frames.
7. Measure capture startup latency and frame delivery jitter.
8. Add device-loss/reconnect handling.
9. Add audio diagnostics and health checks.
10. Only after capture is stable, benchmark VAD and ASR candidates on this Ryzen 7 5700G.

## Phase 2 rules
- no final ASR selection without benchmark data,
- no final wake-word selection without benchmark/license data,
- no cloud dependency on the fast local command path,
- no permanent Windows security-policy changes,
- every visible UI state must represent real runtime state.

## Next session rule
Read PROJECT_STATE.md, ARCHITECTURE.md, NEXT_STEP.md, LATENCY_PLAN.md and KNOWN_ISSUES.md before changing code.
