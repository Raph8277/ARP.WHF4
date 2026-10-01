# Spécification — Rôles Admin / MJ / Joueur, comptes de test et mot de passe

Statut : proposition, à valider avant implémentation.
Périmètre : Keycloak (realm `wfrp4`), API ASP.NET Core, client Blazor.

## 1. État actuel (constaté dans le code)

- Trois rôles realm existent : `wfrp4-joueur`, `wfrp4-maitre-jeu`, `wfrp4-admin`
  ([wfrp4-realm.json](../app/keycloak/wfrp4-realm.json)).
- `KeycloakClaimsTransformation` convertit `realm_access.roles` en `ClaimTypes.Role`.
- Politiques définies dans `Program.cs` : `Joueur`, `MaitreJeu`, `Admin`. Les rôles ne sont
  **pas hiérarchiques côté Keycloak** : un MJ doit porter explicitement `wfrp4-joueur`
  (sinon `[Authorize(Policy = "Joueur")]` le refuse).
- Comptes d'import existants : `joueur1`, `mj1`, `admin` (mot de passe = identifiant).
- Inscription : `KeycloakAdminService` crée le compte et lui attribue `wfrp4-joueur` uniquement.
- Le realm ne définit ni `smtpServer`, ni `resetPasswordAllowed`, ni politique de mot de passe :
  **aucun e-mail ne peut partir aujourd'hui** et « mot de passe oublié » n'est pas proposé.

## 2. Matrice des droits cibles

| Capacité | Joueur | MJ | Admin |
|---|---|---|---|
| Créer / voir / modifier ses propres fiches | oui | oui | oui |
| Voir/modifier la fiche d'un autre | non | non, sauf partage (voir ci-dessous) | oui |
| Partage **Lecture** | — | lecture seule | oui |
| Partage **XP** | — | octroi d'XP uniquement, aucune modification de fiche, équipement ou sorts | oui |
| Octroyer de l'XP | non | si partage XP | oui |
| Outils MJ (aventures, PNJ, PDF MJ) | non | oui | oui |
| Fonctions admin (ex. export PDF admin, `PersonnagesController` l.381) | non | non | oui |
| Gérer les rôles des comptes | non | non | oui |

Règles :
1. Les invariants de [AGENTS.md](../AGENTS.md) priment : un partage Lecture/XP ne donne jamais
   droit de modification ; l'octroi XP garde son propre contrôle.
2. L'autorisation est toujours décidée **côté serveur** à partir des claims du jeton ; l'UI
   (`AuthorizeView`, `IsInRole`) ne fait que masquer.
3. Un compte MJ porte `wfrp4-joueur` + `wfrp4-maitre-jeu` ; un admin porte les trois (modèle actuel conservé).
4. Un utilisateur sans rôle reconnu n'a accès à aucun endpoint métier (401/403, pas 500).

## 3. Nouveaux comptes de test

| Profil | E-mail | Mot de passe | Rôles realm |
|---|---|---|---|
| MJ | darkraphious@gmail.com | 8277 | `wfrp4-joueur`, `wfrp4-maitre-jeu` |
| USER (joueur) | oscarmelvine@gmail.com | 8277 | `wfrp4-joueur` |

Spécificités :
- `username` proposé : `mj-raphious` et `joueur-oscar` (à confirmer), `emailVerified: true`
  pour que la récupération par e-mail fonctionne immédiatement.
- Les identifiants fixes de l'import existant (`1111…`, `2222…`) sont réutilisés comme modèle ;
  attribuer deux nouveaux UUID fixes (`4444…`, `5555…`).
- Un mot de passe de 4 caractères n'est accepté que parce que le realm n'a **aucune
  politique de mot de passe**. Voir §5 : la politique doit être différenciée dev / prod.

### Points d'attention (décision requise)

- **Données personnelles et secrets dans Git.** Ce sont des adresses réelles et un mot de passe
  faible. Les inscrire dans `wfrp4-realm.json` (versionné) les publie dans l'historique.
  Recommandation : un fichier d'import séparé non versionné
  (`app/keycloak/local/wfrp4-test-users.json`, ajouté à `.gitignore`) ou un script de
  provisionnement lisant les valeurs depuis des variables d'environnement ; ne rien committer de réel.
- Ces comptes ne doivent exister que dans l'environnement de dev/test, jamais en production.
- Le mot de passe 8277 doit être changé dès qu'un environnement est exposé.

## 4. Gestion des rôles

### 4.1 Attribution
- Inscription publique : rôle `wfrp4-joueur` seulement (inchangé). Aucun champ de rôle accepté
  dans `InscriptionRequest`.
- Promotion MJ/Admin : par un Admin uniquement, via l'endpoint d'administration décrit en §4.3
  (décision validée : l'application expose cette administration, la console Keycloak reste un recours).
- Un admin ne peut pas retirer son propre rôle admin s'il est le dernier admin.

### 4.3 Endpoint d'administration Keycloak (admin uniquement)

Contrôleur `AdminUtilisateursController`, `[Authorize(Policy = "Admin")]` sur la classe : aucun
endpoint n'est accessible au MJ ni au joueur, et l'autorisation n'est jamais déduite d'un DTO client.
Il s'appuie sur `KeycloakAdminService` (compte de service `wfrp4-admin-service`, déjà doté de
`manage-users` et `view-realm`) ; le jeton de l'appelant n'est jamais transmis à Keycloak.

