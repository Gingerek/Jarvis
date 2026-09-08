# Decisions

This file summarizes accepted decisions. Detailed rationale may live in docs/adr/.

## 2026-09-08 — Local-first Fast Path
Known daily commands execute locally through deterministic routing, context resolution and registered executors. LLM reasoning is not on the hot path.
Status: ACCEPTED

## 2026-09-08 — .NET 10 + WinUI 3
Use C#/.NET 10 LTS and WinUI 3 on Windows App SDK 2.4.0 as the primary Windows desktop stack.
Status: ACCEPTED

## 2026-09-08 — Development deployment
Use unpackaged + self-contained Windows App SDK deployment during development to preserve broad Win32/registry/filesystem access and simplify reproducible setup. Re-evaluate signed production packaging during hardening.
Status: ACCEPTED

## 2026-09-08 — API-first execution hierarchy
Official API/SDK > native Windows API/COM/WinRT > official plugin/extension > UI Automation > controlled input > vision/OCR.
Status: ACCEPTED

## 2026-09-08 — One speech output system
All spoken responses pass through one SpeechOutputManager and one selected voice identity. No unrelated robot-voice fallback is allowed.
Status: ACCEPTED

## 2026-09-08 — ASR selected by benchmark
Parakeet TDT 0.6B v3 and faster-whisper are candidates; no winner is selected before the actual hardware/microphone benchmark.
Status: ACCEPTED

## 2026-09-08 — Wake word remains pluggable
Do not lock production to Porcupine or bundled openWakeWord models until language, license and real-user accuracy gates pass.
Status: ACCEPTED

## 2026-09-08 — Browser Bridge
Chrome/Edge control uses Manifest V3 + Native Messaging to the normal user browser session rather than remote-debugging as the primary mechanism.
Status: ACCEPTED

## 2026-09-08 — OBS control
Use persistent authenticated obs-websocket 5.x connection.
Status: ACCEPTED

## 2026-09-08 — Lightroom and DaVinci truthfulness
Do not claim unsupported creative controls. Every capability is version-specific and labeled by actual integration method; installed editions/versions must be audited before implementation.
Status: ACCEPTED
