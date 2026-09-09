# Next Step

## Current phase
Phase 2 — Audio, Voice & General Command Registry.

## Completed
- Nor-Tec WASAPI capture, PCM normalization and Silero VAD: PASS.
- persistent faster-whisper CPU int8 ASR: PASS.
- wake/session state machine for "Jarvis": implemented.
- persistent VoiceHost + WinUI Named Pipe integration: PASS.
- ElevenLabs streaming TTS + selected Marcin voice: PASS.
- Windows Credential Manager secret store: PASS.
- deterministic application execution: implemented.
- CommandRegistry introduced for general commands.
- generated command corpus: 31,785 utterance variants.
- app catalog: Lightroom, DaVinci Resolve, OBS, Chrome, Edge, Notatnik, Menedżer zadań.
- website actions: YouTube, Google, Marktplaats.
- Google/YouTube search intents: implemented.
- local time/date/day-of-week spoken answers: implemented.
- Windows system executor: volume, mute, active-window controls, desktop and lock.
- VoiceHost is explicitly Windows-only (`net10.0-windows`).
- known-folder executor: Pobrane, Dokumenty, Zdj?cia, Muzyka, Wideo, Pulpit and Jarvis project folder.
- Windows Settings intents backed by official `ms-settings:` pages: display, sound, Bluetooth, Wi-Fi, network, update, apps, storage, power, personalization, privacy, microphone, camera, notifications, clipboard and system info.

- Chrome/Edge MV3 browser bridge implemented: extension + Native Messaging host + Named Pipe client.
- browser intents implemented: tabs, history navigation, reload, scrolling, media toggle, tab mute and active-tab context.
- simulated native bridge round-trip: PASS (BRIDGE_OK=True).
- Core tests after browser parser: 77/77 PASS.
- BrowserHost and full solution build: 0 errors, 0 warnings.
- OBS WebSocket 5.x integration: LIVE PASS.
- OBS 32.2.2 / obs-websocket 5.7.4 authenticated locally on port 4455.
- OBS intents: status, recording/stream status, scenes, record start/stop/pause/resume, stream start/stop, input mute/unmute and source visibility.
- Reversible live control tests restored original microphone/source state.
- Lightroom Classic 15.5.1 Lua SDK bridge: LIVE PASS over loopback-only TCP.
- Lightroom Basic controls verified: Exposure, Contrast, Highlights, Shadows, Whites, Blacks, Texture, Clarity, Dehaze, Vibrance, Saturation, Temperature and Tint.
- Reversible live PASS: Exposure 0 -> 0.1 -> 0.
- Lightroom corpus/parser validation: 1,848/1,848 PASS.
- Release build: 0 errors / 0 warnings; Core 115/115, Structure 1/1, Integration 1/1 PASS; corpus: 32,044 variants.
- Lightroom navigation/rating/flags: LIVE PASS with original rating, flag and active photo restored after reversible tests.

## Verified system facts
- focused general/system/folder/settings parser tests: PASS; 31/31 latest focused suite.
- Full solution build: 0 errors, 0 warnings.
- `ustaw głośność na 57 procent`: PASS; measured endpoint changed 56 -> 57.
- volume restored exactly to 56 after the test.
- `głośniej`: PASS; measured endpoint changed 56 -> 66 and was restored to 56.
- `otw?rz Pobrane`: PASS.
- `otw?rz folder Jarvis`: PASS.
- `otw?rz ustawienia d?wi?ku`: PASS.
- UI + persistent VoiceHost republished and running with the new runtime.

## Immediate objective
Browser, OBS, Lightroom Basic and Lightroom navigation/rating/flags are PASS. Continue Lightroom Classic with copy/paste Develop, undo/redo, crop, masks and further SDK-backed capabilities before moving to DaVinci Resolve.

- Core tests: 119/119 PASS; corpus: 32,142 variants.
- Develop copy/paste and native undo/redo are LIVE PASS with restored state.
