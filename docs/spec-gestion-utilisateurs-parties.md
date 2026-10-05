# Spécification — Gestion des utilisateurs et parties MJ

Statut : implémentée (itération 1). Complète [spec-roles-comptes-mdp.md](spec-roles-comptes-mdp.md).
Périmètre : API ASP.NET Core, EF Core/PostgreSQL, client Blazor, Keycloak (realm `wfrp4`).

## 1. Objectifs

1. Une véritable interface d'administration des comptes : recherche, filtre par profil,
   changement de profil en un geste, activation, réinitialisation du mot de passe, fiche détaillée.
2. Tout nouveau compte est **joueur** (`wfrp4-joueur`). Seuls les administrateurs —
   le compte `admin` et **raph8277@gmail.com** — accordent le profil MJ (ou admin).
3. Un MJ crée des **parties** (aventure ou campagne), y invite des joueurs et choisit,
   pour chaque joueur, **le personnage** qui participe.

## 2. Profils et attribution

| Profil | Rôles Keycloak | Attribué par |
|---|---|---|
| Joueur | `wfrp4-joueur` | automatique (inscription, Google/Yahoo/Meta/Microsoft via mapper) |
| Maître de jeu | `wfrp4-joueur` + `wfrp4-maitre-jeu` | un administrateur |
| Administrateur | `wfrp4-joueur` + `wfrp4-maitre-jeu` + `wfrp4-admin` | un administrateur |

### 2.1 Super-administrateurs déclarés par configuration

`raph8277@gmail.com` se connecte typiquement via Google : son compte Keycloak n'existe qu'après la
première connexion et ne peut donc pas être pré-provisionné dans le realm. La liste
`Administration:SuperAdmins` (e-mails, `appsettings.json`, surchargeable par variable
d'environnement `Administration__SuperAdmins__0`) désigne les propriétaires de l'application.

Règles :
1. `KeycloakClaimsTransformation` ajoute les trois rôles applicatifs quand le jeton porte
   `email` égal (insensible à la casse) à une entrée de la liste **et** `email_verified = true`.
   Sans e-mail vérifié, aucun rôle n'est ajouté (une inscription locale non vérifiée ne suffit pas).
   Keycloak interdit les e-mails en double dans le realm (`duplicateEmailsAllowed` = false par défaut).
2. Le droit est effectif dès la prochaine requête, sans dépendre d'une écriture dans Keycloak.
3. Dans l'interface d'administration, un super-administrateur est signalé (`SuperAdmin = true`),
   affiché comme administrateur et **non modifiable** : changement de profil ou désactivation → 409.
4. Le compte `admin` reste administrateur par ses rôles Keycloak ; les garde-fous existants
   (dernier admin, auto-rétrogradation) s'appliquent toujours.

### 2.1 bis Identité affichée

L'application n'affiche et ne recherche que le **nom d'utilisateur** et l'**e-mail** (décision du
2026-10-04) : prénom et nom Keycloak ne sont ni renvoyés par l'API ni affichés (menu du profil,
administration, ajout de joueur). Le formulaire d'inscription continue de les saisir.

### 2.2 Rôles visibles par le client

