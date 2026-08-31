<#
.SYNOPSIS
    Synchronise un worktree vers le repo local après validation des tests.

.DESCRIPTION
    1. Détecte le worktree courant (ou celui passé en paramètre)
    2. Lance dotnet build + dotnet test dans le worktree
    3. Si les tests passent, copie les fichiers modifiés/ajoutés vers le repo principal
    4. Supprime du repo principal les fichiers supprimés dans le worktree
    5. Affiche un résumé — le commit reste à la charge de l'utilisateur

.PARAMETER WorktreePath
    Chemin du worktree à synchroniser. Par défaut : le répertoire courant.

.PARAMETER SkipTests
    Passer les tests (sync direct). À utiliser pour les worktrees sans code .NET (docs, config).

.PARAMETER DryRun
    Affiche ce qui serait fait sans rien modifier.

.EXAMPLE
    .\scripts\sync-worktree.ps1
    .\scripts\sync-worktree.ps1 -WorktreePath "G:\repos\ARP.WHF4\.claude\worktrees\mon-worktree"
    .\scripts\sync-worktree.ps1 -SkipTests
    .\scripts\sync-worktree.ps1 -DryRun
#>

param(
    [string]$WorktreePath = "",
    [switch]$SkipTests,
    [switch]$DryRun
)

$ErrorActionPreference = "Stop"

# --- Résoudre les chemins ---

if ($WorktreePath -eq "") {
    $WorktreePath = Get-Location
}
$WorktreePath = (Resolve-Path $WorktreePath).Path

$mainRepo = git -C $WorktreePath rev-parse --path-format=absolute --git-common-dir 2>$null
if (-not $mainRepo) {
    Write-Host "ERREUR: $WorktreePath n'est pas dans un dépôt git." -ForegroundColor Red
    exit 1
}
# --git-common-dir renvoie le .git du repo principal
$mainRepoRoot = Split-Path $mainRepo -Parent

# Vérifier que c'est bien un worktree (pas le repo principal lui-même)
$wtRoot = git -C $WorktreePath rev-parse --show-toplevel 2>$null
if ($wtRoot -replace '/', '\' -eq $mainRepoRoot -replace '/', '\') {
    Write-Host "ERREUR: Vous êtes dans le repo principal, pas dans un worktree." -ForegroundColor Red
    exit 1
}

$branch = git -C $WorktreePath branch --show-current
Write-Host ""
Write-Host "=== Sync Worktree ===" -ForegroundColor Cyan
Write-Host "  Worktree : $WorktreePath" -ForegroundColor Gray
Write-Host "  Branche  : $branch" -ForegroundColor Gray
Write-Host "  Repo     : $mainRepoRoot" -ForegroundColor Gray
Write-Host ""

# --- Étape 1 : Tests ---

if (-not $SkipTests) {
    $slnFile = Get-ChildItem -Path $WorktreePath -Recurse -Filter "*.sln" -Depth 3 | Select-Object -First 1
    if ($slnFile) {
        Write-Host "[1/3] Build + Tests..." -ForegroundColor Yellow

        $buildResult = & dotnet build $slnFile.FullName --nologo -v q 2>&1
        if ($LASTEXITCODE -ne 0) {
            Write-Host "ECHEC du build:" -ForegroundColor Red
            $buildResult | ForEach-Object { Write-Host "  $_" }
            exit 1
        }
        Write-Host "  Build OK" -ForegroundColor Green

        $testResult = & dotnet test $slnFile.FullName --nologo --no-build -v q 2>&1
        if ($LASTEXITCODE -ne 0) {
            Write-Host "ECHEC des tests:" -ForegroundColor Red
            $testResult | ForEach-Object { Write-Host "  $_" }
            exit 1
        }
        Write-Host "  Tests OK" -ForegroundColor Green
    }
    else {
        Write-Host "[1/3] Aucune solution .sln trouvée, skip tests." -ForegroundColor DarkYellow
    }
}
else {
    Write-Host "[1/3] Tests ignorés (-SkipTests)." -ForegroundColor DarkYellow
}

# --- Étape 2 : Détecter les changements ---

Write-Host "[2/3] Détection des changements..." -ForegroundColor Yellow

