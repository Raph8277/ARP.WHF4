param(
    [switch]$KillVmmem,
    [switch]$RestartServices,
    [switch]$StartDockerDesktop,
    [int]$ShutdownTimeoutSeconds = 20
)

$ErrorActionPreference = "Continue"

function Write-Section {
    param([string]$Title)
    Write-Host ""
    Write-Host "== $Title ==" -ForegroundColor Cyan
}

function Test-IsAdmin {
    $identity = [Security.Principal.WindowsIdentity]::GetCurrent()
    $principal = [Security.Principal.WindowsPrincipal]::new($identity)
    return $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
}

function Stop-ProcessByName {
    param([string[]]$Names)

    foreach ($name in $Names) {
        Get-Process -Name $name -ErrorAction SilentlyContinue |
            Stop-Process -Force -ErrorAction SilentlyContinue
    }
}

Write-Section "Stopping Docker Desktop processes"
Stop-ProcessByName -Names @(
    "Docker Desktop",
    "com.docker.backend",
    "com.docker.build",
    "docker",
    "docker-sandbox"
)

Write-Section "Trying wsl --shutdown"
$wsl = Start-Process -FilePath "wsl.exe" -ArgumentList "--shutdown" -NoNewWindow -PassThru
if (-not $wsl.WaitForExit($ShutdownTimeoutSeconds * 1000)) {
    Write-Warning "wsl --shutdown did not finish after $ShutdownTimeoutSeconds seconds."
    Stop-Process -Id $wsl.Id -Force -ErrorAction SilentlyContinue

    if ($KillVmmem) {
        Write-Section "Killing vmmemWSL"
        Write-Warning "This forcibly stops all WSL distributions. Unsaved work inside WSL can be lost."
        Stop-Process -Name "vmmemWSL" -Force -ErrorAction SilentlyContinue
    }
    else {
        Write-Warning "Run this script again with -KillVmmem if WSL remains stuck."
    }
}
else {
    Write-Host "wsl --shutdown completed." -ForegroundColor Green
}

if ($RestartServices) {
    Write-Section "Restarting Windows services"
    if (-not (Test-IsAdmin)) {
        Write-Warning "RestartServices requires an elevated PowerShell window."
    }
    else {
        $services = @("WSLService", "vmcompute", "hns")

        foreach ($service in $services) {
            if (Get-Service -Name $service -ErrorAction SilentlyContinue) {
                Stop-Service -Name $service -Force -ErrorAction SilentlyContinue
            }
        }

        foreach ($service in @("hns", "vmcompute", "WSLService")) {
            if (Get-Service -Name $service -ErrorAction SilentlyContinue) {
                Start-Service -Name $service -ErrorAction SilentlyContinue
            }
        }
    }
}

if ($StartDockerDesktop) {
    Write-Section "Starting Docker Desktop"
    $dockerDesktop = Join-Path $Env:ProgramFiles "Docker\Docker\Docker Desktop.exe"
    if (Test-Path $dockerDesktop) {
        Start-Process -FilePath $dockerDesktop
    }
    else {
        Write-Warning "Docker Desktop executable not found at: $dockerDesktop"
    }
}

Write-Section "Remaining WSL/Docker processes"
Get-Process |
    Where-Object { $_.ProcessName -match 'Docker|wsl|vmmem|com\.docker' } |
    Sort-Object ProcessName, Id |
    Select-Object ProcessName, Id, CPU, StartTime |
    Format-Table -AutoSize

Write-Host ""
Write-Host "Next checks:" -ForegroundColor Yellow
Write-Host "  wsl --list --verbose"
Write-Host "  docker version"
