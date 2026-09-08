# Optional speech failure isolation

Local speech-host build: PASS, 0 warnings and 0 errors. Core.Tests: 39 PASS, 0 skipped. Four added cases cover provider failure without retry, success, propagated shutdown cancellation, and recovery after provider cancellation.

Without configured voice identity or readable credentials, the host continues with text responses. TTS provider errors do not terminate local command processing. The existing selected voice is retained; no alternate voice is introduced. Cooperative cancellation requests a stop after 15 seconds; an uncooperative provider is not forcibly killed.

This patch does not implement barge-in or echo cancellation. Capture remains paused during speech, which is an unresolved requirement gap. No live microphone recording, application launch or paid synthesis was performed to validate this patch. Package restore used the existing local NuGet cache; dependencies were not changed.

The existing PROJECT_STATE.md has mixed text encoding; this checkpoint was appended without rewriting historical content.

CI status: pending publication.


## Standard ASR dependency loading
Removed the fake av module from the ASR worker. The worker now imports the installed faster-whisper dependencies normally and respects dependency loading failures. VoiceHost accepts an optional --worker path so a reviewed worker can use the existing model/runtime directory without modifying that installation. Host Release build passed after this change. End-to-end wake and command acceptance remains pending.
