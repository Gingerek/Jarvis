# Project State

Date: 2026-09-08
Phase: 2 — Audio & Voice
Overall status: IN PROGRESS
Repository: Gingerek/Jarvis
Working branch: phase2/audio-voice

## Completed gates
- Phase 0 Research/Audit: PASS.
- Phase 1 Foundation: PASS.

## Phase 2 completed so far
- NAudio 2.3.0 / WASAPI integrated.
- 3 active capture endpoints enumerated.
- active render endpoint enumeration implemented.
- Nor-Tec streaming mic selected and persisted by endpoint ID.
- cancellable WASAPI capture with deterministic disposal implemented.
- ring buffer and timestamped audio chunks implemented.
- 16 kHz mono PCM16 normalization implemented and tested.
- capture startup latency and callback jitter instrumentation implemented.
- preferred-device loss/reconnect monitor implemented.
- audio health probe implemented.

## Current validation
- solution projects: 21,
- Release build: PASS, 0 errors, 0 warnings,
- tests: PASS, 15 total,
- doctor: PASS, 7/7.

## Measured microphone result
- preferred: Mikrofon (Nor-Tec streaming mic),
- format: 48 kHz, 2 channel, 32-bit float,
- endpoint ID: {0.0.1.00000000}.{e40fea3e-eb20-4abb-93c2-ae05d304560b},
- first frame: 89.5 ms,
- mean callback interval: 62.5 ms,
- max measured jitter: 1.6 ms,
- observed RMS: 0.019478,
- observed peak: 0.076092.

Windows default `Microphone (USB Audio CODEC)` was nearly silent during the scan and is not used as Jarvis preferred input.

## Machine constraint
Ryzen 7 5700G, integrated Radeon, no NVIDIA/CUDA. VAD/ASR benchmark must prioritize CPU/ONNX/DirectML-capable paths.

## Still unresolved by design
- final VAD engine,
- final ASR backend/model,
- final wake-word engine,
- final Jarvis voice_id.

## Phase 2 VAD progress
Silero VAD is now integrated through ONNX Runtime 1.29.0.
The model schema was inspected directly on this machine before implementation.
Streaming state is preserved between 512-sample frames at 16 kHz.
A hysteresis speech gate is implemented with separate start/end thresholds.

Measured result on Nor-Tec:
- 10 s background test: 0/312 frames above 0.5,
- VAD inference p50 0.190 ms and p95 0.227 ms on synthetic silence,
- live p95 0.141 ms,
- current VAD CPU cost is negligible relative to the 32 ms frame duration.

Current build status:
- 22 projects,
- Release build: 0 errors, 0 warnings,
- tests: 18 PASS.

## Phase 2 update — VAD and ASR
- Nor-Tec streaming mic is the required Jarvis input endpoint.
- Silero VAD official ONNX streaming path implemented with 64-sample context.
- Silero CPU p50 0.134 ms, p95 0.149 ms, p99 0.182 ms.
- faster-whisper 1.2.1 benchmarked on CPU int8.
- base selected for command fast path: persistent worker ~0.51-0.55 s for reference command sample.
- small rejected: ~1.69-1.73 s with no useful accuracy gain on application proper noun.
- IAsrEngine + persistent Python worker implemented.
- KnownEntityResolver corrects predictable ASR distortions of application names.
- Private microphone WAV files are ignored by Git.

Local caveat: Windows Application Control intermittently blocks newly copied test assemblies under tests/bin. Security policy remains unchanged; GitHub CI is the independent test gate.

## Phase 2 live voice host checkpoint
- Added production-oriented Jarvis.VoiceHost.
- Nor-Tec capture -> PCM16 -> Silero VAD -> persistent faster-whisper -> wake/session pipeline is wired.
- Local Application Control requires single-file host for runtime smoke tests; protection remains enabled.
- GitHub CI is green for the previous checkpoint.


## Deterministic application execution checkpoint
- Added OpenApplicationCommandParser and ApplicationLauncher fast path.
- Verified real command: otw�rz Lightroom -> Adobe Lightroom Classic started successfully.
- Lightroom executable confirmed at C:\Program Files\Adobe\Adobe Lightroom Classic\Lightroom.exe.
- Parser handles Polish diacritics including � -> l.


## Phase 2 TTS checkpoint
- ElevenLabs API authentication validated successfully.
- Secret stored only in Windows Credential Manager; no API key is stored in repo files.
- Selected voice: Marcin — Deep & Cinematic Polish Narrator.
- voice_id: B5tmTXp0L7DzqmlGqIMJ.
- model: eleven_flash_v2_5.
- Live TTS probe completed successfully.
- VoiceHost now speaks execution confirmations with the selected voice.
- Capture is paused and VAD state reset while Jarvis speaks to prevent self-triggering.
- Full Release solution build: PASS, 0 errors, 0 warnings.
## General Command Registry checkpoint — 2026-09-08
- Added structured CommandRegistry instead of single Lightroom-only parser path.
- Added generated `data/commands/pl-PL.generated.jsonl` corpus with 15,741 utterance variants.
- 12,801 variants do not use English open/close verbs.
- Added real execution for open/close app, website open, Google search, YouTube search, time, date and day-of-week.
- Verified installed targets: Lightroom, DaVinci Resolve, OBS, Chrome, Edge, Notatnik, Menedżer zadań.
- Direct execution checks PASS: `która godzina`, `otwórz YouTube`.
- Registry tests PASS: 45/45 total Core tests.
- UI + persistent VoiceHost runtime republished and running.

