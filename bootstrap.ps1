$ErrorActionPreference = 'Stop'

function Test-GitAvailable {
  if (Get-Command git -ErrorAction SilentlyContinue) { return $true }
  return Test-Path 'C:\Program Files\Git\cmd\git.exe'
}

if (-not (Test-GitAvailable)) {
  winget install --id Git.Git -e --accept-package-agreements --accept-source-agreements
  if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
}

$sdks = dotnet --list-sdks 2>$null
if (-not ($sdks -match '^10\.0\.400 ')) {
  winget install --id Microsoft.DotNet.SDK.10 -e --accept-package-agreements --accept-source-agreements
  if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
}

Set-Location $PSScriptRoot
dotnet restore Jarvis.slnx
exit $LASTEXITCODE
