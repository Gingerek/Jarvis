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
