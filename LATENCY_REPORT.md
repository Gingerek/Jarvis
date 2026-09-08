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
