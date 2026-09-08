# Changelog

## 2026-09-08 — Phase 1 foundation started
- completed live Windows hardware/software audit,
- installed Git 2.55.0.3 and .NET SDK 10.0.400,
- cloned repository locally,
- created phase1/foundation branch,
- created 20-project .NET 10 solution,
- created real WinUI 3 UI project on Windows App SDK 2.4.0,
- added pinned SDK and deterministic shared build settings,
- added bootstrap/build/test/doctor/update scripts,
- fixed WinUI Release build ReadyToRun issue for foundation stage,
- verified Release build: 0 warnings, 0 errors,
- verified baseline tests: 3 passed, 0 failed,
- verified doctor: all current checks pass.

## 2026-09-08 — Phase 0 baseline
- created research, architecture, dependency, plugin, latency and test documentation,
- defined local-first Fast Path architecture,
- added read-only Windows audit script,
- recorded unresolved ASR, wake-word, voice and application-integration decisions.

## 2026-09-08 � Phase 1 foundation increment
- added JarvisPaths and persistent AppData layout,
- added plugin contracts and PluginRegistry,
- added SQLite learning database schema,
- upgraded Microsoft.Data.Sqlite to 10.0.11 after blocking vulnerable 10.0.0 dependency,
- added CMD wrappers for locked-down PowerShell environments,
- replaced blocked Architecture.Tests assembly with Structure.Tests,
- Release build: 0 errors, 0 warnings; tests: 6 passed.

## 2026-09-08 — Phase 1 runtime foundation
- added persisted JarvisSettings and settings loader,
- added JSONL diagnostics logger,
- added plugin filesystem discovery loader,
- wired WinUI startup to Core, Diagnostics, Plugins and Learning,
- added configuration and plugin-loader tests,
- switched WinUI to unpackaged/self-contained deployment,
- verified runtime launch without Developer Mode,
- verified creation of settings.json, learning.db and JSONL log,
- validation now passes 20-project Release build, 8 tests and doctor 7/7.

## 2026-09-08 — Phase 1 gate passed
- added plugin manifest validation,
- added foundation health checks,
- added bootstrap.cmd and hardened bootstrap.ps1,
- updated CI with doctor step,
- switched clean-gate procedure to fresh bin/obj removal + restore for unpackaged WinUI,
- clean restore passed,
- Release build passed with 0 errors and 0 warnings,
- 9 tests passed,
- doctor passed 7/7,
- runtime smoke test passed after clean build,
- Phase 1 marked PASS; next phase is Audio & Voice.

## 2026-09-08 — Browser voice bridge
- Added MV3 extension and Native Messaging host for Chrome/Edge.
- Added browser command parser, VoiceHost executor and 27,522-utterance corpus.
- Added tab/navigation/scroll/media/mute/context actions.
- Browser protocol simulation PASS; live extension load pending one-time user install.
