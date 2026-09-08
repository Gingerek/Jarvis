$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$targetDir = Join-Path $root 'models\silero-vad'
$target = Join-Path $targetDir 'silero_vad.onnx'
$url = 'https://raw.githubusercontent.com/snakers4/silero-vad/master/src/silero_vad/data/silero_vad.onnx'

New-Item -ItemType Directory -Force -Path $targetDir | Out-Null
if (-not (Test-Path $target)) {
    Invoke-WebRequest -Uri $url -OutFile $target
}

$hash = Get-FileHash $target -Algorithm SHA256
Write-Output "Silero VAD model: $target"
Write-Output "SHA256: $($hash.Hash)"
