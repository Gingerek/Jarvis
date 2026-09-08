# Project State

Date: 2026-09-08
Phase: 2 â€” Audio & Voice
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

## Phase 2 VAD progress
Silero VAD is now integrated through ONNX Runtime 1.29.0.
The model schema was inspected directly on this machine before implementation.
Streaming state is preserved between 512-sample frames at 16 kHz.
A hysteresis speech gate is implemented with separate start/end thresholds.

Measured result on Nor-Tec:
- 10 s background test: 0/312 frames above 0.5,
- VAD inference p50 0.190 ms and p95 0.227 ms on synthetic silence,
- live p95 0.141 ms,
- current VAD CPU cost is negligible relative to the 32 ms frame duration.

Current build status:
- 22 projects,
- Release build: 0 errors, 0 warnings,
- tests: 18 PASS.

## Phase 2 update â€” VAD and ASR
- Nor-Tec streaming mic is the required Jarvis input endpoint.
- Silero VAD official ONNX streaming path implemented with 64-sample context.
- Silero CPU p50 0.134 ms, p95 0.149 ms, p99 0.182 ms.
- faster-whisper 1.2.1 benchmarked on CPU int8.
- base selected for command fast path: persistent worker ~0.51-0.55 s for reference command sample.
- small rejected: ~1.69-1.73 s with no useful accuracy gain on application proper noun.
- IAsrEngine + persistent Python worker implemented.
- KnownEntityResolver corrects predictable ASR distortions of application names.
- Private microphone WAV files are ignored by Git.

Local caveat: Windows Application Control intermittently blocks newly copied test assemblies under tests/bin. Security policy remains unchanged; GitHub CI is the independent test gate.

## Phase 2 live voice host checkpoint
- Added production-oriented Jarvis.VoiceHost.
- Nor-Tec capture -> PCM16 -> Silero VAD -> persistent faster-whisper -> wake/session pipeline is wired.
- Local Application Control requires single-file host for runtime smoke tests; protection remains enabled.
- GitHub CI is green for the previous checkpoint.


## Deterministic application execution checkpoint
- Added OpenApplicationCommandParser and ApplicationLauncher fast path.
- Verified real command: otwórz Lightroom -> Adobe Lightroom Classic started successfully.
- Lightroom executable confirmed at C:\Program Files\Adobe\Adobe Lightroom Classic\Lightroom.exe.
- Parser handles Polish diacritics including ³ -> l.


## Phase 2 TTS checkpoint
- ElevenLabs API authentication validated successfully.
- Secret stored only in Windows Credential Manager; no API key is stored in repo files.
- Selected voice: Marcin â€” Deep & Cinematic Polish Narrator.
- voice_id: B5tmTXp0L7DzqmlGqIMJ.
- model: eleven_flash_v2_5.
- Live TTS probe completed successfully.
- VoiceHost now speaks execution confirmations with the selected voice.
- Capture is paused and VAD state reset while Jarvis speaks to prevent self-triggering.
- Full Release solution build: PASS, 0 errors, 0 warnings.