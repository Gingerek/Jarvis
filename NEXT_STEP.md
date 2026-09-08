# Next Step

## Immediate objective
Finish the Phase 1 foundation gate, then start Phase 2 audio/voice work.

## Required next actions
1. Add startup/runtime smoke test automation where practical.
2. Extend diagnostics health checks for config, database, plugin directory and logs.
3. Add plugin manifest/version validation before loading arbitrary plugin assemblies.
4. Verify bootstrap.cmd/build.cmd/test.cmd/doctor.cmd on a clean shell.
5. Update CI to use the unpackaged/self-contained WinUI configuration.
6. Re-run clean restore/build/test/doctor.
7. Mark Phase 1 gate PASS and merge phase1/foundation.

## Gate to start Phase 2
- clean bootstrap on this machine,
- Release build 0 errors and 0 warnings,
- all tests pass,
- doctor pass,
- runtime UI smoke test pass,
- configuration/logging/database/plugin loader operational,
- repository state recorded,
- no application-specific Lightroom/OBS/YouTube feature code in foundation.

## Immediate next implementation after gate
Begin Phase 2 with audio device enumeration, microphone selection, WASAPI capture and latency measurement. Do not select final ASR or wake-word engine until measured benchmarks exist.
