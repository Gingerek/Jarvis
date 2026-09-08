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
- generated command corpus: 26,122 utterance variants.
- app catalog: Lightroom, DaVinci Resolve, OBS, Chrome, Edge, Notatnik, Menedżer zadań.
- website actions: YouTube, Google, Marktplaats.
- Google/YouTube search intents: implemented.
- local time/date/day-of-week spoken answers: implemented.
- Windows system executor: volume, mute, active-window controls, desktop and lock.
- VoiceHost is explicitly Windows-only (`net10.0-windows`).
- known-folder executor: Pobrane, Dokumenty, Zdj?cia, Muzyka, Wideo, Pulpit and Jarvis project folder.
- Windows Settings intents backed by official `ms-settings:` pages: display, sound, Bluetooth, Wi-Fi, network, update, apps, storage, power, personalization, privacy, microphone, camera, notifications, clipboard and system info.

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
Implement the proper Chrome/Edge MV3 + Native Messaging browser bridge, then OBS integration. Do not regress to blind keyboard/mouse control when a browser extension API can execute the action. Keep every registered utterance backed by a real executor; do not inflate corpus count with fake capabilities.
