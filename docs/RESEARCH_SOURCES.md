# Phase 0 Research Sources

Verified 2026-09-08. Re-check before changing pinned production versions.

## Microsoft / Windows
- .NET support policy: https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core
  - .NET 10 is LTS; latest patch shown during research: 10.0.11; support ends 2028-11-14.
- Windows App SDK downloads: https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/downloads
  - stable 2.4.0 released 2026-08-13.
- Windows App SDK release notes: https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/release-notes/windows-app-sdk-2-0
- Windows SDK downloads: https://learn.microsoft.com/en-us/windows/apps/windows-sdk/downloads
  - Windows 11 SDK 10.0.28000.2705 listed for Aug 2026.
- Packaging overview: https://learn.microsoft.com/en-us/windows/apps/package-and-deploy/packaging/
- Unpackaged deployment: https://learn.microsoft.com/en-us/windows/apps/package-and-deploy/unpackage-winui-app
- UI Automation clients: https://learn.microsoft.com/en-us/windows/win32/winauto/uiauto-clientsoverview
- SetWinEventHook: https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setwineventhook
- Credential Manager APIs: https://learn.microsoft.com/en-us/windows/win32/api/wincred/

## Audio / VAD / ASR
- NAudio: https://github.com/naudio/NAudio
  - release 2.3.0 dated 2026-03-12; MIT.
- Silero VAD: https://github.com/snakers4/silero-vad
  - code/model project MIT; streaming ONNX supported; v6.x current family in researched documentation.
- faster-whisper: https://github.com/SYSTRAN/faster-whisper
  - MIT; current GPU path documents CUDA 12 + cuDNN 9 for latest CTranslate2.
- NVIDIA Parakeet TDT 0.6B v3: https://huggingface.co/nvidia/parakeet-tdt-0.6b-v3
  - supports Polish among 25 European languages; CC BY 4.0.

## TTS
- ElevenLabs model reference: https://elevenlabs.io/docs/overview/models
  - eleven_flash_v2_5 supports Polish and is optimized for ~75 ms model latency excluding app/network latency.
- Realtime TTS WebSocket: https://elevenlabs.io/docs/eleven-api/guides/how-to/websockets/realtime-tts
- Model selection: https://elevenlabs.io/docs/eleven-api/choosing-the-right-model
- Latency optimization: https://elevenlabs.io/docs/eleven-api/guides/how-to/best-practices/latency-optimization

## Wake word
- Picovoice Porcupine: https://picovoice.ai/docs/porcupine/
  - custom keywords, Windows support, AccessKey required; documented language list researched does not include Polish.
- Porcupine .NET: https://picovoice.ai/docs/quick-start/porcupine-dotnet/
- openWakeWord: https://github.com/dscripka/openWakeWord
  - code Apache-2.0; included pretrained models CC BY-NC-SA 4.0.

## Browser
- Chrome Native Messaging: https://developer.chrome.com/docs/extensions/develop/concepts/native-messaging
- Chrome Manifest V3: https://developer.chrome.com/docs/extensions/develop/migrate/what-is-mv3
- Microsoft Edge Native Messaging: https://learn.microsoft.com/en-us/microsoft-edge/extensions/developer-guide/native-messaging
- Edge Chrome-extension compatibility: https://learn.microsoft.com/en-us/microsoft-edge/extensions-chromium/developer-guide/port-chrome-extension

## OBS
- obs-websocket: https://github.com/obsproject/obs-websocket
  - included by default with OBS Studio 28+; 5.x default port 4455; authentication recommended.

## Adobe Lightroom Classic
- official Lightroom Classic SDK: https://developer.adobe.com/lightroom-classic/
  - Lua plugin SDK; documented strengths include export/publish, metadata and plugin UI.
- Adobe developer guidance warns against relying on undocumented APIs: https://developer.adobe.com/lightroom/lightroom-api-docs/getting-started/developer-guidelines/

## DaVinci Resolve
- Blackmagic developer guidance is bundled with installed Resolve under Help > Developer Documentation / Developer/Scripting.
- Blackmagic forum confirmation of installed developer docs and scripting access behavior: https://forum.blackmagicdesign.com/viewtopic.php?f=21&t=205175

## Research rule
A URL here proves only what its cited official/project documentation states. It does not prove a capability works on the user's installed version. Installed-version probes and end-to-end tests remain mandatory.
