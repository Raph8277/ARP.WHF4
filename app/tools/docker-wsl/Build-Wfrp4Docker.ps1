param(
    [switch]$NoCache,
    [switch]$Up,
    [switch]$Detached = $true,
    [int]$DockerTimeoutSeconds = 90
)

$ErrorActionPreference = "Stop"

function Write-Section {
    param([string]$Title)
    Write-Host ""
    Write-Host "== $Title ==" -ForegroundColor Cyan
}

function Test-DockerReady {
    docker version --format "{{.Server.Version}}" 2>$null
    return $LASTEXITCODE -eq 0
}

$appRoot = Resolve-Path (Join-Path $PSScriptRoot "..\..")

Write-Section "Waiting for Docker"
$deadline = (Get-Date).AddSeconds($DockerTimeoutSeconds)
while ((Get-Date) -lt $deadline) {
    if (Test-DockerReady) {
        Write-Host "Docker server is ready." -ForegroundColor Green
        break
    }

    Start-Sleep -Seconds 3
}

if (-not (Test-DockerReady)) {
    throw "Docker server is not ready. Run tools\docker-wsl\Reset-DockerWsl.ps1, restart Docker Desktop, then retry."
}

Push-Location $appRoot
try {
    Write-Section "Building wfrp4-app"
    $buildArgs = @("compose", "--progress", "plain", "build", "wfrp4-app")
    if ($NoCache) {
        $buildArgs += "--no-cache"
    }

    & docker @buildArgs
    if ($LASTEXITCODE -ne 0) {
        throw "docker compose build failed with exit code $LASTEXITCODE."
    }

    if ($Up) {
        Write-Section "Starting compose stack"
        $upArgs = @("compose", "up")
        if ($Detached) {
            $upArgs += "-d"
        }

        & docker @upArgs
        if ($LASTEXITCODE -ne 0) {
            throw "docker compose up failed with exit code $LASTEXITCODE."
        }
    }
}
finally {
    Pop-Location
}
