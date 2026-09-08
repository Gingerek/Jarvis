$ErrorActionPreference = 'Stop'
Set-Location $PSScriptRoot
$git = (Get-Command git -ErrorAction SilentlyContinue).Source
if (-not $git) { $git = 'C:\Program Files\Git\cmd\git.exe' }
& $git pull --ff-only
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
& .\build.ps1
exit $LASTEXITCODE
