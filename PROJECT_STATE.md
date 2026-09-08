# Project State

Date: 2026-09-08
Phase: 2 — Audio & Voice
Overall status: IN PROGRESS
Repository: Gingerek/Jarvis
Working branch: phase2/audio-voice

## Completed gates
- Phase 0 Research/Audit: PASS.
- Phase 1 Foundation: PASS.

## Phase 2 completed so far
- NAudio 2.3.0 / WASAPI integrated.
- 3 active capture endpoints enumerated.
- active render endpoint enumeration implemented.
- Nor-Tec streaming mic selected and persisted by endpoint ID.
- cancellable WASAPI capture with deterministic disposal implemented.
- ring buffer and timestamped audio chunks implemented.
- 16 kHz mono PCM16 normalization implemented and tested.
- capture startup latency and callback jitter instrumentation implemented.
- preferred-device loss/reconnect monitor implemented.
- audio health probe implemented.

## Current validation
- solution projects: 21,
- Release build: PASS, 0 errors, 0 warnings,
- tests: PASS, 15 total,
- doctor: PASS, 7/7.

## Measured microphone result
- preferred: Mikrofon (Nor-Tec streaming mic),
- format: 48 kHz, 2 channel, 32-bit float,
- endpoint ID: {0.0.1.00000000}.{e40fea3e-eb20-4abb-93c2-ae05d304560b},
- first frame: 89.5 ms,
- mean callback interval: 62.5 ms,
- max measured jitter: 1.6 ms,
- observed RMS: 0.019478,
- observed peak: 0.076092.

Windows default `Microphone (USB Audio CODEC)` was nearly silent during the scan and is not used as Jarvis preferred input.

## Machine constraint
Ryzen 7 5700G, integrated Radeon, no NVIDIA/CUDA. VAD/ASR benchmark must prioritize CPU/ONNX/DirectML-capable paths.

## Still unresolved by design
- final VAD engine,
- final ASR backend/model,
- final wake-word engine,
- final Jarvis voice_id.
