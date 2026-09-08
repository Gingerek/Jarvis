#requires -Version 5.1
[CmdletBinding()]
param(
    [string]$OutputDirectory = ""
)

$ErrorActionPreference = 'SilentlyContinue'
$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = Split-Path -Parent $scriptRoot
if ([string]::IsNullOrWhiteSpace($OutputDirectory)) {
    $OutputDirectory = Join-Path $repoRoot 'audit-output'
}
New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null

function To-Text([object]$value) {
    if ($null -eq $value) { return $null }
    return [string]$value
}

function Get-CommandVersion([string]$name, [string[]]$args) {
    $cmd = Get-Command $name -ErrorAction SilentlyContinue
    if (-not $cmd) { return $null }
    $output = & $cmd.Source @args 2>&1 | Select-Object -First 8
    return [ordered]@{
        Path = $cmd.Source
        VersionOutput = ($output -join "`n")
    }
}

function Get-UninstallApps {
    $roots = @(
        'HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\*',
        'HKLM:\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\*',
        'HKCU:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\*'
    )
    $items = foreach ($root in $roots) {
        Get-ItemProperty $root | Where-Object { $_.DisplayName } | ForEach-Object {
            [pscustomobject]@{
                Name = $_.DisplayName
                Version = $_.DisplayVersion
                Publisher = $_.Publisher
                InstallLocation = $_.InstallLocation
                DisplayIcon = $_.DisplayIcon
                UninstallKey = $_.PSPath
            }
        }
    }
    return $items | Sort-Object Name, Version -Unique
}

function Get-AppPathEntries {
    $roots = @(
        'HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\*',
        'HKCU:\SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\*'
    )
    $items = foreach ($root in $roots) {
        Get-ItemProperty $root | ForEach-Object {
            [pscustomobject]@{
                Key = $_.PSChildName
                Executable = $_.'(default)'
                Path = $_.Path
            }
        }
    }
    return $items | Where-Object { $_.Key } | Sort-Object Key -Unique
}

function Get-StartMenuEntries {
    $locations = @(
        "$env:ProgramData\Microsoft\Windows\Start Menu\Programs",
        "$env:APPDATA\Microsoft\Windows\Start Menu\Programs"
    )
    $shell = New-Object -ComObject WScript.Shell
    $entries = foreach ($location in $locations) {
        if (-not (Test-Path $location)) { continue }
        Get-ChildItem -Path $location -Filter *.lnk -Recurse -File | ForEach-Object {
            $shortcut = $shell.CreateShortcut($_.FullName)
            [pscustomobject]@{
                Name = $_.BaseName
                Shortcut = $_.FullName
                Target = $shortcut.TargetPath
                Arguments = $shortcut.Arguments
            }
        }
    }
    return $entries | Sort-Object Name, Target -Unique
}

function Convert-MonitorString($arr) {
    if ($null -eq $arr) { return $null }
    $chars = $arr | Where-Object { $_ -ne 0 } | ForEach-Object { [char]$_ }
    return -join $chars
}

$os = Get-CimInstance Win32_OperatingSystem
$computer = Get-CimInstance Win32_ComputerSystem
$cpus = Get-CimInstance Win32_Processor
$gpus = Get-CimInstance Win32_VideoController
$memoryModules = Get-CimInstance Win32_PhysicalMemory
$audioDevices = Get-CimInstance Win32_SoundDevice
$audioEndpoints = Get-PnpDevice -Class AudioEndpoint
$monitorsRaw = Get-CimInstance -Namespace root\wmi -ClassName WmiMonitorID
$installedApps = @(Get-UninstallApps)
$appPaths = @(Get-AppPathEntries)
$startMenu = @(Get-StartMenuEntries)

$monitors = @($monitorsRaw | ForEach-Object {
    [pscustomobject]@{
        Manufacturer = Convert-MonitorString $_.ManufacturerName
        ProductCode = Convert-MonitorString $_.ProductCodeID
        SerialNumber = Convert-MonitorString $_.SerialNumberID
        FriendlyName = Convert-MonitorString $_.UserFriendlyName
        Active = $_.Active
    }
})

$browserChoice = Get-ItemProperty 'HKCU:\Software\Microsoft\Windows\Shell\Associations\UrlAssociations\http\UserChoice'

