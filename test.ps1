$ErrorActionPreference = 'Stop'
Set-Location $PSScriptRoot
Write-Host 'Jarvis tests'
dotnet test Jarvis.slnx -c Release --no-restore
exit $LASTEXITCODE