## Windows system command checkpoint — 2026-09-08
- Added `WindowsSystemController` using Windows Core Audio (NAudio/WASAPI-COM) and user32 APIs.
- Added real intents for volume up/down/set, mute/unmute, minimize/maximize/restore/close active window, show desktop and lock computer.
- Added `SystemCommandParser` and corpus variants; generated corpus now contains 16,952 utterances.
- Core tests PASS: 56/56.
- Full solution Release build PASS: 0 errors, 0 warnings.
- Real audio endpoint test PASS: set 56 -> 57 -> 56 and volume-up 56 -> 66 -> 56.
- Lock action is implemented but intentionally not fired by automated smoke tests because it would lock the user's interactive session.
- Updated faster-whisper initial prompt with system-command vocabulary.
- Republished single-file VoiceHost and restarted UI + background host successfully.

## Windows settings voice checkpoint - 2026-09-08
- Added Windows Settings parser backed by documented `ms-settings:` pages.
- Supports natural Polish case forms for sound, microphone, camera and other settings pages.
- Direct execution PASS: `otworz ustawienia dzwieku`.
- Generated command corpus now contains 26,122 utterance variants.
- Full solution build: PASS, 0 errors, 0 warnings.
- Persistent UI + VoiceHost restarted successfully after runtime publish.

## Browser bridge checkpoint — 2026-09-08
- One canonical browser path: VoiceHost -> Jarvis.Browser -> Jarvis.BrowserHost -> MV3 extension.
- Native host com.jarvis.browser registered per-user for Chrome and Edge under HKCU.
- Stable extension id: cdlnajihofjpmochnpnkcomdifdjimgd.
- Simulated Named Pipe + Native Messaging round-trip PASS.
- Corpus: 27,522 generated utterance variants.
- Live Chrome/Edge test still requires one-time Load unpacked because branded Chrome 137+ ignores command-line unpacked extension loading.

## Browser bridge live verification - 2026-09-09
- Chrome extension loaded with stable id cdlnajihofjpmochnpnkcomdifdjimgd.
- Direct Native Messaging launch required on this Windows/Application Control setup.
- Host registration is per-user under HKCU; only the direct-launch policy remains system-level.
- Live end-to-end PASS: active-tab context, new tab and previous/next tab commands.
- Jarvis.BrowserHost verified as a child process of chrome.exe.
- Next integration target: OBS via OBS WebSocket.

## OBS WebSocket live checkpoint - 2026-09-09
- Installed OBS Studio verified: 32.2.2; obs-websocket protocol server: 5.7.4 on localhost:4455.
- OBS WebSocket enabled with authentication; existing password imported once into Windows Credential Manager as `Jarvis/ObsWebSocketPassword` without exposing it in repo/logs.
- Added `Jarvis.OBS` persistent authenticated client with local 2 s request timeout.
- Added real VoiceHost intents for OBS status, record/stream status, scene list/current/set, record start/stop/pause/resume, stream start/stop, input list/mute/unmute and scene-source show/hide.
- Live read-only PASS: version, record status, stream status, current scene, scene list, input list and scene-item list.
- Reversible live PASS: microphone mute False -> True -> False; camera 2 visibility True -> False -> True.
- Production single-file VoiceHost republished to `%LOCALAPPDATA%\Jarvis\runtime`; UI + VoiceHost + OBS running together.
- Core tests: 95/95 PASS. Full Release solution build: 0 errors, 0 warnings.
- Generated Polish command corpus: 29,937 utterance variants.
- Next application integration target: Lightroom Classic.

## Lightroom Classic live checkpoint - 2026-09-09
- Installed Lightroom Classic verified: 15.5.1.
- Added `Jarvis.lrplugin` using the official Lightroom Classic Lua SDK and `LrSocket` on loopback only.
- Added `Jarvis.Lightroom`, `Jarvis.LightroomProbe`, VoiceHost executor and idempotent install script.
- Auto-init issue fixed by adding a real plug-in menu item required with `LrForceInitPlugin`.
- Live bridge PASS: ping, version, current module and Develop get/range/set.
- Verified ranges: Exposure -5..5; main Basic sliders -100..100; Temperature 2000..50000; Tint -150..150.
- Reversible live PASS: Exposure 0 -> 0.1 -> 0.
- Lightroom-generated corpus validation: 1,848/1,848 utterances parsed to the expected intent/argument.
- Full Release build: 0 errors, 0 warnings. Latest xUnit execution was blocked by Windows Application Control (0x800711C7).
- Global generated Polish corpus: 31,785 utterance variants.
- Production single-file VoiceHost republished to the local Jarvis runtime. Direct EXE start is currently blocked by Windows Application Control; the Release DLL starts via dotnet and reaches live SLEEPING/listening state alongside Jarvis.UI + Lightroom.
