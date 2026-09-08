$ErrorActionPreference = 'Stop'
$required = @(
  @{ Id='Git.Git'; Check='git' },
  @{ Id='Microsoft.DotNet.SDK.10'; Check='dotnet' }
)
foreach ($item in $required) {
  if (-not (Get-Command $item.Check -ErrorAction SilentlyContinue)) {
    winget install --id $item.Id -e --accept-package-agreements --accept-source-agreements
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
  }
}
Set-Location $PSScriptRoot
dotnet restore Jarvis.slnx
exit $LASTEXITCODE
