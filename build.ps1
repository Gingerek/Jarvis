$ErrorActionPreference = 'Stop'
Set-Location $PSScriptRoot
Write-Host 'Jarvis build'
dotnet restore Jarvis.slnx
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
dotnet build Jarvis.slnx -c Release --no-restore
exit $LASTEXITCODE
