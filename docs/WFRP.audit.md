# WFRP — Audit de l’application

> **19 septembre 2026 · ARP.WHF4 · Révision `2acede2`**  
> Audit du produit, du code, des données, de la sécurité et de la préparation à l’exploitation.

**Après l'audit :** les corrections et leurs vérifications sont suivies dans
[WFRP.fiabilisation.md](WFRP.fiabilisation.md). Le présent rapport conserve les
constats de la révision auditée.

## 1. Verdict

**Un produit riche, qui compile et dont les tests passent, mais dont les garanties d’intégrité et d’autorisation doivent être renforcées avant une ouverture publique.**

Le socle Blazor WebAssembly / ASP.NET Core / PostgreSQL / Keycloak est cohérent. Les parcours dépassent largement la gestion d’une fiche : création guidée, avancements, partages MJ, grimoire, aventures sauvegardées, outils de préparation et exports PDF sont présents dans le code. Les **19 tests existants réussissent** ; la compilation Release de la solution réussit également.

Cette réussite ne couvre pas les principaux risques identifiés : un partage en lecture peut permettre la suppression d’une possession ; des valeurs de création sont acceptées sans validation suffisante ; des écritures simultanées peuvent désynchroniser les compteurs XP ; un brouillon annulé peut être sauvegardé par une action sur la bourse. La génération PDF accepte aussi un volume de travail non borné.

**Décision recommandée :** conserver un usage de développement ou de démonstration contrôlée, corriger les constats P1, puis valider une préproduction avec les services réels. Le dépôt seul ne permet pas d’attester la disponibilité, la sécurité ou la conformité fonctionnelle d’une instance déployée.

| Indicateur vérifié | Résultat | Interprétation |
| --- | --- | --- |
| Compilation Release | Réussite, code de sortie 0 | Les 7 projets de la solution compilent dans l’environnement disponible |
| Tests automatisés | **19 réussis / 19 exécutés** | 18 côté serveur et 1 côté infrastructure ; aucun ignoré |
| Avertissements lors de la compilation des tests | **64** | 62 `MUD0002`, 2 `CS0618` ; le build incrémental suivant n’en réémet pas |
| Audit NuGet direct et transitif | Aucun package vulnérable signalé | Selon la source NuGet consultée ; ne couvre ni images Docker ni logique applicative |
| Parcours PostgreSQL + Keycloak | Non exécutés | Aucun résultat d’intégration ou de bout en bout revendiqué |
| Préparation à une ouverture publique | Insuffisante en l’état du dépôt | Autorisations, intégrité, bornes de ressources et configuration à traiter |

Les priorités ci-dessous sont qualitatives : **P1** = correction avant élargissement de l’usage ou avant la condition d’exposition indiquée ; **P2** = correction dans les prochaines itérations. Aucun score de sécurité, pourcentage de couverture ou temps de réponse non mesuré n’est présenté.

## 2. Périmètre et méthode

L’analyse porte sur le commit **`2acede2c1665f4e080fbe02cd3e5c2e0660978b2`**, daté du 19 septembre 2026, et les fichiers de travail de l’application. Le fichier local `.claude/settings.local.json`, déjà non suivi avant l’audit, n’a pas été modifié. Aucune correction du code produit ni mutation d’une base ou d’un compte n’a été effectuée.

Ont été examinés : les projets `app/src`, les deux projets de tests, l’outil Seed, les configurations EF et les migrations, le Dockerfile, le Compose, l’import Keycloak, les documentations de démarrage et la spécification locale. Les supports de présentation IA, les livres et illustrations, ainsi que l’outil WPF `mdTools`, sont hors analyse fonctionnelle détaillée. `mdTools` et `MudColorTest` ne font pas partie de la solution testée.

La méthode combine lecture des chemins d’exécution, confrontation client/serveur, examen des contrôles et des modèles, compilation, exécution des tests et interrogation des avis NuGet. La politique de support .NET a été vérifiée sur le site officiel Microsoft. Les références de code renvoient aux lignes de la révision auditée.

**Niveau de preuve :** les résultats de compilation et de tests sont exécutés. Les défauts et scénarios d’impact sont principalement établis par lecture du code ; ils ne constituent pas des reproductions HTTP ou navigateur. Les risques de concurrence, de rotation des clés et de montée en charge nécessitent les validations ciblées proposées. Aucune requête de surcharge ou suppression réelle n’a été lancée.

L’accès initial à la configuration NuGet était refusé dans le contexte restreint ; la vérification a ensuite abouti avec les droits nécessaires. La commande de lecture Docker n’a pas pu accéder au moteur dans ce contexte ; aucun conteneur n’a été démarré. Une restauration NuGet à partir d’un cache vide et une construction de l’image Docker ne sont donc pas validées.

## 3. Architecture et couverture produit

```text
Navigateur — Blazor WebAssembly / MudBlazor
   │  Authentification OIDC, code d’autorisation + PKCE
   ├──────────────────────────────────────► Keycloak
   │  API HTTP + JWT
   ▼
ASP.NET Core — contrôleurs, autorisations, services métier, PDF
   │
   ▼
EF Core / Npgsql ─────────────────────────► PostgreSQL
   ▲
   └── Migrations et seed exécutés au démarrage de l’API

Wfrp4.Shared : DTO et modèles communs au client et au serveur
```