| Méthode | Route | Rôle |
|---|---|---|
| GET | `/api/admin/utilisateurs?recherche=&page=&taille=` | Liste paginée (id, username, e-mail, activé, rôles) |
| GET | `/api/admin/utilisateurs/{id}` | Détail d'un compte |
| PUT | `/api/admin/utilisateurs/{id}/roles` | Remplace l'ensemble des rôles applicatifs |
| PUT | `/api/admin/utilisateurs/{id}/activation` | Active ou désactive le compte |
| POST | `/api/admin/utilisateurs/{id}/reinitialisation-mdp` | Déclenche l'e-mail `UPDATE_PASSWORD` (`execute-actions-email`) |

Règles de sécurité et d'intégrité :
1. Liste blanche des rôles modifiables : `wfrp4-joueur`, `wfrp4-maitre-jeu`, `wfrp4-admin`.
   Tout autre nom (ex. `realm-management`, `offline_access`) est rejeté en 400.
2. Cohérence : `wfrp4-maitre-jeu` ou `wfrp4-admin` implique `wfrp4-joueur` ; le service l'ajoute
   ou rejette la demande, sans dépendre du client.
3. Garde-fous : refus (409) si l'opération laisse le realm sans aucun admin actif, et un admin ne
   peut ni se rétrograder ni se désactiver lui-même.
4. Le compte de service `service-account-*` n'est ni listé ni modifiable.
5. Aucun mot de passe ni jeton n'est accepté, renvoyé ou journalisé ; l'administrateur ne définit
   jamais le mot de passe d'autrui, il déclenche seulement l'e-mail de réinitialisation.
6. Chaque modification est journalisée (qui, cible, rôles avant/après, horodatage) dans les logs
   applicatifs ; pas de table dédiée dans cette itération.
7. Erreurs Keycloak (indisponible, 404) traduites en 503/404 sans fuite du corps de réponse brut.

