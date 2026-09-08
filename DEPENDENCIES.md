# Dependencies and License Audit

Status: PHASE 0 baseline. Exact package pins are committed during Phase 1 bootstrap after machine audit.

| Component | Candidate | Verified status | License / commercial note | Decision |
|---|---|---|---|---|
| Runtime | .NET 10 LTS | Active; support through 2028-11-14 | Microsoft product terms | ACCEPT |
| Windows UI | Windows App SDK 2.4.0 / WinUI 3 | Stable release dated 2026-08-13 | Microsoft product terms | ACCEPT |
| Windows SDK | current stable Windows 11 SDK at bootstrap | 10.0.28000.2705 available Aug 2026 | Microsoft SDK terms | ACCEPT, verify installed toolchain |
| Audio | NAudio 2.3.0 | Released 2026-03-12; WASAPI/Core Audio support | MIT | ACCEPT |
| VAD | Silero VAD v6.x ONNX | Streaming; ONNX available | MIT | ACCEPT FOR BENCHMARK |
| ASR | faster-whisper | Active project; CUDA 12/cuDNN 9 requirements for current CTranslate2 GPU path | MIT | BENCHMARK |
| ASR | NVIDIA parakeet-tdt-0.6b-v3 | Polish among 25 European languages | CC BY 4.0 | BENCHMARK if compatible NVIDIA GPU |
| TTS | ElevenLabs eleven_flash_v2_5 | Polish; real-time model; documented ~75 ms model latency excluding network/app latency | proprietary API; commercial use requires appropriate plan | ACCEPT AS INITIAL CANDIDATE, user voice acceptance required |
| Browser control | Chromium Manifest V3 + Native Messaging | Supported by Chrome and Edge | browser platform terms | ACCEPT |
| OBS | obs-websocket 5.x protocol | Built into OBS 28+; persistent WebSocket; default port 4455 | server project GPL-2.0; Jarvis should consume protocol without copying GPL implementation | ACCEPT |
| Lightroom Classic | Adobe Lightroom Classic Lua SDK | Official SDK | Adobe SDK terms | ACCEPT WHERE DOCUMENTED |
| DaVinci Resolve | bundled Scripting API (Python/Lua) | Version/edition dependent; installed docs are source of truth | Blackmagic terms | AUDIT INSTALLED EDITION |
| Wake word | Picovoice Porcupine | Windows supported; custom keywords; AccessKey required | proprietary/account terms; documented language list currently lacks Polish | DO NOT LOCK IN |
| Wake word | openWakeWord | code Apache-2.0 | bundled pretrained models CC BY-NC-SA 4.0 | DEV/BENCHMARK ONLY unless commercial-safe model chain is proven |
| Data | SQLite via Microsoft-supported .NET provider | planned | provider license to pin in Phase 1 | ACCEPT AFTER PACKAGE PIN |
| IPC | System.IO.Pipes | built into .NET | .NET license | ACCEPT |
| Logging/config | Microsoft.Extensions.* where possible | built around supported .NET packages | .NET licenses | PREFERRED |

## Dependency rules
- Every production dependency is version-pinned.
- NuGet/Python/npm lock state must be reproducible.
- No package with non-commercial restrictions may enter production runtime without an explicit ADR and business decision.
- No dependency is installed merely because it might be useful later.
- Sidecars are allowed only where native .NET integration is materially worse or unavailable.
- Sidecars are supervised, hidden, restartable and keep models resident in memory.

## Current blockers
1. Hardware audit: Windows build, CPU, RAM, GPU, VRAM, audio devices, installed app versions.
2. Installed Lightroom Classic version and plugin SDK compatibility check.
3. Installed DaVinci Resolve edition/version and external scripting availability.
4. Wake-word commercial-safe implementation selection.
5. Final ElevenLabs voice_id and account/API authorization.
