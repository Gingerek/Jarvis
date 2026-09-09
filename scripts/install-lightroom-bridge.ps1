$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$source = Join-Path $root 'integrations\lightroom\Jarvis.lrplugin'
$modules = Join-Path $env:APPDATA 'Adobe\Lightroom\Modules'
$destination = Join-Path $modules 'Jarvis.lrplugin'

if (-not (Test-Path $source)) {
    throw "Jarvis Lightroom plug-in source not found: $source"
}

New-Item -ItemType Directory -Force -Path $modules | Out-Null
New-Item -ItemType Directory -Force -Path $destination | Out-Null
Copy-Item (Join-Path $source '*') $destination -Recurse -Force

Write-Host "Jarvis Lightroom Bridge installed: $destination"
Write-Host 'Restart Lightroom Classic to load the updated plug-in.'