Côté client : page `/admin/utilisateurs` marquée `[Authorize(Roles = "wfrp4-admin")]` (MudBlazor :
tableau, recherche, cases à cocher de rôles, bouton d'activation et de réinitialisation) ; le lien de
menu n'apparaît que pour l'admin, à titre d'ergonomie seulement.

Critères d'acceptation :
1. Joueur et MJ obtiennent 403 sur toutes les routes ; sans jeton, 401 ; admin, 200.
2. Passer `oscarmelvine@gmail.com` MJ ajoute `wfrp4-maitre-jeu` ; il garde `wfrp4-joueur`.
3. Envoyer `["wfrp4-maitre-jeu"]` seul aboutit à `wfrp4-joueur` + `wfrp4-maitre-jeu`.
4. Envoyer un rôle hors liste blanche renvoie 400 sans modification.
5. Rétrograder ou désactiver le dernier admin, ou soi-même, renvoie 409.
6. Un rôle modifié est pris en compte au prochain jeton de la personne concernée (limite : un jeton
   déjà émis reste valide jusqu'à expiration ; durée du jeton d'accès à documenter).

### 4.2 Critères d'acceptation
1. Le jeton de `darkraphious@gmail.com` contient `wfrp4-maitre-jeu` et `wfrp4-joueur`, pas `wfrp4-admin`.
2. Le jeton de `oscarmelvine@gmail.com` ne contient que `wfrp4-joueur`.
3. Joueur → endpoint `MaitreJeu` : 403. MJ → endpoint `Admin` : 403. Admin → les deux : 200.
4. MJ avec partage XP : octroi d'XP OK, modification de fiche/équipement/sorts : 403.
5. Jeton absent ou expiré : 401.

## 5. Changement et récupération de mot de passe

### 5.1 Récupération (« mot de passe oublié »)
Utiliser le flux natif Keycloak plutôt qu'un flux maison (pas de stockage de jeton côté API).

Configuration du realm :
- `resetPasswordAllowed: true` (lien « Mot de passe oublié ? » sur la page de connexion).
- `smtpServer` renseigné : hôte, port, `from`, `fromDisplayName`, `starttls`/`ssl`, `auth`,
  utilisateur et mot de passe. En dev : conteneur **Mailpit/MailHog** ajouté à
  `docker-compose.yml` (SMTP 1025, UI 8025) pour ne rien envoyer réellement. Pour envoyer
  aux vraies adresses ci-dessus : SMTP réel (ex. Gmail avec mot de passe d'application),
  identifiants fournis par variables d'environnement, jamais committés.
- Durée de validité du lien : 15 minutes (`actionTokenGeneratedByUserLifespan`/`resetCredentials` = 900 s), usage unique.
- Thème de connexion en français pour la page et l'e-mail (`internationalizationEnabled`, `defaultLocale: fr`).
- Anti-énumération : Keycloak affiche le même message que l'adresse existe ou non ; ne pas le contourner.
- Limitation de débit : activer la détection de force brute du realm (`bruteForceProtected: true`).

Parcours : page de connexion → « Mot de passe oublié ? » → saisie identifiant ou e-mail →
e-mail avec lien → page Keycloak « nouveau mot de passe » → retour à l'application.

### 5.2 Changement (utilisateur connecté)
- Ajouter dans le menu utilisateur du client (`MainLayout.razor`) un lien « Changer mon mot de
  passe » vers le compte Keycloak : action requise `UPDATE_PASSWORD` via
  `{authority}/account` ou `kc_action=UPDATE_PASSWORD` sur l'URL d'autorisation OIDC (redirection
  complète, pas de formulaire de mot de passe dans Blazor : l'application ne manipule jamais le mot de passe).
- Le client `wfrp4-blazor` autorise déjà les redirections ; vérifier que l'URL de retour est dans `redirectUris`.

### 5.3 Politique de mot de passe
- **Production** : longueur ≥ 10, interdit = nom d'utilisateur, historique des 3 derniers.
- **Dev/test** : politique vide pour autoriser `8277` ; elle doit être injectée uniquement par
  le fichier d'import local, jamais présente dans le realm versionné.
- Conséquence à décider : si la politique de prod est ajoutée au realm versionné, `8277` sera
  refusé à la réinitialisation (4 caractères). C'est volontaire ; les comptes de test devront
  alors utiliser un mot de passe conforme.

### 5.4 Critères d'acceptation
1. Depuis la page de connexion, demander la réinitialisation pour `darkraphious@gmail.com` :
   un e-mail est reçu (Mailpit en dev) avec un lien valide 15 min, à usage unique.
2. Le lien consommé ou expiré affiche une erreur Keycloak et ne change rien.
3. Une adresse inconnue produit le même message qu'une adresse connue et n'envoie rien.
4. Après changement, l'ancien mot de passe est refusé et le nouveau accepté ; les sessions ouvertes sont invalidées.
5. « Changer mon mot de passe » depuis l'application ramène sur l'application après succès.
6. Aucun mot de passe ni jeton n'apparaît dans les logs de l'API.

## 6. Plan de livraison

1. Compose : ajouter Mailpit ; Keycloak reçoit `smtpServer` par l'import local.
2. Realm : `resetPasswordAllowed`, `bruteForceProtected`, locale `fr`, durée du lien.
3. Fichier d'import local non versionné avec les deux comptes de test ; `.gitignore` mis à jour.
4. Client : lien « Changer mon mot de passe ».
5. Service et contrôleur d'administration (§4.3), DTO dans `Wfrp4.Shared`, page `/admin/utilisateurs`.
6. Tests (voir §7) et suivi dans `docs/WFRP.fiabilisation.md`.

Fichiers touchés : `app/docker-compose.yml`, `app/keycloak/wfrp4-realm.json`,
`app/keycloak/local/*` (nouveau, ignoré), `app/src/Wfrp4.Client/Layout/MainLayout.razor`,
`app/src/Wfrp4.Server/Services/KeycloakAdminService.cs`,
`app/src/Wfrp4.Server/Controllers/AdminUtilisateursController.cs` (nouveau),
`app/src/Wfrp4.Shared/DTOs/` (nouveaux DTO), `app/src/Wfrp4.Client/Pages/AdminUtilisateurs.razor` (nouveau),
`.gitignore`, `docs/WFRP.fiabilisation.md`.

## 7. Vérification et limites

| Niveau | Contenu | Prouve |
|---|---|---|
| Unitaire | `KeycloakClaimsTransformation` : rôles extraits de `realm_access` ; absence de claim | mapping des rôles |
| Contrôleur direct | joueur/MJ/admin sur les actions protégées, partage XP vs Lecture ; liste blanche, implication joueur, dernier admin pour `AdminUtilisateursController` (service Keycloak simulé) | règles métier, pas le pipeline HTTP |
| Intégration HTTP | 401/403/200 avec vrais jetons Keycloak de test, dont toutes les routes `/api/admin/utilisateurs` refusées au MJ et au joueur | politiques `[Authorize]` effectives |
| Navigateur / manuel | parcours « mot de passe oublié » avec Mailpit, changement de mot de passe | flux e-mail bout en bout |

Limites : l'envoi d'e-mail vers de vraies adresses dépend d'un SMTP externe non vérifiable en
CI ; les tests d'intégration utilisent une base dédiée et ne touchent aucune base existante ;
`node scripts/harness/verify.cjs` ne démarre pas Keycloak, donc le flux de récupération reste
validé manuellement.

## 8. Questions ouvertes

1. Les `username` proposés conviennent-ils, ou connexion par e-mail (`loginWithEmailAllowed`) uniquement ?
2. SMTP de test : Mailpit seul, ou envoi réel vers les deux adresses (quel fournisseur) ?
3. Acceptez-vous de ne pas versionner les deux comptes réels (recommandé) ?
4. ~~Endpoint d'administration des rôles~~ : tranché, il est ajouté et réservé à l'admin (§4.3).
5. Journal d'audit des changements de rôles : logs applicatifs suffisants, ou table dédiée plus tard ?
