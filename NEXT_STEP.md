# Next Step

## Immediate objective
Finish Phase 0 with a real machine audit. Do not start the product solution before this gate is complete.

## When the authorized Windows device is online
1. Verify Remote Desktop Commander connectivity and latency.
2. Run the Phase 0 audit script from the repository.
3. Capture:
   - Windows edition/build/architecture,
   - CPU and logical/physical cores,
   - RAM,
   - GPU(s), VRAM and driver versions,
   - microphone and playback devices,
   - default audio endpoints where obtainable,
   - installed applications and versions,
   - OBS Studio version,
   - Lightroom Classic version,
   - DaVinci Resolve version and Free/Studio clues,
   - Chrome/Edge versions,
   - .NET/Windows SDK/Visual Studio/Git/Python/Node installations,
   - application executable paths,
   - MSIX/AppX package inventory,
   - Start Menu entries,
   - App Paths registry entries,
   - currently running processes relevant to integration.
4. Store sanitized audit output as HardwareProfile.json and InstalledApps.json.
5. Update DEPENDENCIES.md and PLUGIN_MATRIX.md from measured facts.
6. Run capability probes:
   - OBS WebSocket availability/auth state,
   - Lightroom plugin SDK path/version evidence,
   - Resolve Developer/Scripting files and edition capability,
   - browser Native Messaging prerequisites,
   - CUDA/cuDNN/driver compatibility if NVIDIA GPU is present.
7. Decide which ASR candidates are technically runnable.
8. Define wake-word benchmark candidates that pass the commercial/license gate.
9. Update PROJECT_STATE.md with measured results.
10. Only then mark Phase 0 PASS and start Phase 1 Foundation.

## Phase 1 first commit after the gate
Create the .NET 10 solution with WinUI 3 / Windows App SDK 2.4.0, core project boundaries, deterministic build/bootstrap/doctor scripts and CI. Do not add Lightroom/OBS/YouTube feature code to the first foundation commit.

## Rule for the next AI/developer session
Read PROJECT_STATE.md, ARCHITECTURE.md, NEXT_STEP.md, KNOWN_ISSUES.md if present, and the latest audit outputs before making changes. Never rely on chat memory as the project source of truth.