$nvidia = $null
if (Get-Command nvidia-smi -ErrorAction SilentlyContinue) {
    $gpuRows = & nvidia-smi --query-gpu=name,driver_version,memory.total --format=csv,noheader,nounits 2>&1
    $nvidiaHeader = & nvidia-smi 2>&1 | Select-Object -First 12
    $nvidia = [ordered]@{
        Query = @($gpuRows)
        Header = ($nvidiaHeader -join "`n")
    }
}

$developerTools = [ordered]@{
    Git = Get-CommandVersion 'git' @('--version')
    DotNet = Get-CommandVersion 'dotnet' @('--info')
    PowerShell7 = Get-CommandVersion 'pwsh' @('--version')
    Python = Get-CommandVersion 'python' @('--version')
    Python3 = Get-CommandVersion 'python3' @('--version')
    Node = Get-CommandVersion 'node' @('--version')
    Npm = Get-CommandVersion 'npm' @('--version')
    Winget = Get-CommandVersion 'winget' @('--version')
    CMake = Get-CommandVersion 'cmake' @('--version')
}

$importantRegex = '(?i)OBS Studio|Adobe Lightroom|Lightroom Classic|DaVinci Resolve|Google Chrome|Microsoft Edge|Adobe Photoshop|Adobe Acrobat|VLC|Spotify|Ableton|Visual Studio|Windows SDK|CUDA|cuDNN|Python|Node\.js|Git'
$importantApps = @($installedApps | Where-Object { $_.Name -match $importantRegex })

$processes = @(Get-Process | Where-Object {
    $_.ProcessName -match '(?i)obs|lightroom|resolve|chrome|msedge|photoshop|acrobat|vlc|spotify|ableton|python|node'
} | Select-Object ProcessName, Id, Path, StartTime)

$appx = @(Get-AppxPackage | Select-Object Name, PackageFullName, Version, InstallLocation)

$hardware = [ordered]@{
    AuditUtc = (Get-Date).ToUniversalTime().ToString('o')
    Windows = [ordered]@{
        Caption = $os.Caption
        Version = $os.Version
        BuildNumber = $os.BuildNumber
        Architecture = $os.OSArchitecture
        InstallDate = $os.InstallDate
        LastBootUpTime = $os.LastBootUpTime
    }
    Computer = [ordered]@{
        Manufacturer = $computer.Manufacturer
        Model = $computer.Model
        TotalPhysicalMemoryBytes = [int64]$computer.TotalPhysicalMemory
    }
    CPU = @($cpus | Select-Object Name, Manufacturer, NumberOfCores, NumberOfLogicalProcessors, MaxClockSpeed)
    MemoryModules = @($memoryModules | Select-Object Manufacturer, PartNumber, Speed, ConfiguredClockSpeed, Capacity)
    GPU = @($gpus | Select-Object Name, AdapterCompatibility, DriverVersion, AdapterRAM, VideoProcessor, PNPDeviceID)
    NvidiaSmi = $nvidia
    AudioDevices = @($audioDevices | Select-Object Name, Manufacturer, Status, PNPDeviceID)
    AudioEndpoints = @($audioEndpoints | Select-Object FriendlyName, Status, InstanceId)
    Monitors = $monitors
    DefaultBrowserProgId = $browserChoice.ProgId
    DeveloperTools = $developerTools
}

$software = [ordered]@{
    AuditUtc = (Get-Date).ToUniversalTime().ToString('o')
    ImportantApplications = $importantApps
    InstalledApplications = $installedApps
    AppPaths = $appPaths
    StartMenu = $startMenu
    AppxPackages = $appx
    RelevantRunningProcesses = $processes
}

$hardwarePath = Join-Path $OutputDirectory 'HardwareProfile.raw.json'
$softwarePath = Join-Path $OutputDirectory 'InstalledApps.raw.json'
$hardware | ConvertTo-Json -Depth 8 | Set-Content -Path $hardwarePath -Encoding UTF8
$software | ConvertTo-Json -Depth 8 | Set-Content -Path $softwarePath -Encoding UTF8

Write-Host "Jarvis Phase 0 audit complete."
Write-Host "Hardware: $hardwarePath"
Write-Host "Software: $softwarePath"
Write-Host "These are RAW local audit files. Review/sanitize before committing anything to Git."
