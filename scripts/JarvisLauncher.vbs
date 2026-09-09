Option Explicit
Dim sh, fso, svc, items, p, ui, cmd
Set sh = CreateObject("WScript.Shell")
Set fso = CreateObject("Scripting.FileSystemObject")
Set svc = GetObject("winmgmts:\\.\root\cimv2")
Set items = svc.ExecQuery("SELECT * FROM Win32_Process WHERE Name='Jarvis.UI.exe'")
If items.Count > 0 Then WScript.Quit 0
For Each p In svc.ExecQuery("SELECT * FROM Win32_Process WHERE Name='dotnet.exe'")
  If InStr(1, LCase(p.CommandLine & ""), "jarvis.voicehost.dll", 1) > 0 Then p.Terminate
Next
ui = sh.ExpandEnvironmentStrings("%LOCALAPPDATA%") & "\Jarvis\runtime\ui\Jarvis.UI.exe"
If Not fso.FileExists(ui) Then
  MsgBox "Jarvis.UI nie jest zainstalowany: " & ui, 16, "Jarvis"
  WScript.Quit 2
End If
cmd = Chr(34) & ui & Chr(34)
sh.Run cmd, 1, False
