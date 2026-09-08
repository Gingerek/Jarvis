# Project State

Date: 2026-09-08
Phase: 0 — Research & Audit
Overall status: IN PROGRESS
Repository: Gingerek/Jarvis
Default branch: main

## Completed in this phase
- Verified .NET 10 as current LTS baseline.
- Verified current stable Windows App SDK 2.4.0 / WinUI 3 baseline.
- Selected local-first modular architecture and Fast Path rules.
- Defined unpackaged + self-contained development deployment baseline.
- Verified NAudio 2.3.0 / MIT as initial WASAPI wrapper candidate.
- Verified Silero VAD v6.x / MIT as VAD benchmark candidate.
- Verified faster-whisper / MIT and NVIDIA Parakeet TDT 0.6B v3 / CC BY 4.0 as ASR benchmark candidates.
- Verified Parakeet v3 supports Polish.
- Verified ElevenLabs Flash v2.5 supports Polish and low-latency streaming; final voice_id remains a user acceptance item.
- Verified Chrome/Edge Manifest V3 Native Messaging architecture.
- Verified obs-websocket 5.x is built into OBS 28+ and should be primary OBS integration.
- Verified official Lightroom Classic Lua SDK exists, but did NOT assume undocumented direct Develop slider control.
- Confirmed DaVinci scripting must be checked against the installed edition/version and bundled Developer documentation.
- Rejected premature wake-word lock-in because current candidates have language/license/account constraints.
- Created architecture, dependency, command, plugin, latency and test documentation.

## Current blocker
The authorized Remote Desktop Commander device named "Rafal" is offline. Therefore the required live hardware/software audit has not yet been executed. No CPU/GPU/VRAM, installed app version, microphone, browser, Lightroom, Resolve or OBS version is being guessed.

## Phase 0 pending work
1. Connect to the real Windows 11 machine.
2. Execute automated hardware/software inventory.
3. Commit HardwareProfile.json and InstalledApps.json only after removing private/sensitive values that do not belong in Git.
4. Check actual OBS, Lightroom Classic, DaVinci Resolve, Chrome/Edge and audio device versions.
5. Check GPU driver/CUDA compatibility before ASR benchmark setup.
6. Complete wake-word benchmark candidates and licensing gate.
7. Reconcile PLUGIN_MATRIX.md and DEPENDENCIES.md with actual installed software.
8. Mark Phase 0 gate PASS only after audit results are recorded.

## Decisions intentionally NOT made yet
- final ASR backend/model/compute type,
- final wake-word backend,
- final ElevenLabs voice_id,
- final production installer/packaging model,
- claimed support for Lightroom Develop controls,
- claimed DaVinci external scripting capability,
- local LLM model/provider.

These decisions depend on measurements, installed versions, licensing or user acceptance and must not be guessed.
