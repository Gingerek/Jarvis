# Next Step

## Current phase
Phase 2 — Audio & Voice.

## Immediate objective
Validate Silero speech start/end behavior on controlled speech, then benchmark ASR candidates on this Ryzen 7 5700G.

## Completed
- WASAPI capture and device persistence,
- Nor-Tec selected as Jarvis microphone,
- 16 kHz mono PCM16 normalization,
- ring buffer, timestamps, reconnect and audio health,
- Silero VAD ONNX integration,
- VAD CPU and background false-trigger benchmark,
- hysteresis speech gate.

## Required next actions
1. Measure speech-start detection latency on controlled speech.
2. Measure speech-end detection latency with current 480 ms silence gate.
3. Tune thresholds only from measurements, not intuition.
4. Benchmark CPU-capable ASR candidates for Polish.
5. Record WER-like command accuracy and end-to-transcript latency.
6. Choose ASR backend only after benchmark comparison.
7. Continue to wake-word benchmark after ASR baseline is stable.

## Rules
No final ASR or wake-word selection without measured data and license review.
