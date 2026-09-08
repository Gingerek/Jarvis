# Plugin Matrix

Legend: VERIFIED = control method verified from vendor/platform documentation; AUDIT = installed version or machine must be checked; RESEARCH = capability-specific investigation required before implementation.

| Application / area | Capability | Primary control method | Fallback | Dependency | License/terms | Permissions | Expected Jarvis dispatch | Reliability target | Status |
|---|---|---|---|---|---|---|---|---|---|
| Windows | launch installed app | dynamic App Index + Win32/Shell activation | Start Menu/App Paths resolution | Windows APIs | OS | user | <=250 ms after transcript | high | VERIFIED design |
| Windows | foreground/window state | SetWinEventHook + Win32 window APIs | UIA focus/window events | User32 | OS | user | event-driven | high | VERIFIED design |
| Windows | UI controls | Microsoft UI Automation | controlled input | UIAutomationCore | OS | user; cross-elevation limits apply | command-dependent | medium-high | VERIFIED design |
| Windows | files | System.IO / Shell APIs | UIA in Explorer only when needed | .NET/Windows | OS/.NET | user | local I/O bound | high | VERIFIED design |
| Windows | clipboard | Win32/WinRT clipboard path | controlled input | Windows | OS | user | local | high | VERIFIED design |
| Windows | audio | Core Audio/WASAPI | UIA only for vendor-specific mixers | NAudio/Windows | MIT/OS | user | local | high | VERIFIED design |
| Windows | display/power/network | native Windows APIs | UIA/system settings only if API unavailable | Windows | OS | some operations may elevate | local | high where API exists | RESEARCH per action |
| Chrome | tabs/page control | Manifest V3 extension + Native Messaging long-lived port | UIA | Chromium extension APIs | platform terms | extension permissions | interactive | high | VERIFIED architecture |
| Edge | tabs/page control | Manifest V3 extension + Native Messaging long-lived port | UIA | Edge extension APIs | platform terms | extension permissions | interactive | high | VERIFIED architecture |
| YouTube | search/play/pause/seek/volume/title | Browser Bridge semantic DOM/media adapter | generic browser commands, then UIA | extension | platform/site terms | page host permission | interactive | high after adapter tests | RESEARCH selectors/API behavior |
| OBS Studio | record/stream/scenes/audio/status | obs-websocket 5.x persistent authenticated connection | UIA | built into OBS 28+ | OBS/obs-websocket terms | local WebSocket credential | interactive | high | VERIFIED architecture; AUDIT version |
| Lightroom Classic | export/publish/metadata/plugin UI | official Lightroom Classic Lua SDK | UIA/shortcuts | Adobe SDK | Adobe terms | plugin install | command-dependent | high where SDK documented | VERIFIED SDK scope |
| Lightroom Classic | next/previous/rating/flag/crop/navigation | documented SDK where available, otherwise shortcut/UIA capability map | controlled input | Adobe + Windows | Adobe/OS | user | interactive | medium-high | RESEARCH per command |
| Lightroom Classic | exposure/WB/shadows/highlights/etc. | only documented supported mechanism for installed version | UIA/shortcut or approved helper after research | Adobe | Adobe terms | user/plugin | interactive | must be measured | RESEARCH — do not claim native support |
| DaVinci Resolve | projects/timelines/media/render | installed Resolve Python/Lua Scripting API | UIA | Resolve Developer Scripting | Blackmagic terms | edition/version dependent | command-dependent | high for documented API | AUDIT edition/version |
| DaVinci Resolve | edit/color functions absent from scripting API | documented app shortcut/UIA capability | controlled input | Windows | Blackmagic/OS | user | interactive | medium | RESEARCH |
| Photoshop | document/edit automation | official Adobe automation/plugin API for installed version | UIA | Adobe | Adobe terms | app-specific | TBD | TBD | RESEARCH |
| Acrobat | document operations | official Acrobat APIs/automation where available | UIA | Adobe | Adobe terms | app-specific | TBD | TBD | RESEARCH |
| VLC | playback | documented remote/control interface or libVLC path | media keys/UIA | VLC | license review required per selected binding | user | interactive | high | RESEARCH |
| Spotify | playback | official Spotify API where permitted for desired action | media session/UIA | Spotify | Spotify terms | OAuth if API | network/local | medium-high | RESEARCH |
| Word | document automation | Office object model/COM | UIA | Microsoft Office | Microsoft terms | user | interactive | high | RESEARCH installed version |
| Excel | workbook automation | Office object model/COM | UIA | Microsoft Office | Microsoft terms | user | interactive | high | RESEARCH installed version |
| PowerPoint | presentation automation | Office object model/COM | UIA | Microsoft Office | Microsoft terms | user | interactive | high | RESEARCH installed version |
| Outlook | mail/calendar client automation | Microsoft-supported object model/Graph depending scenario | UIA | Microsoft | Microsoft terms | user/OAuth | network/local | high where supported | RESEARCH |
| Gmail | mail operations | Google API / connected auth | Browser Bridge | Google APIs | Google terms | OAuth | network | high | RESEARCH |
| Google Drive | file operations | Google Drive API / connected auth | Browser Bridge | Google APIs | Google terms | OAuth | network | high | RESEARCH |
| Google Calendar | calendar operations | Google Calendar API / connected auth | Browser Bridge | Google APIs | Google terms | OAuth | network | high | RESEARCH |
| Home Assistant | smart home | official Home Assistant REST/WebSocket API | none until integration verified | HA | project/license terms | token | network | high | OPTIONAL RESEARCH |

## Rules
- A row is not considered implemented until its health check and tests exist.
- Capability status is stored per installed application version.
- Fallback level must be visible in diagnostics.
- Vision/OCR is never silently promoted to primary control.
- If an app upgrade breaks selectors/API compatibility, the plugin health check must fail closed instead of pretending success.