# Fichiers modifiés et ajoutés (par rapport à la branche principale du repo)
$mainBranch = git -C $mainRepoRoot branch --show-current
$diffFiles = git -C $WorktreePath diff --name-only --diff-filter=ACMR "$mainBranch" HEAD 2>$null
$deletedFiles = git -C $WorktreePath diff --name-only --diff-filter=D "$mainBranch" HEAD 2>$null

# Aussi inclure les fichiers non-suivis et modifiés non-commités
$untrackedFiles = git -C $WorktreePath ls-files --others --exclude-standard 2>$null
$dirtyFiles = git -C $WorktreePath diff --name-only --diff-filter=ACMR 2>$null

# Combiner tout
$allModified = @()
if ($diffFiles) { $allModified += $diffFiles }
if ($untrackedFiles) { $allModified += $untrackedFiles }
if ($dirtyFiles) { $allModified += $dirtyFiles }
$allModified = $allModified | Sort-Object -Unique

# Exclure les fichiers dans .claude/worktrees/ (meta-données d'autres worktrees)
$allModified = $allModified | Where-Object { $_ -notmatch '^\.' -eq $false -and $_ -notmatch '\.claude/worktrees/' }
# Garder tout sauf les chemins dans .claude/worktrees/
$allModified = $allModified | Where-Object { $_ -notmatch '\.claude/worktrees/' }

$allDeleted = @()
if ($deletedFiles) { $allDeleted += $deletedFiles }
$allDeleted = $allDeleted | Where-Object { $_ -notmatch '\.claude/worktrees/' }

if ($allModified.Count -eq 0 -and $allDeleted.Count -eq 0) {
    Write-Host "  Aucun changement détecté." -ForegroundColor DarkYellow
    exit 0
}

Write-Host "  $($allModified.Count) fichier(s) modifié(s)/ajouté(s)" -ForegroundColor Gray
Write-Host "  $($allDeleted.Count) fichier(s) supprimé(s)" -ForegroundColor Gray

# --- Étape 3 : Synchroniser ---

Write-Host "[3/3] Synchronisation vers $mainRepoRoot..." -ForegroundColor Yellow

$copied = 0
$removed = 0
$errors = 0

foreach ($file in $allModified) {
    $src = Join-Path $WorktreePath $file
    $dst = Join-Path $mainRepoRoot $file

    if (-not (Test-Path $src)) { continue }

    $dstDir = Split-Path $dst -Parent
    if ($DryRun) {
        Write-Host "  [COPIE] $file" -ForegroundColor DarkCyan
    }
    else {
        try {
            if (-not (Test-Path $dstDir)) {
                New-Item -ItemType Directory -Path $dstDir -Force | Out-Null
            }
            Copy-Item -Path $src -Destination $dst -Force
            $copied++
        }
        catch {
            Write-Host "  ERREUR copie $file : $_" -ForegroundColor Red
            $errors++
        }
    }
}

foreach ($file in $allDeleted) {
    $dst = Join-Path $mainRepoRoot $file

    if (-not (Test-Path $dst)) { continue }

    if ($DryRun) {
        Write-Host "  [SUPPR] $file" -ForegroundColor DarkMagenta
    }
    else {
        try {
            Remove-Item -Path $dst -Force
            $removed++
        }
        catch {
            Write-Host "  ERREUR suppression $file : $_" -ForegroundColor Red
            $errors++
        }
    }
}

# --- Résumé ---

Write-Host ""
if ($DryRun) {
    Write-Host "=== DRY RUN — aucune modification effectuée ===" -ForegroundColor DarkYellow
}
else {
    Write-Host "=== Résumé ===" -ForegroundColor Cyan
    Write-Host "  Copiés   : $copied" -ForegroundColor Green
    Write-Host "  Supprimés: $removed" -ForegroundColor Yellow
    if ($errors -gt 0) {
        Write-Host "  Erreurs  : $errors" -ForegroundColor Red
    }
    Write-Host ""
    Write-Host "  Prêt pour commit dans $mainRepoRoot" -ForegroundColor Gray
    Write-Host "  > cd $mainRepoRoot" -ForegroundColor White
    Write-Host "  > git add -A && git status" -ForegroundColor White
}
