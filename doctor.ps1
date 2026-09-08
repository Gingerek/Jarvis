$ErrorActionPreference = 'Stop'
Set-Location $PSScriptRoot
$checks = [ordered]@{
  DotNetSdk = (dotnet --list-sdks | Select-String '^10\.0\.400') -ne $null
  Git = ((Get-Command git -ErrorAction SilentlyContinue) -ne $null) -or (Test-Path 'C:\Program Files\Git\cmd\git.exe')
  Python = (Get-Command python -ErrorAction SilentlyContinue) -ne $null
  Node = (Get-Command node -ErrorAction SilentlyContinue) -ne $null
  Winget = (Get-Command winget -ErrorAction SilentlyContinue) -ne $null
  Solution = Test-Path '.\Jarvis.slnx'
  UIProject = Test-Path '.\src\Jarvis.UI\Jarvis.UI.csproj'
}
$checks.GetEnumerator() | ForEach-Object { '{0}: {1}' -f $_.Key,$_.Value }
if ($checks.Values -contains $false) { exit 1 }
exit 0

