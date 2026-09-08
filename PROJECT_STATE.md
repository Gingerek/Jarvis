# Project State

Date: 2026-09-08
Phase: 1 — Foundation
Overall status: IN PROGRESS
Repository: Gingerek/Jarvis
Working branch: phase1/foundation

## Phase 0 result
PASS for hardware/software baseline and architecture start.

Measured machine:
- HP Pavilion Desktop TP01-2xxx
- Windows 11 Home build 26200 x64
- AMD Ryzen 7 5700G, 8C/16T
- 31.34 GB RAM
- AMD Radeon integrated graphics, no NVIDIA/CUDA
- Lightroom Classic 15.5.1
- DaVinci Resolve 21.0.3
- OBS Studio 32.2.2
- Chrome 152.0.7977.76
- Edge 152.0.4191.66
- Python 3.12.10, Node 24.15.0, Ollama 0.33.3

## Phase 1 completed so far
- Installed Git 2.55.0.3 and .NET SDK 10.0.400.
- Cloned Gingerek/Jarvis locally to C:\Users\rafal\Documents\Jarvis.
- Created branch phase1/foundation.
- Created Jarvis.slnx with 20 projects.
- Created real WinUI 3 Jarvis.UI on Windows App SDK 2.4.0.
- Added global.json pinned to .NET SDK 10.0.400.
- Added Directory.Build.props with warnings-as-errors and deterministic build.
- Added bootstrap.ps1, build.ps1, test.ps1, doctor.ps1 and update.ps1.
- Release build passes with 0 warnings and 0 errors.
- Test suite passes 3/3 baseline tests.
- Doctor passes all current foundation checks.

## Important architecture consequence
Parakeet/CUDA cannot be the primary ASR path on this machine because there is no NVIDIA GPU. ASR benchmark must prioritize CPU-capable or AMD/DirectML/ONNX-capable backends.

## Still unresolved by design
- final ASR backend,
- final wake-word engine,
- final Jarvis voice_id,
- final production packaging/signing,
- verified Lightroom Develop capability matrix,
- verified DaVinci scripting capability matrix.
