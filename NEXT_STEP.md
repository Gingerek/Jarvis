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
- generated command corpus: 15,741 utterance variants; 12,801 non-English-verb variants.
- real app catalog: Lightroom, DaVinci Resolve, OBS, Chrome, Edge, Notatnik, Menedżer zadań.
- real website actions: YouTube, Google, Marktplaats.
- web/YouTube search intents: implemented.
- local time/date/day-of-week spoken answers: implemented.
- graceful close-app execution: implemented.

## Current facts
- `otwórz YouTube` executes successfully.
- `która godzina` executes successfully.
- Core tests: 45/45 PASS after CommandRegistry changes.
- full solution build: 0 errors, 0 warnings before final runtime publish.
- Jarvis.UI and Jarvis.VoiceHost are running with the new runtime.

## Immediate objective
Expand registry from the current general-command pack into Windows audio/window/file controls, then browser navigation and OBS, while keeping every registered intent backed by a real executor.
