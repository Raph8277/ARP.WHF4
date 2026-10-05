param(
    [switch]$Detailed
)

$ErrorActionPreference = "Continue"

function Write-Section {
    param([string]$Title)
    Write-Host ""
    Write-Host "== $Title ==" -ForegroundColor Cyan
}

Write-Section "WSL processes"
Get-Process |
    Where-Object { $_.ProcessName -match 'Docker|wsl|vmmem|com\.docker' } |
    Sort-Object ProcessName, Id |
    Select-Object ProcessName, Id, CPU, StartTime |
    Format-Table -AutoSize

Write-Section "WSL services"
Get-Service |
    Where-Object {
        $_.Name -match 'wsl|lxss|vmcompute|hns' -or
        $_.DisplayName -match 'Windows Subsystem|WSL|Linux|Virtual Machine|Host Network'
    } |
    Sort-Object Name |
    Select-Object Name, DisplayName, Status, StartType |
    Format-Table -AutoSize

Write-Section "Docker context"
docker context ls

Write-Section "Docker version"
docker version

Write-Section "WSL distributions"
wsl --list --verbose

if ($Detailed) {
    Write-Section "Docker compose config"
    Push-Location (Join-Path $PSScriptRoot "..\..")
    try {
        docker compose config
    }
    finally {
        Pop-Location
    }
}