| Brique | Responsabilité observée | Appréciation |
| --- | --- | --- |
| `Wfrp4.Client` | Pages, formulaires, référentiels, thèmes, préparation MJ | Fonctionnellement dense ; plusieurs composants cumulent trop de responsabilités |
| `Wfrp4.Server` | 7 contrôleurs, règles métier, accès, inscription, PDF | Découpage lisible, mais règles et droits appliqués de façon inégale selon les routes |
| `Wfrp4.Infrastructure` | Entités, configurations EF, seed et 22 migrations | Contraintes relationnelles utiles ; dérive du snapshot et données métier à corriger |
| `Wfrp4.Shared` | DTO, enums, modèles de mise en page | Contrats communs pratiques ; plusieurs champs sensibles manquent de bornes |
| `Wfrp4.Seed` | Initialisation séparée des données | Utile pour préparer l’environnement sans servir l’API |
| Deux projets de tests | Coûts XP, création et seed | Base utile, encore étroite par rapport au produit livré |

Inventaire sous `app/src`, hors `bin` et `obj` : **110 fichiers C#**, y compris migrations et fichiers générés de migration, et **31 fichiers Razor**. Ces nombres décrivent la taille du périmètre, pas sa complexité réelle ni sa qualité.

| Parcours | Présence dans le code | Éléments à fiabiliser |
| --- | --- | --- |
| Connexion et inscription | OIDC, PKCE S256, API d’inscription Keycloak | Audience, rotation des clés, échecs partiels et débit d’inscription |
| Création et fiche personnage | Wizard, identité, caractéristiques, équipement, bourse | Invariants serveur, réserves initiales, annulation et isolation des brouillons |
| Progression et XP | Caractéristiques, compétences, talents, carrière, retours, historique | Solde, provenance des remboursements, concurrence, attributs dérivés |
| Partage joueur / MJ | Lecture, octroi XP, expiration | Protection de la suppression d’équipement ; matrice de droits à tester |
| Référentiels et grimoire | Espèces, carrières, sorts, créatures, titres | Complétude des niveaux de carrière et validations exhaustives |
| Atelier MJ et aventures | Générateurs, sauvegarde par utilisateur, exports | Événements MudBlazor, limites PDF, migrations et reprise sur erreur |
| Personnalisation et PDF | Thèmes, designers, fiche et export MJ | Permissions visibles, ergonomie, limites de taille et tests de rendu |

**Points solides à préserver.** Le serveur contrôle la propriété et l’expiration des partages, vérifie explicitement la permission d’octroi XP et cloisonne les aventures par utilisateur. Les JWT conservent la validation de l’émetteur et de la durée de vie. Les clés composites et index uniques limitent plusieurs catégories de doublons. Les chargements de graphes complexes utilisent déjà `AsSplitQuery`, et de nombreuses lectures utilisent `AsNoTracking`. Les mutations XP et leurs écritures d’historique sont généralement regroupées dans un même `SaveChangesAsync` ; cela est utile, même si cela ne résout pas les mises à jour concurrentes.

## 4. Vérifications exécutées

Environnement : Windows x64, SDK **.NET 10.0.401**, cible applicative **net9.0**, runtime .NET / ASP.NET Core **9.0.20** présent. Aucun `global.json` ne fixe la version de SDK du dépôt.

| Commande ou contrôle | Résultat constaté | Portée et limite |
| --- | --- | --- |
| `dotnet test app/Wfrp4.sln --no-restore --configuration Release` | 18 tests Server + 1 Infrastructure réussis | Dépendances déjà restaurées ; EF InMemory pour les données |
| `dotnet build app/Wfrp4.sln --no-restore --configuration Release` | 7 projets compilés ; 0 erreur | Build incrémental, sans reconstruction d’une image de déploiement |
| `dotnet list app/Wfrp4.sln package --vulnerable --include-transitive --no-restore --config tmp/WFRP.audit.nuget.config` | Aucun package vulnérable signalé pour les 7 projets | Configuration temporaire utilisant `https://api.nuget.org/v3/index.json` |
| Recherche des workflows versionnés | Aucun fichier dans `.github/workflows` | Une CI externe peut exister ; elle n’est pas visible dans ce dépôt |
| Comparaison modèle EF / snapshot | L’entité aventure manque au snapshot | Contrôle statique ; migrations non appliquées sur PostgreSQL pendant l’audit |

Les tests sont répartis entre **13 cas de `PersonnageServiceCreationTests`**, **5 cas de coûts XP** et **1 cas de seed**. Ils couvrent notamment les choix gratuits à la création, certains remboursements et un retour de carrière. Ils ne couvrent pas la matrice d’accès HTTP, les transactions PostgreSQL concurrentes, la rotation Keycloak, la reprise d’inscription, les parcours navigateur ou les limites de génération PDF.

Le succès du build incrémental ne fait pas disparaître les avertissements de la première compilation : **62 attributs MudBlazor non reconnus** et **2 usages de `StatutSocial` obsolète** ont été émis pendant les tests. Certains avertissements concernent des événements de chargement ; ils ont donc un impact potentiel sur le comportement, au-delà de la présentation.