L'id_token OIDC ne porte pas `realm_access` : sans aide, `AuthorizeView Roles=…` et `IsInRole`
côté Blazor ne voient aucun rôle (ni, a fortiori, ceux d'un super-administrateur). `GET /api/moi`
(authentifié) renvoie les rôles applicatifs effectifs après transformation ; une
`AccountClaimsPrincipalFactory` client les ajoute à l'utilisateur à la connexion. C'est purement
ergonomique (menus, pages) : chaque endpoint revérifie ses droits côté serveur.

## 3. Interface d'administration des comptes (`/admin/utilisateurs`)

Accès : `[Authorize(Policy = "Admin")]` côté API, `[Authorize(Roles = "wfrp4-admin")]` côté page.

| Élément | Comportement |
|---|---|
| Recherche | nom d'utilisateur, prénom, nom ou e-mail (Entrée ou bouton) |
| Filtre profil | Tous / Joueurs seuls / MJ / Admins (`?profil=`) |
| Tableau | avatar à initiales, identité, e-mail + pastille « vérifié », sélecteur de profil, interrupteur actif, menu d'actions |
| Changer de profil | sélecteur Joueur / MJ / Admin ; confirmation pour toute promotion admin ou rétrogradation |
| Actions | fiche détaillée, envoi du lien de réinitialisation du mot de passe |
| Fiche détaillée | rôles, nombre de personnages, parties menées (MJ), parties jouées |

API (en plus des routes existantes) :

| Méthode | Route | Rôle |
|---|---|---|
| GET | `/api/admin/utilisateurs?recherche=&profil=&page=&taille=` | `profil` ∈ `joueur`, `mj`, `admin` (filtre sur le profil le plus élevé) |
| GET | `/api/admin/utilisateurs/{id}/activite` | personnages, parties MJ, participations |

Limite connue : le filtre par profil lit les membres du rôle Keycloak (1 000 au plus) puis
pagine en mémoire ; suffisant pour une table de jeu, pas pour un annuaire massif.

## 4. Parties

### 4.1 Modèle

```
Partie (Id, Nom ≤120, Description ≤2000, Type Aventure|Campagne,
        Statut Preparation|EnCours|Terminee, MjKeycloakId, MjNom, CreatedAt, UpdatedAt)
PartieMembre (Id, PartieId → Partie cascade, JoueurKeycloakId, JoueurNom,
              PersonnageId? → Personnage set null, AjouteLe)
    unique (PartieId, JoueurKeycloakId)
PersonnagePartage.PartieId? → Partie set null      (origine d'un partage créé par une partie)
```

`MjNom` et `JoueurNom` sont des copies d'affichage (nom d'utilisateur Keycloak au moment de l'ajout),
jamais utilisées pour autoriser.

### 4.2 Règles métier et de sécurité

1. Créer, modifier, supprimer une partie et gérer ses membres : rôle MJ et **MJ propriétaire**
   de la partie, ou admin. Un autre MJ reçoit 403.
2. Ajouter un membre : l'identifiant est vérifié côté serveur auprès de Keycloak (compte existant,
   actif, rôle `wfrp4-joueur`) ; un identifiant inconnu → 400. Doublon → 409. 12 membres au plus.
   Le MJ ne peut pas s'ajouter lui-même comme joueur de sa propre partie.
3. Trouver un joueur : `GET /api/parties/joueurs?recherche=` ; recherche vide = **liste de tous les
   joueurs actifs** (100 au plus, triés, MJ appelant exclu), sinon filtre sur nom d'utilisateur ou e-mail.
   Renvoie identifiant, nom d'utilisateur et e-mail (affiché sous le nom pour distinguer
   les homonymes ; décision du 2026-10-04, qui remplace la règle initiale « jamais l'e-mail »).
4. Choisir le personnage d'un membre : le MJ voit la liste résumée (nom, espèce, carrière, XP) des
   personnages **actifs** de ce membre uniquement, et seulement pour un membre de sa partie.
   Le personnage choisi doit appartenir au membre (contrôle serveur, l'identifiant du DTO ne
   prouve rien) → sinon 400.
5. Accès du MJ au personnage choisi : réutilisation du mécanisme de partage existant. Le choix crée
   un `PersonnagePartage` permission **XP** (`PartieId` renseigné) si aucun partage n'existe déjà
   entre ce personnage et ce MJ. Un partage préexistant (créé par le joueur) n'est jamais modifié.
   Conformément à AGENTS.md, ce partage ne donne **aucun** droit de modifier la fiche, l'équipement
   ou les sorts ; il permet la lecture et l'octroi d'XP.
6. Retirer le personnage, retirer le membre, le joueur qui quitte la partie ou la suppression de la
   partie retirent le partage **uniquement s'il a été créé par cette partie** ; s'il sert encore à une
   autre partie du même MJ pour ce personnage, il y est rattaché au lieu d'être supprimé.
7. Le joueur voit ses participations (`GET /api/participations`) : partie, MJ, statut,
   personnage choisi, et peut **quitter** la partie (`DELETE /api/participations/{partieId}`).
   Il peut aussi révoquer le partage depuis sa fiche, comme tout partage.
8. Un personnage supprimé libère la place (`PersonnageId` à null) ; son partage disparaît en cascade.
9. Le joueur peut **proposer** (ou retirer) lui-même l'un de ses personnages actifs dans une partie dont il
   est membre (`PUT /api/participations/{partieId}/personnage`). L'effet est identique au choix du MJ
   (partage XP rattaché à la partie) ; le MJ garde la main et peut changer ce choix. Un personnage qui
   n'appartient pas à l'appelant ou est inactif → 400 ; partie dont il n'est pas membre → 404.
