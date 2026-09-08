# Known Issues

## KI-001 — Live Windows audit blocked
Severity: BLOCKER for Phase 0 gate
Status: OPEN

The authorized Remote Desktop Commander device "Rafal" is currently offline. Hardware/software facts must not be guessed. Phase 1 cannot start until the real machine audit completes.

## KI-002 — Wake-word production backend unresolved
Severity: HIGH
Status: OPEN

Porcupine requires an AccessKey and its documented language list currently does not include Polish. openWakeWord code is Apache-2.0 but bundled pretrained wake models are CC BY-NC-SA 4.0. A production-safe wake implementation must pass both license and accuracy gates.

## KI-003 — Lightroom Develop capability not yet proven
Severity: HIGH
Status: OPEN

The official Lightroom Classic Lua SDK is verified, but direct control of all requested Develop sliders must not be assumed. Installed Lightroom version and each capability require research/testing before support is claimed.

## KI-004 — DaVinci external scripting depends on edition/version
Severity: HIGH
Status: OPEN

Resolve scripting support and external access must be checked against the actual installed edition/version and bundled Developer/Scripting documentation. Do not assume Studio-level external scripting.

## KI-005 — Final ASR backend unresolved
Severity: MEDIUM
Status: OPEN

Parakeet and faster-whisper are candidates. The winner depends on actual GPU/VRAM/driver/microphone measurements and Polish command accuracy.

## KI-006 — Final TTS voice identity unresolved
Severity: MEDIUM
Status: OPEN

ElevenLabs Flash v2.5 is the initial real-time TTS candidate, but final voice_id requires user listening/acceptance and valid service authorization.
