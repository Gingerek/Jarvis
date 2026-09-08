# Jarvis

Jarvis is a local-first voice-operated Windows 11 desktop agent. The project goal is a real, extensible product: fast local command execution, application context, reliable automation, one consistent voice output pipeline and capability-specific integrations that prefer official APIs over screen clicking.

## Current status
Phase 0 — Research & Audit is in progress.

Read these first in every development/AI session:
1. PROJECT_STATE.md
2. ARCHITECTURE.md
3. NEXT_STEP.md
4. KNOWN_ISSUES.md (when present)

Then consult:
- DEPENDENCIES.md
- PLUGIN_MATRIX.md
- COMMAND_ARCHITECTURE.md
- LATENCY_PLAN.md
- TEST_PLAN.md
- ROADMAP.md

## Baseline stack
- C# / .NET 10 LTS
- WinUI 3 / Windows App SDK 2.4.0
- Windows 11
- local deterministic Fast Path for known commands
- pluggable ASR/wake/reasoning providers
- Named Pipes for local sidecar IPC by default
- SQLite for durable user data
- Windows Credential Manager for secrets

## Current gate
The real Windows machine audit must complete before Phase 1 Foundation begins. Hardware-dependent choices such as ASR backend, wake-word engine and installed-application capabilities are deliberately not guessed.

## Audit
`tools/phase0-audit.ps1` is a read-only inventory script. Its raw output goes to `audit-output/`, which is ignored by Git. Raw machine inventory must be reviewed and sanitized before any derived audit data is committed.

## Development rule
Repository state, tests and measured audit/benchmark results are the source of truth. Chat memory is not.
