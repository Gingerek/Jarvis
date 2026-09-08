# Test Plan

## Test layers
1. Unit tests — normalization, slot parsing, context rules, risk policy, state machines.
2. Integration tests — audio pipeline, IPC, SQLite, plugin loading, browser bridge, OBS connection.
3. Plugin tests — capability-specific commands against supported app versions.
4. UI Automation tests — selector stability, pattern invocation, failure behavior.
5. Voice tests — VAD, wake word, ASR, barge-in, endpointing.
6. Latency tests — percentile budgets from LATENCY_PLAN.md.
7. Regression tests — every fixed production bug receives a reproducible test when practical.
8. End-to-end tests — real applications and real microphone.
9. Soak tests — long running sessions, reconnections, failures and repeated context changes.

## Required Phase 0 checks
- Architecture decisions are explicit.
- Dependencies have license/status notes.
- Plugin control methods are classified.
- Hardware-dependent decisions are marked pending instead of guessed.
- Latency boundaries and acceptance metrics are defined.

## Phase 1 acceptance
- clean checkout can bootstrap and build,
- bootstrap.ps1 is idempotent,
- doctor.ps1 reports missing components precisely,
- app launches without a console window,
- plugin loader loads zero/one/many plugins safely,
- configuration and SQLite survive restart,
- no secret is present in repository or ordinary logs.

## Phase 2 voice acceptance
- microphone capture stable for extended run,
- wake/session lifecycle deterministic,
- Polish ASR benchmark recorded,
- selected model remains resident,
- no model reload per command,
- endpointing does not routinely clip short commands.

## Phase 3 brain acceptance
- >=10,000 verified utterance corpus,
- held-out validation/test split,
- confusion matrix and per-intent metrics,
- known commands route without LLM/network,
- context-dependent commands pass targeted tests,
- unknown/ambiguous commands fail safely or escalate to reasoning.

## Phase 4 speech acceptance
- exactly one SpeechOutputManager controls all spoken output,
- no overlapping speech during stress test,
- stop/cicho/przestań interrupts immediately within measured budget,
- selected voice accepted by user,
- cached phrases use the same voice identity/profile.

## Windows core acceptance
Test launch/focus/minimize/maximize/close/switch, volume, clipboard, files, screenshots and foreground-context detection. Verify failure behavior with missing apps, elevation mismatch, locked files and unavailable devices.

## Browser/YouTube mandatory scenario
1. "Jarvis"
2. "Włącz YouTube"
3. "Znajdź film o Warszawie"
4. "Włącz pierwszy"
5. "Pauza"
6. "Wznów"
7. "Przewiń do piątej minuty"
8. "Uruchom OBS"
9. "Włącz nagrywanie"
10. "Wróć do YouTube"

Expected: context changes correctly and the sequence works without repeating Jarvis/YouTube/OBS before every command.

## OBS acceptance
- authenticated persistent connection,
- recording/streaming state reflects actual OBS state,
- scenes and source audio commands verified semantically,
- reconnect after OBS restart,
- wrong password produces actionable doctor result rather than fake success.

## Lightroom acceptance
Each capability has a declared method (NATIVE/SDK/UIA/SHORTCUT/EXPERIMENTAL/UNAVAILABLE). Tests verify actual photo/catalog state after the command. No capability is marked supported because a click was merely attempted.

## DaVinci acceptance
Tests are edition/version specific. External scripting is not assumed. Each supported action must verify project/timeline/render state through the scripting API when available; fallback UI operations require selector/shortcut regression tests.

## Destructive-action tests
High-risk operations must exercise confirmation/policy gates, cancellation, target validation and audit logging. Tests must prove that ambiguous targets are not acted upon.

## Soak test before production
Run multi-hour sessions including hundreds of commands, app switches, TTS interruptions, internet loss, TTS outage, plugin crash/restart, ASR errors, missing apps, window closures and context changes. Record memory growth, handles, CPU/GPU use, false wake events, failures and latency drift.
