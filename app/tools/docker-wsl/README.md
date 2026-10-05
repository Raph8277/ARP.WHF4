# Docker / WSL helper scripts

Scripts for recovering Docker Desktop + WSL when Docker Compose builds fail with pipe EOF or Docker Desktop Linux engine errors.

Run from the `app` folder or with full paths.

## 1. Inspect state

```powershell
.\tools\docker-wsl\Get-DockerWslStatus.ps1
```

## 2. Soft reset

```powershell
.\tools\docker-wsl\Reset-DockerWsl.ps1
```

This stops Docker Desktop processes and tries `wsl --shutdown`.

## 3. Hard reset, only if WSL remains stuck

Use an Administrator PowerShell:

```powershell
.\tools\docker-wsl\Reset-DockerWsl.ps1 -KillVmmem -RestartServices -StartDockerDesktop
```

`-KillVmmem` forcibly stops all WSL distributions. Unsaved work inside WSL can be lost.

## 4. Rebuild the app image

After Docker Desktop is running:

```powershell
.\tools\docker-wsl\Build-Wfrp4Docker.ps1 -NoCache
```

To build and start the full stack:

```powershell
.\tools\docker-wsl\Build-Wfrp4Docker.ps1 -NoCache -Up
```
