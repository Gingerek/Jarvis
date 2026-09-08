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
- generated command corpus: 16,952 utterance variants.
- app catalog: Lightroom, DaVinci Resolve, OBS, Chrome, Edge, Notatnik, Menedżer zadań.
- website actions: YouTube, Google, Marktplaats.
- Google/YouTube search intents: implemented.
- local time/date/day-of-week spoken answers: implemented.
- Windows system executor: volume, mute, active-window controls, desktop and lock.
- VoiceHost is explicitly Windows-only (`net10.0-windows`).

## Verified system facts
- Core parser tests: 56/56 PASS.
- Full solution build: 0 errors, 0 warnings.
- `ustaw głośność na 57 procent`: PASS; measured endpoint changed 56 -> 57.
- volume restored exactly to 56 after the test.
- `głośniej`: PASS; measured endpoint changed 56 -> 66 and was restored to 56.
- UI + persistent VoiceHost republished and running with the new runtime.

## Immediate objective
Add safe file/folder/navigation commands and then browser navigation/media controls. Keep every registered utterance backed by a real executor; do not inflate corpus count with fake capabilities.
