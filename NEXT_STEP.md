# Next Step

## Immediate objective
Finish the Phase 1 foundation gate before starting audio/ASR implementation.

## Required next actions
1. Add CI for restore, Release build and tests.
2. Verify bootstrap.ps1 is safe to run repeatedly.
3. Add real architecture tests instead of template placeholder tests.
4. Add initial project references enforcing Core/Execution/Plugin boundaries.
5. Add configuration and logging abstractions without application-specific code.
6. Add SQLite foundation for learning/config metadata.
7. Add plugin contracts and plugin loader skeleton.
8. Add diagnostics contracts and health-check model.
9. Run clean restore/build/test/doctor again.
10. Commit and push phase1/foundation.

## Gate to start Phase 2
- clean bootstrap on this machine,
- Release build 0 errors,
- tests pass,
- doctor pass,
- repository state recorded,
- no Lightroom/OBS/YouTube feature code in foundation.

## Next session rule
Read PROJECT_STATE.md, ARCHITECTURE.md, NEXT_STEP.md and KNOWN_ISSUES.md before changing code.
