# WFRP4 — Application de Gestion de Personnages

Application web pour la gestion de personnages de **Warhammer Fantasy Roleplay 4e édition**.

## Architecture

| Projet | Rôle |
|--------|------|
| `Wfrp4.Shared` | DTOs, enums, modèles partagés client/serveur |
| `Wfrp4.Infrastructure` | Entités EF Core, DbContext, configurations PostgreSQL |
| `Wfrp4.Server` | API ASP.NET Core (JWT Keycloak, Controllers, Services) |
| `Wfrp4.Client` | Blazor WebAssembly (MudBlazor, OIDC) |
| `Wfrp4.Seed` | Outil d'import des référentiels (espèces, carrières…) |
| `Wfrp4.Server.Tests` | Tests unitaires API |
| `Wfrp4.Infrastructure.Tests` | Tests unitaires Infrastructure |

## Prérequis

- .NET 9 SDK
- Docker + Docker Compose (pour PostgreSQL et Keycloak)

## Démarrage rapide

### 1. Infrastructure locale

```bash
docker compose up -d
```

Cela démarre :
- **PostgreSQL** sur `localhost:5432` (user: `wfrp4`, password: `changeme`, db: `wfrp4_dev`)
- **Keycloak** sur `http://localhost:8080` (admin: `admin` / `admin`)
- **Application** sur `http://localhost:5080`

Comptes de démonstration importés automatiquement dans Keycloak :
- `joueur1` / `joueur1`
- `mj1` / `mj1`
- `admin` / `admin`

Le compte `joueur1` dispose d'un personnage de démonstration automatiquement créé en base. Ce personnage est partagé avec `mj1` avec permission d'octroi d'XP.

### 2. Configuration Keycloak

Le realm `wfrp4`, les clients OIDC et les rôles sont importés automatiquement depuis [app/keycloak/wfrp4-realm.json](app/keycloak/wfrp4-realm.json).

L'admin Keycloak reste disponible sur `http://localhost:8080/admin` si vous voulez inspecter ou enrichir cette configuration.

### 3. Migration et seed

```bash
cd tools/Wfrp4.Seed
dotnet run
```

Le seed est aussi exécuté automatiquement au démarrage de l'API. Cette commande reste utile pour préparer la base sans lancer l'application.

### 4. Lancement

```bash
cd src/Wfrp4.Server
dotnet run
```

L'application est accessible sur `https://localhost:7142`.

Au premier démarrage, l'API applique automatiquement les migrations puis insère les référentiels minimaux nécessaires à la création de personnages: espèces, classes, carrières, compétences et talents.

### 5. Parcours de démo recommandé

1. Connectez-vous sur `http://localhost:5080` avec `joueur1` / `joueur1` pour voir et modifier le personnage de démonstration.
2. Déconnectez-vous puis connectez-vous avec `mj1` / `mj1` pour accéder au personnage partagé et lui octroyer de l'XP.
3. Connectez-vous avec `admin` / `admin` pour vérifier les droits complets.

## Tableau des coûts XP (Livre de Base p. 47)

| Avances | Caractéristique | Compétence |
|---------|-----------------|------------|
| 0–5     | 25 XP           | 10 XP     |
| 6–10    | 30 XP           | 15 XP     |
| 11–15   | 40 XP           | 20 XP     |
| 16–20   | 50 XP           | 30 XP     |
| 21–25   | 70 XP           | 40 XP     |
| 26–30   | 90 XP           | 60 XP     |
| 31–35   | 120 XP          | 80 XP     |
| 36–40   | 150 XP          | 110 XP    |
| 41–45   | 190 XP          | 140 XP    |
| 46–50   | 230 XP          | 180 XP    |

## Sécurité

- Authentification OIDC via Keycloak (Authorization Code + PKCE)
- JWT Bearer validation côté API
- Contrôle d'accès par ownership (`PersonnageOwnerFilter`)
- Rôles : Joueur, Maître de Jeu, Admin
- Partage granulaire (Lecture / XP) avec expiration optionnelle