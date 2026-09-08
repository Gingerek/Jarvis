# Next Step

## Current phase
Phase 2 — Audio & Voice.

## Completed
- WASAPI capture foundation: PASS.
- Nor-Tec streaming mic selected by stable endpoint ID.
- 16 kHz mono PCM16 normalization: PASS.
- Silero VAD streaming implementation: PASS.
- Silero CPU/live benchmark: PASS.

## Immediate objective
Benchmark CPU-capable Polish ASR candidates on Ryzen 7 5700G before selecting the production backend.

## Required next actions
1. Capture a controlled Polish speech sample from Nor-Tec.
2. Benchmark faster-whisper CPU candidates.
3. Measure warm/cold transcription latency and real-time factor.
4. Compare accuracy on command-style Polish phrases.
5. Select backend only from measured results.
6. Integrate chosen ASR behind Jarvis.ASR interface.

## Rule
Do not select final ASR or wake-word engine without benchmark data.
