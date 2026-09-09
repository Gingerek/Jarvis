#Requires -RunAsAdministrator
$ErrorActionPreference = 'Stop'

$targets = @(
    'HKLM:\SOFTWARE\Policies\Google\Chrome',
    'HKLM:\SOFTWARE\Policies\Microsoft\Edge'
)

foreach ($key in $targets) {
    New-Item -Path $key -Force | Out-Null
    New-ItemProperty -Path $key `
        -Name 'NativeHostsExecutablesLaunchDirectly' `
        -PropertyType DWord -Value 1 -Force | Out-Null
}

Write-Host 'Direct Native Messaging host launch enabled for Chrome and Edge.'
Write-Host 'Restart the browser once for the policy to take effect.'
