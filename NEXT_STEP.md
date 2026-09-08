# Next Step

## Current phase
Phase 2 — Audio & Voice foundation.

## Immediate objective
Benchmark VAD candidates on the now-stable 16 kHz mono audio path before selecting ASR.

## Completed audio gate
- NAudio/WASAPI capture: PASS,
- stable endpoint persistence: PASS,
- preferred Nor-Tec microphone selected,
- ring buffer/timestamps: PASS,
- PCM16 16 kHz mono normalization: PASS,
- latency/jitter instrumentation: PASS,
- reconnect/health foundation: PASS.

## Required next actions
1. Benchmark Silero VAD on Ryzen 7 5700G CPU.
2. Measure VAD processing time, false starts and speech-end latency.
3. Compare at least one alternative VAD if licensing/runtime quality justifies it.
4. Record p50/p90/p95/p99 where sample count is sufficient.
5. Select VAD only after benchmark evidence.
6. Then benchmark CPU-capable ASR candidates using the same recorded Polish utterance corpus.
7. Do not select wake word until its licensing and Polish recognition gate passes.

## Rules
No cloud dependency on the fast local command path and no permanent Windows security-policy changes.
