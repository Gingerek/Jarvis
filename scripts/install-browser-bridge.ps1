param(
    [string]$Configuration = 'Release'
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$runtime = Join-Path $env:LOCALAPPDATA 'Jarvis\browser-host'
$hostExe = Join-Path $runtime 'Jarvis.BrowserHost.exe'
$manifestPath = Join-Path $runtime 'com.jarvis.browser.json'
$templatePath = Join-Path $root 'integrations\browser\native-host-manifest.template.json'

New-Item -ItemType Directory -Force $runtime | Out-Null

dotnet publish (Join-Path $root 'src\Jarvis.BrowserHost\Jarvis.BrowserHost.csproj') `
    -c $Configuration -r win-x64 --self-contained false `
    -p:PublishSingleFile=true -o $runtime
if ($LASTEXITCODE -ne 0) { throw 'BrowserHost publish failed.' }

$template = Get-Content $templatePath -Raw
$escapedHost = $hostExe.Replace('\', '\\')
$manifest = $template.Replace('__HOST_PATH__', $escapedHost)
[IO.File]::WriteAllText($manifestPath, $manifest, [Text.UTF8Encoding]::new($false))

$hostKeys = @(
    'HKCU:\Software\Google\Chrome\NativeMessagingHosts\com.jarvis.browser',
    'HKCU:\Software\Microsoft\Edge\NativeMessagingHosts\com.jarvis.browser'
)
foreach ($key in $hostKeys) {
    New-Item -Path $key -Force | Out-Null
    Set-Item -Path $key -Value $manifestPath
}

$directPolicy = Get-ItemPropertyValue `
    -Path 'HKLM:\SOFTWARE\Policies\Google\Chrome' `
    -Name 'NativeHostsExecutablesLaunchDirectly' `
    -ErrorAction SilentlyContinue

Write-Host "Jarvis BrowserHost installed: $hostExe"
Write-Host "Native manifest: $manifestPath"
Write-Host 'Registered for Chrome and Edge under HKCU.'
if ($directPolicy -ne 1) {
    Write-Warning 'If Chrome reports Failed to start native messaging host, run scripts\enable-browser-direct-launch.ps1 as Administrator and restart Chrome.'
}
Write-Host "Extension folder: $(Join-Path $root 'integrations\browser\extension')"
