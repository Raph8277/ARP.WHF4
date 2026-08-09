# État des lieux — ARP.WHF4

> **Date** : 25 juillet 2026  
> **Branche analysée** : `feat/one`  
> **Périmètre** : dépôt complet (`app/`, `docs/`, `.github/`, `warhammer_ai_squad/`)

## 1. Vue d'ensemble

Le projet vise à livrer une application web de gestion des personnages **Warhammer Fantasy Roleplay 4e édition** (joueurs et MJ), conforme au Livre de Base FR. Le socle technique est **Blazor WebAssembly + ASP.NET Core 9 + EF Core 9 + PostgreSQL 16**, avec authentification déléguée à **Keycloak 26** (OIDC/PKCE). Un pattern **Blazor Hosted** unifie l'API et le WASM derrière un seul conteneur.

Le dépôt contient également un espace `warhammer_ai_squad/` (slides d'un side-project IA) et une couche `.github/` riche en agents et skills de workshop (dotnet, git, sqlite, warhammer-analyst) — potentiellement héritée d'un template ; non alignée sur le projet livré.

## 2. Structure du dépôt

| Zone | Contenu | État |
|---|---|---|
| `app/` | Solution .NET (`Wfrp4.sln`) : Client, Server, Infrastructure, Shared, Seed, tests | Fonctionnel |
| `app/keycloak/` | `wfrp4-realm.json` — import automatique du realm | Présent |
| `app/docker-compose.yml` + `Dockerfile` | Stack complète (Postgres, Keycloak, app) | Fonctionnel |
| `docs/spec-app-personnages.md` | Spécification v0.4 (mars 2026), très complète | À jour |
| `README.md` (racine) | Une seule ligne (`# ARP.WHF4`) | À enrichir |
| `app/README.md` | Doc démarrage + comptes démo | À jour |
| `.github/skills/warhammer-analyst/` | Références rules/personnages/carrieres | Utile pour IA |
| `.github/skills/{dotnet,git,sqlite}-workshop/` | Skills génériques de workshop | Non utilisés par le projet livré |
| `.github/agents/*.agent.md` | ~20 agents (dotnet, sqlite, git, warhammer) | Non utilisés à l'exécution |
| `warhammer_ai_squad/` | Slides d'une présentation IA | Hors périmètre applicatif |
| `Warhammer 4 - Livre de base.md` (racine) | Doublon du fichier `.github/ressources/` | À dédupliquer |

## 3. Solution .NET — état par projet

Solution en 7 projets, alignée sur la spec (§7), sans écart structurel majeur :

- **`Wfrp4.Shared`** : DTOs (`PersonnageDto`, `CreatePersonnageRequest`, `UpdatePersonnageRequest`, `AvanceRequest`, `XPGrantRequest`, `PartageDto`, `ReferentielDtos`) et enums (`TypeXP`, `PermissionPartage`, `StatutTier`).
- **`Wfrp4.Infrastructure`** : entités EF Core (Personnage + 6 tables filles, référentiels Espece/Classe/Carriere/NiveauCarriere/Competence/Talent), `Wfrp4DbContext`, configurations, migration initiale `20260328220758_InitialCreate` (PostgreSQL/Npgsql), `Wfrp4DataSeeder` avec **données de démo** (joueur1, mj1, admin + personnage partagé).
- **`Wfrp4.Server`** : 4 controllers (`Personnages`, `Avancements`, `Partages`, `Referentiels`), 2 services (`PersonnageService`, `XPService`), 1 filter (`PersonnageOwnerFilter`), 2 helpers d'auth (`KeycloakClaimsTransformation`, `KeycloakBackchannelHandler`). Migration + seed exécutés automatiquement au démarrage avec retry (10 × 3 s) pour attendre Postgres.
- **`Wfrp4.Client`** (Blazor WASM + MudBlazor) : pages `Dashboard`, `CreationWizard`, `FichePersonnage`, `Referentiels`, `Authentication` ; composants `AvancementDialog`, `PartageDialog`, `XpGrantDialog` ; `Wfrp4ApiClient` typé + `ApiAuthorizationMessageHandler`.
- **`Wfrp4.Seed`** (tools) : outil console pour préparer la base sans lancer l'API.
- **Tests** : `Wfrp4.Server.Tests` (2 tests XPService — paliers de coût) et `Wfrp4.Infrastructure.Tests` (1 test seed idempotent, InMemory). **Couverture minimale**.

Volumétrie : **42 fichiers `.cs`** hors `bin/obj`, **12 pages/composants `.razor`**.

## 4. Infrastructure et déploiement

- `docker-compose.yml` orchestre **Postgres 16-alpine**, **Keycloak 26** (start-dev + import du realm `wfrp4`), et l'app .NET (build depuis `Dockerfile`). Ports : 5432 / 8080 / 5080.
- `Dockerfile` multi-étages (SDK 9 → aspnet 9), publish `Wfrp4.Server` en `Release`.
- Realm Keycloak versionné (`app/keycloak/wfrp4-realm.json`) avec clients OIDC et 3 rôles (`wfrp4-joueur`, `wfrp4-maitre-jeu`, `wfrp4-admin`) et 3 comptes de démo (`joueur1/mj1/admin`).
- L'API applique automatiquement les migrations puis exécute le seed → **démarrage à froid opérationnel**.

## 5. Conformité à la spécification (§ `docs/spec-app-personnages.md` v0.4)

| Chapitre spec | État d'implémentation |
|---|---|
| §2 Stack | Conforme (WASM + API + EF Core + PG16 + Keycloak 26 + MudBlazor). |
| §3 Rôles | 3 rôles + policies câblées dans `Program.cs`. |
| §4-6 C4 (contexte/conteneurs/composants) | Conforme au diagramme. |
| §7 Structure solution | Conforme, avec `Wfrp4.Seed` sous `tools/`. |
| §8 Modèle de données | Entités et migration présentes ; **pas d'entité Repository** (accès direct au DbContext via services). |
| §9 Intégration Keycloak | JWT Bearer validé via JWKS, `KeycloakClaimsTransformation` en place ; `RoleClaimType` réglé sur `ClaimTypes.Role`. |
| §10 Contrôle d'accès | `PersonnageOwnerFilter` + policies + double-check ownership dans controllers. |
| §11 Règles métier | Coûts XP conformes p. 47 ; règles avances/changement de carrière : **à vérifier dans `PersonnageService`**. |
| §12 UC-01 à UC-04 | Endpoints présents ; UC-01 à travers `CreationWizard.razor`. |

**Fonctionnalités v1 déjà couvertes** : authentification OIDC, CRUD personnage, référentiels lecture, partage MJ (lecture/XP), octroi XP par MJ, avancement caractéristiques/compétences, historique XP.

**Absent / partiel** :
- Export PDF fiche personnage → hors v1 (spec §1) ✓.
- Avancement **talent** (`TypeXP.Talent`) : présent dans l'enum et dans `XPService.CalculerCoutTalent`, mais **non branché** dans `AvancementsController.AvancerPersonnage` (switch limité à Caractéristique et Compétence).
- Édition admin des référentiels (spec §6.2 : `[Authorize(Policy=Admin)]` en écriture) : `ReferentielsController` **n'expose que du GET** — pas d'écriture admin.
- Endpoint dédié `partages-recues` : présent ✓ ; recherche du sub MJ via Keycloak Admin API (spec UC-03) : **à confirmer dans `PartagesController`**.

## 6. Qualité, tests, dette technique

- **Tests unitaires : 3** au total. `XPService` couvert sur les paliers ; seeder testé pour idempotence. Aucun test d'intégration (WebApplicationFactory), aucun test des controllers, du `PersonnageOwnerFilter`, ni du wizard.
- `bin/`, `obj/`, `.vs/` **présents dans le dépôt** (2 130 fichiers ignorés dans la liste `Glob`) → `.gitignore` à durcir.
- Fichier `Warhammer 4 - Livre de base.md` dupliqué (racine + `.github/ressources/`).
- `README.md` racine vide (une ligne).
- `.github/agents/` et 3 skills workshop (dotnet/git/sqlite) apparemment orphelins par rapport au produit.
- `warhammer_ai_squad/` : side-project (slides) non lié à l'application → à isoler ou déplacer.
- Branche courante `feat/one` : **beaucoup de suppressions non commitées** (skills warhammer-*, script `extract_pdf.py`, `copilot-instructions.md`) → nettoyage en cours à finaliser et committer.

## 7. Risques identifiés

1. **Sécurité — validation de l'audience JWT désactivée** : `ValidateAudience = false` dans `Program.cs`. À réactiver (`Keycloak:Audience = wfrp4-api`) pour bloquer les tokens émis pour un autre client du même realm.
2. **Secrets en clair** : `changeme` / `admin` dans `docker-compose.yml` et `appsettings.json`. Acceptable en dev, à sortir via secrets/variables d'env avant tout déploiement.
3. **Migrations auto au démarrage** : pratique en dev, dangereux en prod (perte de rollback contrôlé). Prévoir un mode `--migrate-only` ou pipeline CI/CD dédié.
4. **Couverture de tests très faible** : régressions probables sur `PersonnageService` (calculs dérivés, avances, changement de carrière) et sur le filtre d'autorisation.
5. **Aucun pipeline CI/CD** visible (`.github/workflows/` absent) → build, tests, image Docker à automatiser.
6. **Talent non branché** dans le controller d'avancement → bug fonctionnel côté joueur.

## 8. Prochaines étapes proposées

Court terme (débloquage / hygiène) :
1. Activer `ValidateAudience = true` et vérifier les tokens Keycloak.
2. Brancher `TypeXP.Talent` dans `AvancementsController.AvancerPersonnage` + méthode `AvancerTalent` dans `PersonnageService`.
3. Finaliser le nettoyage de la branche `feat/one` (commit des suppressions ou revert), rebase sur `main`.
4. Ajouter `.gitignore` pour `bin/`, `obj/`, `.vs/`, `*.user` ; purger l'historique du binaire.
5. Enrichir le `README.md` racine (pointeurs vers `docs/spec-app-personnages.md` et `app/README.md`).

Moyen terme (qualité) :
6. Ajouter des tests d'intégration API (WebApplicationFactory + Testcontainers Postgres) pour au moins : création personnage, avance caractéristique/compétence/talent, partage + octroi XP MJ.
7. Ajouter tests unitaires sur `PersonnageService` (calcul dérivés, règles carrière) et `PersonnageOwnerFilter`.
8. Mettre en place un workflow GitHub Actions : restore/build/test + `docker build` + push image.

Moyen terme (fonctionnel) :
9. Écriture admin des référentiels (POST/PUT/DELETE) avec `[Authorize(Policy = "Admin")]`.
10. Endpoint création/suppression de partage MJ documenté (résolution du sub via Keycloak Admin API, cf. spec UC-03).
11. Préparer l'export PDF fiche (v2) : squelette d'endpoint + choix librairie (QuestPDF/PdfPig).

Long terme (organisation) :
12. Décider du sort de `.github/skills/*-workshop/` et `warhammer_ai_squad/` (garder / archiver / dépôt séparé).
13. Passer la spec à v0.5 pour refléter le code livré et arbitrer les écarts.

## 9. Verdict synthétique

Le socle est **solide et cohérent avec la spécification v0.4** : l'architecture cible est en place, le démarrage local via Docker est opérationnel, les principaux cas d'usage (création, avancement, partage, octroi XP) sont câblés bout en bout. Le projet est **prêt pour une itération fonctionnelle et un durcissement qualité/sécurité** ; les blocages restants sont ciblés (audience JWT, talent non branché) et le principal chantier transverse est la **couverture de tests** et l'**hygiène de dépôt** (branche à recommitter, artefacts binaires à ignorer, README racine à écrire).
