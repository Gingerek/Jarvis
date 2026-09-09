Option Explicit
Dim sh, fso, ui, cmd
Set sh = CreateObject("WScript.Shell")
Set fso = CreateObject("Scripting.FileSystemObject")
ui = sh.ExpandEnvironmentStrings("%LOCALAPPDATA%") & "\Jarvis\runtime\ui\Jarvis.UI.exe"
If Not fso.FileExists(ui) Then
  MsgBox "Jarvis.UI nie jest zainstalowany: " & ui, 16, "Jarvis"
  WScript.Quit 2
End If
cmd = Chr(34) & ui & Chr(34)
sh.Run cmd, 1, False
