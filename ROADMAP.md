# Jarvis Roadmap

Every phase is gated. A later phase does not begin because the previous one merely looks plausible.

## Phase 0 — Research & Audit [IN PROGRESS]
Deliver architecture, dependency/license audit, plugin matrix, command architecture, latency plan, test plan and machine audit.

Gate: hardware/software audit complete; unresolved high-risk architecture items explicitly classified; Phase 0 docs reviewed for internal consistency.

## Phase 1 — Foundation
Create .NET solution and projects, CI, bootstrap.ps1, build.ps1, test.ps1, doctor.ps1, update.ps1, config, structured logging, SQLite data layer, plugin loader, dependency manager, UI shell and updater boundaries.

Gate: clean checkout/bootstrap/build succeeds and bootstrap is idempotent.

## Phase 2 — Audio Input
WASAPI capture, buffering, VAD, wake/session manager, ASR benchmark and selected resident ASR backend.

Gate: stable Polish command recognition on the real microphone with latency report.

## Phase 3 — Local Brain
Normalizer, Command Registry, Entity Extractor, Context Resolver, deterministic router, semantic matcher provider, Fast Path, >=10,000 validated Polish utterances.

Gate: held-out command classification and context tests meet target; known commands are network/LLM independent.

## Phase 4 — Speech Output
One SpeechOutputManager, streaming TTS, one voice_id, cancellation, barge-in, ducking and same-voice cache.

Gate: user accepts the voice; no overlap in prolonged interruption/stress tests.

## Phase 5 — Windows Core
App index, launch/focus/window/process/file/clipboard/audio/display/power/network/screenshot/search capabilities, UI Automation and controlled input fallback.

Gate: daily Windows command suite passes reliably.

## Phase 6 — Browser + YouTube
Manifest V3 Chromium extension, Chrome/Edge Native Messaging host, browser context, semantic page adapter and YouTube capabilities.

Gate: mandatory YouTube scenario passes repeatedly without repeated context words.

## Phase 7 — OBS
obs-websocket 5.x adapter with persistent authenticated connection, status, recording, streaming, scenes, sources and audio.

Gate: real OBS state confirms each action; restart/reconnect tests pass.

## Phase 8 — Lightroom Classic
Installed-version capability research, official Lua SDK plugin where supported, navigation/metadata/export features, Develop-control capability map, UIA/shortcut fallback only where justified.

Gate: agreed real editing workflow works voice-first and every capability has a truthful method/status.

## Phase 9 — DaVinci / Creative
DaVinci first, then Photoshop/Acrobat/VLC and other creative apps one at a time: research -> adapter -> tests -> acceptance.

## Phase 10 — Office / Web / Other
Word, Excel, PowerPoint, Outlook, Gmail, Drive, Calendar and detected applications, each through the strongest supported integration method.

## Phase 11 — Learning Engine
Persistent synonym learning, safe typed workflow learning and custom commands stored as data.

Gate: learned behavior survives restart/update and cannot inject arbitrary executable code.

## Phase 12 — Hardening
Least privilege, elevation broker, recovery/watchdogs, crash handling, migrations, memory/startup/performance, installer, signing and release pipeline.

## Phase 13 — Production Acceptance
Multi-hour soak tests, regression suite, failure injection, upgrade/recovery tests and final latency/reliability report.

Gate: core workflows stable enough for normal daily use; no known blocker disguised as a limitation note.
