# Jarvis Architecture

Status: PHASE 0 / architecture baseline
Date: 2026-09-08

## Product boundary
Jarvis is a local-first Windows 11 voice-operated desktop agent. Known commands must execute without a cloud LLM. LLMs are optional reasoning providers for ambiguous or multi-step work.

## Verified platform baseline
- Language/runtime: C# on .NET 10 LTS.
- UI: WinUI 3 on Windows App SDK 2.4.0.
- Development deployment: unpackaged + self-contained to preserve broad Win32, registry, filesystem, UI Automation and process access.
- Production packaging decision: deferred until hardening; evaluate MSIX with external location versus traditional signed installer.
- Audio capture/playback: WASAPI through NAudio 2.3.0 unless benchmark disproves it.
- IPC for local sidecars: Named Pipes first; localhost gRPC only if a measured need appears.
- Local state: SQLite, isolated from binaries under per-user AppData.
- Secrets: Windows Credential Manager / Win32 CredRead/CredWrite; never plaintext config.

## Process topology
1. Jarvis.App — WinUI shell and composition visual layer.
2. Jarvis.Host — long-running orchestration process and lifecycle owner.
3. Jarvis.Audio — WASAPI capture/playback, buffering and device management.
4. Jarvis.WakeWord — pluggable IWakeWordEngine.
5. Jarvis.VAD — local streaming VAD.
6. Jarvis.ASR — provider abstraction plus supervised ML sidecar when required.
7. Jarvis.Brain — intent router, entity extraction, context resolution and planning.
8. Jarvis.Execution — command dispatch and Windows executors.
9. Jarvis.Plugins — capability adapters for applications and services.
10. Jarvis.Speech — one SpeechOutputManager, one selected voice identity.
11. Jarvis.Security — policy, permissions, secrets, elevation broker.
12. Jarvis.Diagnostics — structured logs, metrics, traces and latency benchmarks.
13. Jarvis.Updater — versioned update/migration boundary.

## Fast Path
MICROPHONE -> VAD -> session/wake state -> ASR -> normalization -> intent router -> context resolver -> command registry -> executor -> result -> response formatter -> SpeechOutputManager.

A cloud LLM is not allowed on this path for known commands.

## Context model
The Context Manager is event-driven and maintains current_app, current_window, current_process, current_browser, current_tab, current_website, current_document, current_project, current_selection, current_media, previous_context and bounded context_history.

Foreground-window changes are observed through Win32 event hooks (SetWinEventHook / foreground events) and UI Automation events rather than heavy polling.

## Execution priority
For every capability, use this order:
1. official application API/SDK,
2. native Windows API / COM / WinRT,
3. official plugin/extension,
4. Windows UI Automation,
5. controlled keyboard/mouse input,
6. screen vision/OCR only as a last fallback.

## Browser architecture
Chromium Manifest V3 extension + Native Messaging host + Jarvis IPC. Keep a long-lived native messaging port for interactive sessions. Chrome and Edge use separate registry host locations but share nearly identical extension code.

## OBS architecture
Use obs-websocket 5.x directly over an authenticated persistent WebSocket connection. Do not drive OBS primarily through UI clicks.

## Lightroom Classic architecture
Use the official Lightroom Classic Lua SDK where capabilities are documented. Direct Develop-control automation is not assumed to be covered by the public SDK; each requested Develop capability receives a capability classification and fallback analysis before implementation.

## DaVinci Resolve architecture
Use the installed Resolve Developer/Scripting documentation as the source of truth for the installed edition/version. External scripting capability must be audited because it can differ between Free and Studio. UI Automation is fallback only for missing scripting capabilities.

## ASR selection
Do not hardcode an ASR winner before the hardware audit. Benchmark at minimum NVIDIA Parakeet TDT 0.6B v3 when NVIDIA hardware is compatible, and faster-whisper. Record latency, Polish command accuracy, CPU/GPU/RAM/VRAM and failure behavior.

## Wake word selection
Expose IWakeWordEngine. No production engine is selected yet. Porcupine requires an AccessKey and currently does not provide a Polish language model in its documented language list. openWakeWord code is Apache-2.0 but bundled pretrained models are CC BY-NC-SA 4.0. Production choice therefore requires both accuracy and license gates.

## Speech output
All modules return SpeechResponse objects to one SpeechOutputManager. The manager owns queueing, cancellation, barge-in, ducking, cache and audio playback. Initial cloud TTS candidate: ElevenLabs Flash v2.5 over streaming/WebSocket because it supports Polish and is optimized for low latency. Final voice_id is a user acceptance decision.

## Privilege model
Jarvis runs at normal user integrity. Privileged operations go through a narrow elevated helper with an explicit command contract. Never run the entire product permanently elevated.

## Non-negotiable invariants
- no fake UI controls,
- no per-command process spawning for hot-path integrations,
- no model reload per utterance,
- no 10,000 if/else rules,
- no secrets in Git/logs/plaintext,
- no hidden success when execution failed,
- no next phase before the current gate passes.