10. Pas d'invitation à accepter : le joueur ajouté est membre immédiatement (décision du 2026-10-04).

### 4.3 API

| Méthode | Route | Politique | Rôle |
|---|---|---|---|
| GET | `/api/parties` | MaitreJeu | parties dont je suis MJ (admin : toutes) |
| POST | `/api/parties` | MaitreJeu | créer |
| GET | `/api/parties/{id}` | MaitreJeu | détail + membres |
| PUT | `/api/parties/{id}` | MaitreJeu | nom, description, type, statut |
| DELETE | `/api/parties/{id}` | MaitreJeu | supprimer (et partages issus de la partie) |
| GET | `/api/parties/joueurs?recherche=` | MaitreJeu | recherche de joueurs |
| POST | `/api/parties/{id}/membres` | MaitreJeu | ajouter un joueur |
| DELETE | `/api/parties/{id}/membres/{membreId}` | MaitreJeu | retirer un joueur |
| GET | `/api/parties/{id}/membres/{membreId}/personnages` | MaitreJeu | personnages actifs du membre |
| PUT | `/api/parties/{id}/membres/{membreId}/personnage` | MaitreJeu | choisir (ou retirer : `null`) le personnage |
| GET | `/api/participations` | Joueur | mes participations |
| PUT | `/api/participations/{partieId}/personnage` | Joueur | proposer (ou retirer : `null`) mon personnage |
| DELETE | `/api/participations/{partieId}` | Joueur | quitter une partie |

### 4.4 Écrans

- `/mj/parties` (MJ) : cartes des parties (type, statut, nombre de joueurs), création par dialogue.
- `/mj/parties/{id}` (MJ) : édition des informations, ajout d'un joueur par autocomplétion,
  tableau des membres avec choix du personnage, lien vers la fiche, retrait, suppression de la partie.
- `/parties` (tous) : mes participations, choix de mon personnage, bouton « Quitter ».
- Menu : « Mes parties » pour tous ; « Mes tables (MJ) » visible des MJ ; « Comptes et rôles » des admins.
  Masquer un lien n'est qu'ergonomique : le serveur décide.

## 5. Critères d'acceptation

1. Un compte créé par inscription ou fournisseur social n'a que `wfrp4-joueur`.
2. Jeton avec `email = raph8277@gmail.com` et `email_verified = true` → rôles joueur, MJ et admin ;
   même e-mail non vérifié → aucun rôle ajouté.
3. Modifier le profil ou désactiver un super-administrateur → 409, aucun appel Keycloak d'écriture.
4. Joueur sur une route `/api/parties` MJ → 403 ; MJ non propriétaire sur la partie d'un autre → 403.
5. Ajouter un compte inconnu ou sans rôle joueur → 400 ; doublon → 409.
6. Choisir le personnage d'un autre joueur → 400, aucun partage créé.
7. Choisir un personnage crée un partage XP lié à la partie ; le MJ peut octroyer de l'XP mais
   reçoit 403 en modification d'équipement.
8. Retirer le membre supprime le partage créé par la partie, mais conserve un partage manuel préexistant.
9. Le joueur qui quitte la partie disparaît des membres et le partage issu de la partie est supprimé.
10. Le joueur propose son personnage : partage XP créé ; le MJ peut le remplacer ; proposer le personnage
    d'un autre ou un personnage inactif → 400 ; non-membre → 404.

## 6. Vérification et limites

| Niveau | Contenu |
|---|---|
| Unitaire | transformation des claims super-admin (vérifié / non vérifié / autre e-mail) |
| Contrôleur direct (EF InMemory, Keycloak simulé) | règles §5 : propriété, validation membre, appartenance du personnage, cycle de vie du partage, garde-fous super-admin |
| Non couvert | pipeline HTTP `[Authorize]` avec vrais jetons Keycloak, contraintes PostgreSQL (unicité, FK set null), migration sur base dédiée, parcours navigateur |

EF InMemory ne prouve ni les index uniques ni les `ON DELETE SET NULL` : la migration
`AddParties` doit être appliquée sur une base PostgreSQL **dédiée** avant toute mise en service.

## 7. Décisions (2026-10-04)

1. Partage automatique en permission **XP** : confirmé.
2. Pas d'acceptation explicite par le joueur : le MJ ajoute directement.
3. Le joueur peut proposer lui-même son personnage (règle 9) ; le MJ peut le changer.
