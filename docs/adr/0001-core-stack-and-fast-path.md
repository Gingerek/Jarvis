# ADR-0001: Core Windows Stack and Local Fast Path

Date: 2026-09-08
Status: Accepted

## Context
Jarvis must be a low-latency Windows 11 desktop agent that can operate normal applications without sending every command to a remote LLM. It needs broad Win32 integration, modern UI, supervised local ML components and a plugin model.

## Decision
Use C# on .NET 10 LTS with WinUI 3 / Windows App SDK 2.4.0 for the primary application. During development, deploy unpackaged and self-contained. Use native Windows APIs/COM/WinRT first for system functions, UI Automation after official app/native APIs, and controlled input only as a fallback.

Known commands execute through a local deterministic Fast Path. ASR/wake/reasoning implementations are provider abstractions; exact ML backends are selected by measured hardware results.

Use Named Pipes as the default local IPC mechanism for supervised ML sidecars. Use one centralized SpeechOutputManager for all speech.

## Consequences
Positive:
- current supported Windows/.NET platform,
- broad desktop automation access,
- lower latency and lower cloud dependency,
- clear separation between deterministic control and reasoning,
- ML sidecars can change without rewriting the Windows product.

Trade-offs:
- unpackaged development lacks some package-identity-gated Windows features,
- production packaging/signing remains a later decision,
- some application integrations will still need version-specific UIA/shortcut fallbacks,
- local model choices cannot be finalized before the hardware audit.

## Rejected alternatives
- Python as the entire desktop application: rejected because ML dependency alone is not sufficient reason to move Windows host/UI/execution out of .NET.
- cloud LLM on every command: rejected for latency, reliability and unnecessary network dependence.
- coordinate-based mouse automation as primary integration: rejected as brittle.
- installing every possible integration dependency up front: rejected as unnecessary and risky.
