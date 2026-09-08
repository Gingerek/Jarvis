# Latency Report

Status: NO BENCHMARK DATA YET
Date initialized: 2026-09-08

This file contains measured results only. Vendor claims, estimates and single-run observations do not count as Jarvis benchmark results.

## Machine profile
PENDING_HARDWARE_AUDIT

## VAD
PENDING

## Wake word
PENDING

## ASR
PENDING

Candidates currently eligible for benchmark:
- NVIDIA Parakeet TDT 0.6B v3 if the audited NVIDIA hardware/software path is compatible.
- faster-whisper with model/compute type selected from audited hardware capability.

## Known-command routing
PENDING

Target:
- p50 <= 50 ms
- p95 <= 150 ms

## Known-command dispatch
PENDING

Target:
- p95 <= 250 ms after transcript availability

## Window focus / application activation
PENDING

Target for already-running app:
- p95 <= 300 ms when Windows foreground restrictions and the target application permit it.

## Browser Bridge
PENDING

## OBS WebSocket
PENDING

## TTS
PENDING

Initial candidate: ElevenLabs Flash v2.5 streaming/WebSocket. Record machine-observed time-to-first-playable-audio, not the vendor's model-only latency figure.

## Rules
- Report p50/p90/p95/p99/max and sample count.
- Report failure rate alongside latency.
- Separate Jarvis decision/dispatch time from application cold-start/completion time.
- Re-run benchmarks after material pipeline changes.

## Audio capture — 2026-09-08
Measured on preferred Nor-Tec streaming microphone via NAudio/WASAPI.

- native format: 48 kHz stereo 32-bit float,
- first audio callback: 89.5 ms,
- mean callback interval: 62.5 ms,
- maximum callback jitter in scan: 1.6 ms,
- observed RMS: 0.019478,
- observed peak: 0.076092.

Comparison during the same scan:
- USB Audio CODEC: first 98.7 ms, max jitter 2.9 ms, near-silent signal,
- Focusrite Analogue 1+2: first 61.0 ms, max jitter 1.9 ms, near-silent signal,
- Nor-Tec: first 89.5 ms, max jitter 1.6 ms, clearly active signal.

These are single scan observations, not percentile benchmark results. Repeat runs are required before declaring a latency gate.

## Silero VAD benchmark — 2026-09-08
Machine: Ryzen 7 5700G, CPU execution via ONNX Runtime 1.29.0.
Model: official Silero VAD ONNX, 16 kHz, 512-sample frames.

Synthetic silence, 1000 measured frames after warm-up:
- p50: 0.190 ms
- p90: 0.208 ms
- p95: 0.227 ms
- p99: 0.277 ms
- max: 0.430 ms

Nor-Tec live background, 10 s:
- 312 frames processed
- threshold: 0.50
- frames >= threshold: 0
- observed false-frame rate: 0.00%
- mean probability: 0.0005
- max probability: 0.0006
- live inference p50: 0.131 ms
- live inference p95: 0.141 ms
- live inference p99: 0.168 ms
- live inference max: 0.420 ms

## Silero VAD — measured 2026-09-08
Machine: Ryzen 7 5700G CPU, Nor-Tec streaming mic.
Model: official silero_vad.onnx, ONNX Runtime CPU.

CPU inference, 1000 x 512-sample frames:
- p50: 0.134 ms
- p95: 0.149 ms
- p99: 0.182 ms
- max: 0.614 ms

Live run, 466 frames:
- max speech probability: 1.000
- inference p50: 0.379 ms
- inference p95: 0.482 ms
- inference p99: 0.531 ms
- capture first callback: 97.8 ms
- capture callback max jitter: 12.7 ms
- real speech transitions detected: PASS

Note: speech-start wall-clock latency is not claimed because the exact human speech onset timestamp was not instrumented.

## ASR — measured 2026-09-08
Engine: faster-whisper 1.2.1 / CTranslate2 CPU int8, 8 threads.
Reference: local Polish TTS, 16 kHz mono PCM16, known command text.

### base
- runs: 0.565 / 0.536 / 0.532 s without domain prompt
- persistent C# worker: 0.553 / 0.529 / 0.507 s
- RTF: ~0.06
- text: all command semantics correct; Lightroom spelling distorted

### small
- runs: 1.729 / 1.698 / 1.694 s
- RTF: ~0.19
- did not improve Lightroom recognition

Decision for command fast path: faster-whisper base int8.
Known application names are corrected by local fuzzy entity resolution.
Larger model remains optional fallback for open-ended dictation, not fast command routing.