## 5. Sécurité et contrôle d’accès

### S01 · P1 — Un partage en lecture permet une suppression d’équipement

**Constat.** Le filtre autorise un MJ disposant d’un partage actif, quelle que soit sa permission. La route de suppression d’une possession applique ce filtre mais n’ajoute pas le contrôle propriétaire/admin utilisé sur les suppressions de sorts et de parchemins.

**Impact et scénario.** Un compte possédant les rôles joueur et MJ, avec un partage Lecture ou XP actif, peut demander la suppression d’une possession du personnage partagé. L’identifiant est accessible dans la fiche. Le contrôle d’appartenance de la possession au personnage est présent ; c’est le droit d’écriture sur ce personnage qui manque.

**Correction attendue.** Exiger propriétaire/admin sur cette route, puis centraliser les politiques lecture, modification et octroi XP. Le test d’acceptation doit vérifier un refus pour un MJ lecteur et un MJ XP, tout en conservant leur accès en lecture.

Preuves : [PersonnageOwnerFilter.cs, L62–74](../app/src/Wfrp4.Server/Filters/PersonnageOwnerFilter.cs#L62), [PersonnagesController.cs, L508–518](../app/src/Wfrp4.Server/Controllers/PersonnagesController.cs#L508).

### S02 · P1 — La génération PDF accepte un travail non borné

**Constat.** Le DTO expose `NotesLineCount` sans borne ; la génération itère jusqu’à cette valeur et accumule les tracés du document en mémoire. La route `POST /api/mj/pdf` est accessible à tout utilisateur authentifié. Le jeton d’annulation n’est pas vérifié dans cette boucle.

**Impact.** Une petite requête peut provoquer un coût CPU et mémoire disproportionné. L’indisponibilité effective n’a pas été mesurée et aucun payload de surcharge n’a été exécuté.

**Correction attendue.** Refuser les valeurs de mise en page hors plage, plafonner sections, lignes, pages et taille totale, contrôler l’annulation et limiter la concurrence de génération. Les valeurs extrêmes doivent recevoir une erreur rapide avant construction du document.

Preuves : [MjPdfExportDto.cs, L35–38](../app/src/Wfrp4.Shared/DTOs/MjPdfExportDto.cs#L35), [MjPdfExportService.cs, L211–219](../app/src/Wfrp4.Server/Services/MjPdfExportService.cs#L211), [MjController.cs, L10–19](../app/src/Wfrp4.Server/Controllers/MjController.cs#L10).

### S03 · P2 — L’audience des JWT n’est pas vérifiée

**Constat.** Une audience est configurée, mais `ValidateAudience = false`. La signature, l’émetteur et l’expiration restent contrôlés.

**Impact.** Un jeton valide du même realm, destiné à un autre client et portant les rôles requis, peut être accepté. Le risque dépend notamment de la présence d’autres applications dans ce realm ; ce n’est pas une acceptation de jetons arbitraires.

**Correction attendue.** Activer la validation, configurer l’audience côté Keycloak et tester un jeton du bon émetteur avec une audience incorrecte.

Preuve : [Program.cs, L25–26 et L58–64](../app/src/Wfrp4.Server/Program.cs#L58).

### S04 · P2 — Les clés de signature sont chargées une seule fois

**Constat.** Le démarrage télécharge les JWKS puis affecte une configuration OIDC statique à `options.Configuration`.

**Risque.** Une rotation des clés Keycloak peut entraîner le refus des nouveaux jetons jusqu’au redémarrage de l’API. Ce scénario n’a pas été exécuté.

**Correction attendue.** Utiliser le rafraîchissement des métadonnées et clés OIDC avec le backchannel requis, puis faire un test de rotation sans redémarrer le serveur.

Preuve : [Program.cs, L35–37 et L178–190](../app/src/Wfrp4.Server/Program.cs#L35).

### S05 · P2 — L’inscription manque de reprise et de protection contre l’abus

**Constat.** L’inscription publique enchaîne création du compte, mot de passe et attribution du rôle sans compensation. Un échec après création laisse un compte partiel ; une nouvelle tentative rencontre un conflit. Aucune limitation applicative du débit n’est configurée dans `Program.cs`.

**Impact.** L’utilisateur peut rester bloqué après une erreur partielle. Si l’endpoint est public sans protection équivalente en amont, l’automatisation des inscriptions peut aussi charger Keycloak et créer des comptes indésirables. La présence d’une protection externe n’est pas vérifiable ici.

**Correction attendue.** Rendre la séquence récupérable ou compensable, transmettre les credentials lors de la création lorsque possible, et prévoir une limitation du débit et une politique d’inscription explicite. Tester l’échec de chaque étape et la reprise.

Preuves : [KeycloakAdminService.cs, L48–50, L90–93 et L125–142](../app/src/Wfrp4.Server/Services/KeycloakAdminService.cs#L48), [InscriptionController.cs, L19–28](../app/src/Wfrp4.Server/Controllers/InscriptionController.cs#L19).

## 6. Règles métier et intégrité des données

### D01 · P1 — La création fait trop confiance aux valeurs client

**Constat.** `XpBonus` positif devient directement le capital XP, alors que son contrat décrit des valeurs de 0, 25 ou 50. Le service ne valide pas complètement les dix caractéristiques, leurs valeurs, le niveau de carrière initial ni la compatibilité espèce/carrière. Les contrôles existants sur les choix de compétences, talents, titres et sorts ne couvrent pas ces invariants.

**Scénario.** Une requête propriétaire avec `XpBonus = 1000000` est prise en compte. Des caractéristiques absentes ou aberrantes et un niveau de carrière supérieur peuvent également franchir les validations identifiées. Ce constat concerne l’intégrité du personnage créé, pas un accès à celui d’un tiers.

**Correction attendue.** Valider côté serveur les valeurs autorisées, l’ensemble exact des caractéristiques, le niveau initial et les combinaisons de référentiels. Les bonus liés au tirage doivent être calculés ou vérifiés par une règle serveur. Compléter également l’initialisation de Destin, Fortune, Résilience et Résolution : ces entiers restent à zéro dans la création courante, tandis que le seed les renseigne pour le personnage de démonstration.

Preuves : [CreatePersonnageRequest.cs, L13–20](../app/src/Wfrp4.Shared/DTOs/CreatePersonnageRequest.cs#L13), [PersonnageService.cs, L23–35, L80–89 et L104–107](../app/src/Wfrp4.Server/Services/PersonnageService.cs#L23), [Personnage.cs, L24–27](../app/src/Wfrp4.Infrastructure/Entities/Personnage.cs#L24).

### D02 · P1 — Les mises à jour concurrentes peuvent perdre des XP

**Constat.** Les opérations lisent un personnage, modifient des compteurs puis sauvegardent. Aucun jeton de concurrence n’est défini sur le personnage ou les lignes d’avancement. La transaction implicite d’une sauvegarde ne protège pas contre une lecture préalable devenue obsolète.

**Scénario à reproduire sur PostgreSQL.** Deux requêtes lisent `XpDepense = 0`, puis augmentent respectivement F et E pour 25 XP. Les deux caractéristiques et deux écritures de −25 peuvent être conservées, tandis que la dernière écriture du compteur laisse `XpDepense = 25`. Deux octrois XP simultanés peuvent perdre un gain de façon analogue.

**Correction attendue.** Ajouter une stratégie de concurrence explicite : version optimiste avec relecture et gestion du conflit, ou opérations atomiques et isolation adaptées. Vérifier que chaque opération appliquée se retrouve exactement une fois dans les compteurs et l’historique.

Preuves : [PersonnageService.cs, L208–237 et L578–597](../app/src/Wfrp4.Server/Services/PersonnageService.cs#L208), [PersonnageConfiguration.cs](../app/src/Wfrp4.Infrastructure/Data/Configurations/PersonnageConfiguration.cs), [Personnage.cs](../app/src/Wfrp4.Infrastructure/Entities/Personnage.cs).

### D03 · P2 — La politique de dépense XP varie selon le parcours

**Constat.** Les avances de caractéristique, compétence et talent ne contrôlent pas le solde disponible ; le changement de carrière le contrôle. La fiche principale accepte les ajustements non nuls et affiche un déficit, alors que certains dialogues exigent un solde suffisant. Les restrictions de carrière annoncées pour caractéristiques et compétences ne sont pas uniformément appliquées par ces services.

**Écart de spécification.** La spécification locale prévoit validation du solde et vérification du plan de carrière. Le code semble également proposer une logique d’ajustement libre. Il faut arbitrer cette évolution plutôt que considérer automatiquement tout déficit comme une erreur d’interface.

**Correction attendue.** Définir le comportement attendu, puis l’appliquer au serveur et à toutes les interfaces. Si un mode de correction libre est conservé, lui donner des droits explicites et une trace distincte. En progression réglementée, une dépense supérieure au solde doit être refusée sans mutation.

Preuves : [PersonnageService.cs, L221–224, L279–287, L353–377 et L478–479](../app/src/Wfrp4.Server/Services/PersonnageService.cs#L221), [FichePersonnage.razor, L1664–1682](../app/src/Wfrp4.Client/Pages/FichePersonnage.razor#L1664), [spécification, L281 et L707–708](spec-app-personnages.md#L281).

### D04 · P2 — Les remboursements ne retrouvent pas toujours la dépense d’origine

**Constat.** L’annulation de carrière sélectionne la dernière dépense négative de type carrière, sans la relier à la transition annulée ni exclure une dépense déjà remboursée. Les retraits d’avances ou de talents utilisent quant à eux un plafond global de XP dépensés, sans distinguer correctement toutes les acquisitions gratuites et payantes.

**Scénarios.** Après A→B à 100 XP puis B→C à 200 XP, un retour rembourse 200 ; le suivant sélectionne encore 200 et peut échouer ou trop rembourser. Après des compétences payantes totalisant 75 XP, retirer le talent initial gratuit peut rembourser ces 75 XP tout en conservant les compétences achetées.

**Correction attendue.** Rattacher chaque acquisition à son coût effectivement payé et chaque remboursement à l’écriture d’origine ; empêcher le double remboursement. Ajouter les cas de retours successifs et de gratuité mélangée à des dépenses réelles. Les tests existants ne couvrent qu’un retour simple et un remboursement gratuit sans autre dépense.

Preuves : [PersonnageService.cs, L534–547, L279–287, L353–377 et L599–600](../app/src/Wfrp4.Server/Services/PersonnageService.cs#L534), [PersonnageServiceCreationTests.cs, L165–197 et L261–286](../app/tests/Wfrp4.Server.Tests/PersonnageServiceCreationTests.cs#L165).

### D05 · P2 — Les blessures maximales deviennent périmées après une avance

**Constat.** La formule des blessures utilise F, E et FM, mais elle n’est pas rappelée par `AvancerCaracteristique`. L’API restitue la valeur persistée.

**Scénario.** Passer E de 39 à 40 devrait ajouter 2 selon la formule actuellement implémentée ; les blessures restent inchangées jusqu’à une autre opération déclenchant le recalcul.

**Correction attendue.** Recalculer les attributs dérivés dans la même opération que la progression, ou les calculer à la lecture. Tester le passage et le retour des seuils de dizaines. Ce contrôle porte sur la cohérence interne du code, sans certifier l’ensemble des règles officielles par espèce.

Preuves : [PersonnageService.cs, L187–199 et L221–237](../app/src/Wfrp4.Server/Services/PersonnageService.cs#L187), [PersonnagesController.cs, L192](../app/src/Wfrp4.Server/Controllers/PersonnagesController.cs#L192).

### D06 · P2 — Le seed ne décrit pas tous les plans de progression

**Constat.** Quatre niveaux sont créés avec intitulé et statut, mais les compétences et talents sont renseignés pour le seul niveau 1. Aucune affectation d’`AvancesCarac` n’est présente dans le seeder. Les règles de progression interprètent certaines données absentes comme l’absence de prérequis.

**Impact.** Sur une base neuve issue de ce seed, les niveaux supérieurs ne portent pas les nouveaux choix attendus et certains contrôles de progression sont incomplets. Une base enrichie manuellement pourrait différer ; son contenu n’a pas été inspecté.

**Correction attendue.** Compléter les quatre niveaux à partir du référentiel retenu, versionner ces données et tester leur complétude, leurs codes et leurs prérequis. Distinguer explicitement une liste volontairement vide d’une donnée manquante.

Preuves : [Wfrp4DataSeeder.cs, L163–169, L385–388 et L466–471](../app/src/Wfrp4.Infrastructure/Data/Wfrp4DataSeeder.cs#L163), [PersonnageService.cs, L430–442 et L564–576](../app/src/Wfrp4.Server/Services/PersonnageService.cs#L430).

### D07 · P2 — Le snapshot EF ne reflète plus les aventures

**Constat.** Le contexte et la migration SQL ajoutent `AventuresSauvegardees`, mais cette entité manque au snapshot du modèle. Le démarrage ignore globalement `PendingModelChangesWarning`.

**Risque.** La prochaine migration générée peut proposer de recréer une table déjà existante ; l’alerte qui permettrait de détecter la dérive est neutralisée. La migration d’aventures elle-même existe : ce constat ne signifie pas que la table est toujours absente.

**Correction attendue.** Réconcilier le snapshot et la migration sans recréer la table, retirer la suppression globale de l’alerte et vérifier les migrations sur base vide et base existante. Exécuter ensuite le contrôle EF des changements de modèle en CI.

Preuves : [Wfrp4DbContext.cs, L32](../app/src/Wfrp4.Infrastructure/Data/Wfrp4DbContext.cs#L32), [migration aventures, L18–34](../app/src/Wfrp4.Infrastructure/Data/Migrations/20260915120000_AddAventuresSauvegardees.cs#L18), [snapshot EF](../app/src/Wfrp4.Infrastructure/Data/Migrations/Wfrp4DbContextModelSnapshot.cs), [Program.cs, L17](../app/src/Wfrp4.Server/Program.cs#L17).

## 7. Interface et expérience utilisateur

### U01 · P1 — Une modification annulée peut être enregistrée avec la bourse

**Constat.** Le bouton Annuler masque le formulaire sans réinitialiser `_edition`. `ModifierBourse` actualise seulement les champs monétaires de ce même brouillon puis transmet le DTO complet au serveur.

**Scénario déduit.** Modifier le nom ou la motivation, cliquer Annuler, puis ajouter une couronne : le changement annulé est inclus dans la sauvegarde de la bourse. À l’inverse, certaines opérations appellent `Recharger`, qui recrée le brouillon et peut effacer des modifications non sauvegardées.

**Correction attendue.** Réinitialiser le brouillon à l’annulation et isoler les commandes monétaires du formulaire d’identité. Protéger les modifications en cours lors d’un rechargement. Le scénario Annuler → modification de la bourse doit conserver l’identité initiale.

Preuves : [FichePersonnage.razor, L559, L598, L1384, L1482 et L2043–2060](../app/src/Wfrp4.Client/Pages/FichePersonnage.razor#L1482).

### U02 · P2 — Des événements MudBlazor utilisés pour charger des données ne sont pas reconnus

**Constat.** Le build signale `IsExpandedChanged` comme attribut illégal sur `MudExpansionPanel`. Cet attribut est utilisé pour charger les épisodes et les aventures sauvegardées lors de l’ouverture d’un panneau. Il faut traiter cet avertissement comme un problème de comportement à vérifier, et pas seulement de style.

**Impact attendu.** Le chargement prévu par le callback peut ne pas se produire lors de l’ouverture. Les autres usages de `Dense`, `Title` ou `IsInitiallyExpanded` nécessitent également une revue ciblée ; les 62 avertissements n’impliquent pas 62 défauts fonctionnels distincts.

**Correction attendue.** Aligner les paramètres sur l’API MudBlazor installée, puis tester l’ouverture des aventures et épisodes avec données sauvegardées. Faire disparaître les avertissements de paramètres inconnus sans les masquer.

Preuves : [MjTools.razor, L724 et L755](../app/src/Wfrp4.Client/Pages/MjTools.razor#L724), avertissements `MUD0002` observés pendant `dotnet test`, [Wfrp4.Client.csproj, L14](../app/src/Wfrp4.Client/Wfrp4.Client.csproj#L14).

### U03 · P2 — Plusieurs écrans n’ont pas de reprise explicite après une erreur de chargement

**Constat.** Dashboard, fiche, wizard et référentiels réalisent des chargements initiaux sans gestion locale homogène de l’erreur ni action Réessayer. Un échec peut laisser l’état de chargement actif ou remonter au traitement global. Le démarrage visuel impose en outre un minimum de quatre secondes et ne prévoit pas de `catch` sur `Blazor.start()`.

**Impact.** Une indisponibilité API ou un échec du bootstrap peut présenter une interface bloquée ou peu explicite. Le délai de quatre secondes est une règle du code, pas une mesure de performance globale.

**Correction attendue.** Prévoir des états chargement, vide, erreur et succès, libérer les indicateurs dans `finally`, offrir une reprise et rendre le message de démarrage accessible. Afficher immédiatement l’application dès qu’elle est prête.

Preuves : [Dashboard.razor, L76–88](../app/src/Wfrp4.Client/Pages/Dashboard.razor#L76), [FichePersonnage.razor, L1363–1410](../app/src/Wfrp4.Client/Pages/FichePersonnage.razor#L1363), [CreationWizard.razor, L841–851](../app/src/Wfrp4.Client/Pages/CreationWizard.razor#L841), [index.html, L465–479](../app/src/Wfrp4.Client/wwwroot/index.html#L465).

### U04 · P2 — La navigation comporte des obstacles d’accessibilité identifiables dans le code

**Constat.** Des cartes du tableau de bord déclenchent la navigation uniquement avec `@onclick`, sans lien ou bouton explicite ni gestion clavier dans ce code. Plusieurs commandes à icône ne possèdent pas de nom accessible explicite ; le sélecteur de thème natif ne possède pas de label associé.

**Impact à vérifier au navigateur.** Les parcours au clavier ou au lecteur d’écran risquent d’être incomplets. La présence de MudBlazor ne garantit pas, à elle seule, l’accessibilité des usages personnalisés. Aucun audit WCAG complet, contraste calculé ou test mobile de l’application n’a été effectué.

**Correction attendue.** Utiliser des liens et boutons sémantiques, nommer les commandes et associer les labels. Vérifier Tab, Entrée, Espace, visibilité du focus et lecture des noms sur les parcours essentiels.

Preuves : [Dashboard.razor, L93–115](../app/src/Wfrp4.Client/Pages/Dashboard.razor#L93), [MainLayout.razor, L10–34](../app/src/Wfrp4.Client/Layout/MainLayout.razor#L10).

### U05 · P2 — Le designer PDF propose une sauvegarde interdite au joueur

**Constat.** Le menu présente le designer PDF aux utilisateurs connectés ; la page et le bouton Enregistrer ne sont pas réservés aux administrateurs, alors que le PUT serveur l’est.

**Impact.** Un joueur peut passer du temps à éditer une mise en page avant de recevoir un refus 403. Le contrôle serveur est un point positif ; l’incohérence concerne le parcours proposé.

**Correction attendue.** Montrer une consultation ou une prévisualisation clairement identifiée pour les joueurs et réserver la sauvegarde aux utilisateurs autorisés, ou définir une vraie sauvegarde personnelle distincte.

Preuves : [NavMenu.razor, L17–22](../app/src/Wfrp4.Client/Layout/NavMenu.razor#L17), [PdfDesigner.razor, L2 et L41–43](../app/src/Wfrp4.Client/Pages/PdfDesigner.razor#L41), [PersonnagesController.cs, L380–381](../app/src/Wfrp4.Server/Controllers/PersonnagesController.cs#L380).

## 8. Exploitation, qualité et maintenance

### O01 · P1 conditionnel — Le Compose livré est une configuration de démonstration

**Constat.** Keycloak démarre en `start-dev`, l’application en `Development`, les services utilisent HTTP, des ports sont publiés sans restriction explicite à la boucle locale, et des comptes et valeurs d’accès d’exemple sont versionnés. Le Dockerfile annonce Production, mais le Compose surcharge ce réglage.

**Condition de risque.** Le blocage concerne une réutilisation de cette configuration telle quelle sur un serveur accessible. Les valeurs de démonstration ne sont pas la preuve d’une fuite de secrets de production ; aucun secret n’est reproduit dans ce rapport.

**Correction attendue.** Fournir un profil de production avec TLS/proxy configuré, secrets injectés, comptes exemples supprimés, administration et PostgreSQL sur réseau privé, utilisateur de conteneur non privilégié, sondes de disponibilité et sauvegarde/restauration documentée. Synchroniser aussi le secret du client de service entre l’import Keycloak et l’application lors de sa rotation.

Preuves : [docker-compose.yml, L23, L29–41 et L54–64](../app/docker-compose.yml#L23), [Dockerfile](../app/Dockerfile), [wfrp4-realm.json, L43 et L149–177](../app/keycloak/wfrp4-realm.json#L43).

### O02 · P2 — Les tests et l’automatisation ne protègent pas encore les parcours critiques

**Constat.** Les 19 tests couvrent surtout les calculs et la création ; les tests de données utilisent EF InMemory. Aucun workflow GitHub Actions n’est versionné, aucun test d’intégration HTTP ou de parcours navigateur n’est présent dans les projets examinés.

**Impact.** Des régressions d’autorisation, de contraintes PostgreSQL, de migrations et de chargement de composants peuvent coexister avec une suite entièrement verte, comme le montre cet audit.

**Correction attendue.** Automatiser restore, build, tests et audit des dépendances ; ajouter d’abord les tests des défauts de ce rapport, puis une suite d’intégration avec PostgreSQL et une matrice de rôles API. Tester les migrations à partir d’une base vide et d’une version précédente. Fixer le SDK et préciser les étapes de publication reproductibles.

Preuves : [tests Server](../app/tests/Wfrp4.Server.Tests/PersonnageServiceCreationTests.cs#L317), [test Infrastructure, L12](../app/tests/Wfrp4.Infrastructure.Tests/UnitTest1.cs#L12), inventaire Git de `.github/workflows`, résultats exécutés au §4.

### O03 · P2 — Le socle .NET doit être entretenu et sa migration planifiée

**Constat local.** Les projets applicatifs ciblent .NET 9 ; plusieurs packages Microsoft sont fixés à `9.0.14`, Npgsql à `9.0.4`, et les images Docker utilisent les tags `9.0`.

**État vérifié au 19 septembre 2026.** .NET 9 est encore supporté, en phase de maintenance, jusqu’au **10 novembre 2026**. Le correctif courant indiqué par Microsoft est **9.0.20**. .NET 10 LTS est annoncé supporté jusqu’au **14 novembre 2028**. Il serait inexact de qualifier .NET 9 de déjà hors support à la date de cet audit. Source : [politique officielle de support .NET](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core).

**Correction attendue.** Vérifier et mettre à jour les correctifs applicables aux packages et images, puis planifier le passage à .NET 10 LTS avant l’échéance. Valider Blazor, EF/Npgsql, authentification et génération PDF après migration. L’absence d’alerte NuGet connue n’atteste pas que tous les composants déployés sont à jour.

Preuves locales : [Wfrp4.Server.csproj, L10–12 et L30](../app/src/Wfrp4.Server/Wfrp4.Server.csproj#L10), [Wfrp4.Infrastructure.csproj, L8–14](../app/src/Wfrp4.Infrastructure/Wfrp4.Infrastructure.csproj#L8), [Dockerfile](../app/Dockerfile).

### O04 · P2 — La concentration du code et le démarrage compliquent l’exploitation

**Constat.** `MjTools.razor` comporte **3 682 lignes**, `FichePersonnage.razor` **2 118**, `CreationWizard.razor` **1 243** et le seeder **1 743**. Plusieurs responsabilités et états y sont couplés. L’API applique migrations et seed à chaque démarrage ; elle attend également les clés Keycloak avant de construire le serveur. Aucun endpoint de santé n’est configuré. Le middleware désactive le cache pour toutes les routes hors `/api`, y compris les ressources de l’application.

**Impact.** Les changements sont plus difficiles à isoler et tester. Le démarrage dépend des services externes et de l’état des données ; la stratégie de migration est peu adaptée à plusieurs instances. L’interdiction globale de cache peut augmenter les transferts au rechargement, sans mesure chiffrée effectuée ici.

**Correction attendue.** Extraire des composants et services selon les fonctions réelles : fiche/identité, bourse, progression, aventures, génération, mise en page. Séparer les migrations de la mise en service en production, ajouter des sondes et des erreurs structurées, puis mesurer le démarrage et adopter une stratégie de cache adaptée aux ressources versionnées.

Preuves : [MjTools.razor](../app/src/Wfrp4.Client/Pages/MjTools.razor), [FichePersonnage.razor](../app/src/Wfrp4.Client/Pages/FichePersonnage.razor), [Program.cs, L12 et L94–154](../app/src/Wfrp4.Server/Program.cs#L94).

## 9. Ce qui a changé depuis l’état des lieux de juillet

Le document [état des lieux de juillet 2026](etat-des-lieux-2026-07.md) reste utile comme historique, mais plusieurs de ses conclusions ne décrivent plus le code actuel.

| Sujet | État observé en septembre |
| --- | --- |
| Avancement des talents | Branché dans le contrôleur, contrairement à l’ancien constat |
| Exports PDF | Fiche personnage, designer et exports MJ présents |
| Fonctionnalités | Inscription, grimoire, titres, bestiaire, aventures et outils MJ ont élargi le produit |
| Tests | 19 cas exécutés, contre 3 décrits en juillet |
| Audience JWT | Toujours désactivée ; constat maintenu |
| Workflows CI versionnés | Toujours absents du répertoire attendu |
| Hygiène des artefacts | Les `bin/obj` de l’application ne sont pas suivis ; des fichiers `obj` de templates sous `.claude` restent suivis |
| Documentation | README racine limité au nom du dépôt ; spécification à réaligner avec les nouvelles fonctionnalités et les règles XP |

La présence de fichiers ignorés sur un poste ne signifie pas qu’ils sont versionnés. Les recommandations de nettoyage doivent donc s’appuyer sur `git ls-files`, et ne pas reprendre sans contrôle les chiffres de l’ancien document.

## 10. Plan de correction proposé

Les tailles ci-dessous sont des ordres de grandeur de travail, sans engagement calendaire : **S** = modification localisée ; **M** = plusieurs couches ou tests d’intégration ; **L** = chantier de données ou d’exploitation. Elles doivent être confirmées après arbitrage métier.

| Ordre | Livraison attendue | Constats | Taille | Validation de sortie |
| --- | --- | --- | --- | --- |
| 1 | Droits de suppression cohérents et génération PDF bornée | S01, S02 | S–M | Refus de toute écriture par partage Lecture ; entrées extrêmes rejetées rapidement |
| 2 | Création fiable et brouillons réellement annulables | D01, U01 | M | Requêtes hors règles refusées ; Annuler puis bourse ne modifie pas l’identité |
| 3 | Compteurs et remboursements exacts | D02, D03, D04, D05 | M–L | Tests simultanés, gratuit/payant, retours successifs et seuils de caractéristiques |
| 4 | Référentiels et migrations cohérents | D06, D07 | M–L | Quatre niveaux complets ; base vide et base existante migrées sans dérive |
| 5 | Authentification et inscription robustes | S03, S04, S05 | M | Mauvaise audience refusée ; rotation transparente ; inscription récupérable |
| 6 | Parcours compréhensibles et accessibles | U02, U03, U04, U05 | M | Données chargées à l’ouverture ; reprise réseau ; clavier ; droits visibles |
| 7 | Préproduction reproductible | O01, O02, O03 | M–L | Profil production, CI, restauration vérifiée, versions entretenues |
| 8 | Réduction progressive des composants volumineux | O04 | L, incrémental | Responsabilités isolées, comportement conservé et mesures avant/après |

**Répartition suggérée.** Backend/sécurité pour S01–S05 et D01–D05 ; données/métier pour D06–D07 ; frontend pour U01–U05 ; exploitation et qualité pour O01–O04. L’arbitrage sur le mode d’ajustement libre des XP revient au responsable produit et au référent de règles.

## 11. Critères de validation avant ouverture publique

- Une matrice API vérifie propriétaire, tiers, MJ Lecture, MJ XP, administrateur et partage expiré pour chaque opération de lecture, écriture, suppression et octroi XP.
- Les valeurs de création invalides sont refusées sans personnage partiellement persisté ; les réserves initiales et les règles de carrière sont explicites.
- Deux opérations concurrentes ne perdent aucun gain ou coût XP ; chaque remboursement correspond à une acquisition réellement payée et non déjà annulée.
- Les blessures et autres attributs dérivés suivent immédiatement les changements qui les affectent.
- Les migrations passent sur PostgreSQL depuis une base vide et une version existante ; le snapshot correspond au modèle.
- L’export PDF refuse les volumes hors limites et respecte l’annulation ; les principales sorties sont vérifiées visuellement.
- Une panne réseau, une erreur d’inscription ou une rotation Keycloak disposent d’un comportement testé et compréhensible.
- Les parcours de connexion, création, progression, partage, aventures et export sont validés au navigateur, au clavier et sur petit écran.
- Une CI rejoue les contrôles ; le profil de production exclut les comptes exemples et comprend une restauration de sauvegarde réellement testée.

Ces critères forment un seuil de validation proposé. Ils ne décrivent pas des corrections déjà réalisées par cet audit.

## 12. Reproduire les vérifications

Depuis la racine du dépôt, avec les SDK et dépendances nécessaires :

```powershell
dotnet test app/Wfrp4.sln --no-restore --configuration Release
dotnet build app/Wfrp4.sln --no-restore --configuration Release
dotnet list app/Wfrp4.sln package --vulnerable --include-transitive --no-restore
git ls-files .github/workflows
```

Les options `--no-restore` reproduisent le contrôle effectué avec le cache existant. Pour vérifier un environnement neuf, ajouter une restauration préalable autorisée puis une construction de l’image Docker ; ces deux validations restent à faire. L’audit NuGet exécuté ici utilisait une configuration temporaire limitée à la source officielle NuGet ; elle n’est pas requise dans un environnement disposant déjà de sa configuration normale.

**Livrables :** ce Markdown constitue la source de l’audit ; [WFRP.audit.html](WFRP.audit.html) en présente une conversion intégrale, autonome et imprimable. Les constats sont valables pour la révision indiquée et doivent être réévalués après correction.
