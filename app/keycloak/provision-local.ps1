<#
.SYNOPSIS
  Applique sur un Keycloak DE DÉVELOPPEMENT déjà initialisé les réglages de récupération de mot de passe
  et crée/met à jour les comptes de test listés dans app/keycloak/local/test-users.json (non versionné).

.DESCRIPTION
  Le realm n'est importé qu'à la première création : un realm existant ne reçoit pas les nouveaux
  réglages de wfrp4-realm.json. Ce script est idempotent. Il n'exécute aucune migration et ne touche
  pas à la base de l'application. NE PAS le lancer contre un environnement exposé.

  SMTP réel (optionnel) : WFRP4_SMTP_HOST, WFRP4_SMTP_PORT, WFRP4_SMTP_FROM, WFRP4_SMTP_USER,
  WFRP4_SMTP_PASSWORD, WFRP4_SMTP_SSL ("true"/"false"). Sans ces variables, le SMTP du realm est laissé
  tel quel (Mailpit en dev via docker-compose).
#>
param(
    [string]$KeycloakUrl = 'http://localhost:8080',
    [string]$Realm = 'wfrp4',
    [string]$AdminUser = 'admin',
    [string]$AdminPassword = 'admin',
    [string]$UsersFile = (Join-Path $PSScriptRoot 'local/test-users.json')
)

$ErrorActionPreference = 'Stop'

$token = (Invoke-RestMethod -Method Post -Uri "$KeycloakUrl/realms/master/protocol/openid-connect/token" -Body @{
    grant_type = 'password'; client_id = 'admin-cli'; username = $AdminUser; password = $AdminPassword
}).access_token
$headers = @{ Authorization = "Bearer $token" }
$admin = "$KeycloakUrl/admin/realms/$Realm"

function Send-Json($Method, $Uri, $Body) {
    Invoke-RestMethod -Method $Method -Uri $Uri -Headers $headers -ContentType 'application/json' -Body ($Body | ConvertTo-Json -Depth 10)
}

# --- Réglages du realm ---
$realmRep = Invoke-RestMethod -Uri $admin -Headers $headers
$realmRep.resetPasswordAllowed = $true
$realmRep.loginWithEmailAllowed = $true
$realmRep.bruteForceProtected = $true
$realmRep.internationalizationEnabled = $true
$realmRep.supportedLocales = @('fr', 'en')
$realmRep.defaultLocale = 'fr'
$realmRep.actionTokenGeneratedByUserLifespan = 900
# Propriété absente de la représentation tant qu'aucun thème n'est choisi : Add-Member la crée.
$realmRep | Add-Member -NotePropertyName loginTheme -NotePropertyValue 'wfrp4' -Force

if ($env:WFRP4_SMTP_HOST) {
    $smtp = @{
        host = $env:WFRP4_SMTP_HOST
        port = if ($env:WFRP4_SMTP_PORT) { $env:WFRP4_SMTP_PORT } else { '587' }
        from = $env:WFRP4_SMTP_FROM
        fromDisplayName = 'WFRP4'
        ssl = if ($env:WFRP4_SMTP_SSL) { $env:WFRP4_SMTP_SSL } else { 'false' }
        starttls = 'true'
    }
    if ($env:WFRP4_SMTP_USER) {
        $smtp.auth = 'true'; $smtp.user = $env:WFRP4_SMTP_USER; $smtp.password = $env:WFRP4_SMTP_PASSWORD
    }
    $realmRep.smtpServer = $smtp
}
Send-Json Put $admin $realmRep
Write-Host "Realm '$Realm' mis à jour."

# --- Comptes de test ---
if (-not (Test-Path $UsersFile)) {
    Write-Host "Aucun fichier $UsersFile : comptes de test ignorés."
    return
}

foreach ($u in (Get-Content $UsersFile -Raw | ConvertFrom-Json)) {
    $existing = Invoke-RestMethod -Uri "$admin/users?username=$([uri]::EscapeDataString($u.username))&exact=true" -Headers $headers
    $rep = @{
        username = $u.username; email = $u.email; firstName = $u.firstName; lastName = $u.lastName
        enabled = $true; emailVerified = $true
    }
    if ($existing.Count -eq 0) {
        Send-Json Post "$admin/users" $rep | Out-Null
        $id = (Invoke-RestMethod -Uri "$admin/users?username=$([uri]::EscapeDataString($u.username))&exact=true" -Headers $headers)[0].id
    } else {
        $id = $existing[0].id
        Send-Json Put "$admin/users/$id" $rep | Out-Null
    }

    Send-Json Put "$admin/users/$id/reset-password" @{ type = 'password'; value = $u.password; temporary = $false } | Out-Null

    $wanted = foreach ($name in $u.roles) { Invoke-RestMethod -Uri "$admin/roles/$name" -Headers $headers }
    Send-Json Post "$admin/users/$id/role-mappings/realm" @($wanted) | Out-Null
    Write-Host "Compte '$($u.username)' prêt (rôles : $($u.roles -join ', '))."
}
