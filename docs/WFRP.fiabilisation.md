# WFRP — suivi de fiabilisation

19 septembre 2026. Premier lot après l'[audit historique](WFRP.audit.md), établi
sur `2acede2`. Les modifications ci-dessous sont dans l'arbre de travail ; elles
sont déployées uniquement sur l'environnement local, sans publication distante.

## Harness installé

- [AGENTS.md](../AGENTS.md) définit les invariants et le routage.
- Quatre skills dans `.agents/skills/` : sécurité, métier/données, Blazor, vérification.
- Quatre agents correspondants dans `.codex/agents/`, sans imposer de modèle.
- Hooks `SessionStart`, `PostToolUse`, `Stop` dans `.codex/hooks.json`.
- Commande commune : `node scripts/harness/verify.cjs`.
- Workflow GitHub Actions présent, à exécuter après publication ; non exécuté à distance ici.

Voir [WFRP.harness.md](WFRP.harness.md) pour les prérequis et l'activation des hooks.
Leur exécution automatique exige une revue via `/hooks` ; les scripts sont testés
directement et la preuve de validation est calculée sur le contenu courant.

## Corrections de ce lot

| Constat | Changement | Preuve et limite |
| --- | --- | --- |
| S01 | Suppression d'une possession réservée au propriétaire/admin, avec vérification du personnage cible | Matrice de six cas exécutant le filtre et l'action : propriétaire/admin autorisés, MJ Lecture/XP, tiers et partage expiré refusés. Ce n'est pas un test HTTP avec Keycloak. |
| S02 | Validation préalable des paramètres PDF, bornes de volume, annulation et deux exports simultanés maximum par processus | Rejets testés avant lecture du template, validation des contenus et export ordinaire testé. Les codes 400/429 sont prévus par le contrôleur ; la limite HTTP de taille exige encore un test d'hébergement. |
| D01 — partiel | Bonus XP limité à 0/25/50 ; dix caractéristiques exactes entre 1 et 100 ; niveau initial 1 ; compatibilité espèce/carrière ; nom borné et listes obligatoires | Requêtes invalides rejetées sans personnage persisté. La borne 1–100 est une borne technique, pas une certification des tirages par espèce. Réserves Destin/Résilience et preuve serveur du tirage restent à concevoir. |
| D05 | Recalcul des blessures lors d'une avance de caractéristique | Test E 39→40 puis retour à 39, avec +2 puis −2 blessures selon la formule existante. Les formules officielles par espèce ne sont pas revues ici. |
| U01 — scénario corrigé | Annuler reconstruit le brouillon ; commande de bourse dédiée, sans champs d'identité ; une erreur de bourse ne recharge plus toute la fiche | Neuf tests de la commande monétaire : droits, préservation de l'identité, trois monnaies, zéro et débordements. Parcours Razor Annuler→bourse restant à valider au navigateur ; les autres rechargements de fiche restent à examiner. |
| U02 — partiel | `ExpandedChanged` remplace les callbacks inconnus ; deux panneaux de l'atelier utilisent `Expanded` ; chargement des sauvegardes attendu avec `await` | Compilation avec l'API MudBlazor 9.2.0 installée. Les autres avertissements de paramètres et les parcours navigateur restent à traiter. |
| PDF-UX | Éditions sémantiques distinctes pour PNJ, trésor, aventure et campagne ; presets de densité, aperçu/téléchargement, notes imprimables et variantes MJ/joueurs | Trois régressions vérifient pagination longue, accents, sections trésor et masquage des secrets. Les PDF de référence ont été rendus en PNG et inspectés ; les tests ne couvrent pas encore le parcours navigateur ni l'impression physique. |
| AVENTURE-NARRATION | Le pitch dépend désormais du thème et du lieu ; un synopsis développé relie ambiance, accroche, menace, itinéraire, retournement, climax et conséquence d'échec. Une variante joueurs exclut les révélations MJ et est utilisée par son export PDF. | Huit cas de régression couvrent les cinq thèmes, la structure en paragraphes, la cohérence du contexte et l'absence de fuite du synopsis MJ dans le PDF joueurs. L'affichage Razor compile, mais reste à valider dans un parcours navigateur. |
| PDF-INTERLIGNE | L'interligne des champs PDF multilignes est désormais calculé dans le repère du template à partir de la taille réelle de la police, avec deux points de respiration. Les retours à la ligne du synopsis produisent de vrais paragraphes au lieu de caractères `?`. | Deux régressions inspectent les lignes de base du flux PDF : écart de 12 à 14 points en densité standard et ligne blanche entre deux paragraphes. |
| PDF-TITRE | L'en-tête des aventures et campagnes réserve désormais une séparation verticale suffisante entre le libellé du dossier, le grand titre et le trait horizontal. | Une régression contrôle un écart de 28 à 30 points entre les lignes de base du libellé et du titre. |
| CARTES-ALÉATOIRES | L'onglet Plans de l'Atelier MJ génère quatre familles de lieux : donjon, grotte, auberge et village/ville. Les plans de localités sont raccordés au référentiel existant de l'Atelier MJ : nom, type, population et région sont repris sans créer un second catalogue. La population et le statut règlent la densité et l'implantation de l'habitat. Inspirées du principe « block-centric » du [Medieval Fantasy City Generator de Watabou](https://watabou.itch.io/medieval-fantasy-city-generator), les localités sont découpées en îlots polygonaux autour du marché ; les rues occupent l'espace entre les îlots, et les bâtiments restent contenus dans leur parcelle. L'enceinte conserve ses portes et les villes se densifient selon leur statut. Le réglage de ville comprend l'emprise (65–160 %), la densité bâtie (65–140 %), la forme compacte, étirée ou étalée, et la largeur de rue (60–150 %). Rivière, côte et forêt sont combinables. Les lieux détaillés restent dans l'emprise terrestre et leurs détails suivent leur déplacement. Les options sont conservées à l'export et à l'ouverture du projet JSON. L'auberge forme un bâtiment continu ; le donjon utilise des salles irrégulières et des corridors à plusieurs coudes. | Les régressions couvrent la génération des quatre familles, le format projet, les paramètres urbains, l'emprise et l'eau, ainsi que la densité, les rues et les quartiers. Tests unitaires, sans parcours visuel authentifié. L'import reste limité à 1 Mio et valide identifiants, bornes et rattachements avant de remplacer le plan courant. |
| O02 — partiel | Régressions ajoutées, harness exécutable et workflow CI | Validation locale réussie ; les intégrations PostgreSQL/Keycloak et la CI distante restent à exécuter. |

Limites PDF de ce lot : 2 Mio de corps HTTP, 64 sections, 512 lignes, 128 profils,
100 000 caractères au total, 64 pages et 4 Mio de commandes de dessin. La limite
des commandes de dessin exclut les images du template local. Les paramètres de
mise en page sont bornés et les couleurs hexadécimales sont validées. Les exports
au-delà de ces limites sont refusés, sans découpage implicite.

Le nouvel endpoint `POST /api/personnages/{id}/bourse` reçoit uniquement
`{ "monnaie": "C" | "P" | "S", "delta": entier }`. Il conserve le plancher zéro,
refuse les deltas hors ±1 000 000 et les dépassements d'entier. Il protège
l'identité mais ne résout pas les mises à jour concurrentes de la bourse.

## Vérifications

- Paramétrage urbain affiné : emprise continue de 65 à 160 %, densité bâtie de
  65 à 140 %, morphologie compacte/étirée/étalée, et largeur de rue de 60 à 150 %.
  Les quatre réglages sont enregistrés dans le projet, mettent à jour l'aperçu et
  déplacent les repères pour qu'ils restent dans les terres. Côte, rivière et
  forêt restent combinables. Les tests couvrent chaque axe, la sauvegarde JSON,
  les bornes d'import et l'emprise terrestre ; le rendu complet reste à contrôler
  dans le navigateur authentifié.
- Optimisation de l'atlas urbain : rues, toits, arbres et traversées de pont sont
  regroupés en chemins SVG, et les bâtiments ne comparent leurs collisions qu'aux
  voisins spatiaux. Le plan est réutilisé entre le recalage des lieux et le rendu ;
  densité, largeur de rue et forêt ne déclenchent plus un recalage inutile.
  Sur une cité maximale mixant forêt, côte et rivière, le rendu Release passe de
  12 036 à environ 165 éléments `<path>` et de 1,05 Mo à 144 Ko. C'est une mesure
  directe du moteur .NET Release, sans le coût DOM/WebAssembly du navigateur.

- Référence visuelle Doverley fournie par l'utilisateur : le rendu urbain utilise
  désormais `SettlementAtlas`, indépendant des salles de donjon. Petites emprises
  alignées sur des rues partagées, cours, jardins, rivière traversante et ponts,
  faubourg étiré, palette mate et quatre noms de quartiers. Aucun code ni asset de
  Watabou n'est incorporé. Les paramètres de localité et le numéro restent
  déterministes ; les repères existants sont accessibles via « Lieux et détails ».
  Les maisons de fond et les rues ne sont pas individuellement éditables. Les
  anciennes descriptions ci-dessous concernant les enceintes et la voirie du
  composant restent l'historique des itérations, pas les capacités de cet atlas.
  Cinq régressions contrôlent les chevauchements, les bornes, l'eau, le déterminisme,
  les densités et l'encodage XML du titre. Aperçus SVG du même moteur générés dans
  `tmp/atlas-preview/` et inspectés en PNG ; ce contrôle n'est pas un parcours
  authentifié de l'éditeur complet. La connexité globale des rues reste à couvrir.

- Ergonomie des plans : « Graine » devient « Numéro de génération », avec une
  explication visible du déterminisme et du bouton Surprise. Les actions Surprise
  et Générer sont aussi disponibles sous le plan, alignées à droite, et réutilisent
  les mêmes événements que les actions en haut. Ce changement ne constitue pas
  une validation de la qualité visuelle des villes et villages.

- Avant correction : **15 tests de régression en échec**, quatre cas autorisés
  ou déjà refusés réussis, sans requête de surcharge ni suppression réelle.
- Après correction : **56 tests .NET réussis** (55 serveur, 1 infrastructure),
  **4 tests du harness réussis**, compilation Release réussie.
- Après ajout de la narration d'aventure : **67 tests .NET réussis**
  (66 serveur, 1 infrastructure), harness et compilation Release réussis.
- Après ajustement final de l'en-tête PDF : **70 tests .NET réussis**
  (69 serveur, 1 infrastructure), harness et compilation Release réussis.
- Après ajout de l'éditeur de plans : **76 tests .NET réussis**
  (75 serveur, 1 infrastructure), harness et compilation Release réussis.
- Après enrichissement interactif des plans : **78 tests .NET réussis**
  (77 serveur, 1 infrastructure), harness et compilation Release réussis.
- Après ajout des auberges et villages : **88 tests .NET réussis**
  (87 serveur, 1 infrastructure), harness et compilation Release réussis.
- Après refonte médiévale des plans : **90 tests .NET réussis**
  (89 serveur, 1 infrastructure), harness et compilation Release réussis.
- Après refonte des donjons : **91 tests .NET réussis**
  (90 serveur, 1 infrastructure), harness et compilation Release réussis.
- Après raccordement des plans aux localités existantes : **93 tests .NET réussis**
  (92 serveur, 1 infrastructure), harness et compilation Release réussis.
- Après enrichissement visuel des localités : **94 tests .NET réussis**
  (93 serveur, 1 infrastructure), harness et compilation Release réussis.
- Après ajout de la sauvegarde et de la réouverture des projets de carte :
  **96 tests .NET réussis** (95 serveur, 1 infrastructure), harness et compilation
  Release réussis. Les deux nouvelles régressions sont unitaires ; elles ne
  simulent pas le sélecteur de fichier du navigateur ni le téléchargement.
- Après structuration des villes et villages en quartiers inspirés de Watabou :
  **100 tests .NET réussis** (99 serveur, 1 infrastructure), harness et compilation
  Release réussis. Les nouvelles régressions contrôlent les géométries générées,
  pas encore leur lisibilité dans toutes les tailles d'écran.
- Après différenciation des paysages et raccordement complet de la voirie :
  **101 tests .NET réussis** (100 serveur, 1 infrastructure), harness et compilation
  Release réussis. La régression unitaire garantit que la route
  principale dépasse les deux bords ; le pont et les textures restent à confirmer
  visuellement dans un navigateur authentifié.
- Après hiérarchisation du réseau urbain, ajout des portes et réservation des rues :
  **103 tests .NET réussis** (102 serveur, 1 infrastructure), harness et compilation
  Release réussis. Deux nouvelles régressions prouvent la connexité de tous les
  quartiers vers le marché et la présence d'une porte sur chaque face de l'enceinte ;
  elles ne remplacent pas une inspection visuelle authentifiée.
- Après inspection de la capture d'un village généré, suppression des boucles
  polygonales de quartier et de la ceinture globale, réduction des aplats et des
  libellés, et limitation des ruelles aux bourgs, villes et cités. Le total reste
  **103 tests .NET réussis** ; cette correction concerne principalement la lisibilité SVG.
- Après décision de reprendre entièrement la lisibilité des localités, séparation
  du rendu urbain et du rendu générique des donjons : les passages techniques ne
  sont plus superposés aux rues, les polygones de quartiers deviennent une seule
  empreinte urbaine discrète, la densité des bâtiments baisse et la légende décrit
  désormais routes, rues, lieux et paysage. Un passage choisi dans l'inspecteur
  reste ponctuellement surligné afin de préserver son édition. Le total reste
  **103 tests .NET réussis** ; le parcours visuel authentifié demeure la limite.
- Après reprise « block-centric » des villes et villages, les anciens chemins
  dessinés par-dessus le plan ont été retirés du composant. La voirie est désormais
  formée par les intervalles continus entre des îlots rétractés ; les bâtiments sont
  contenus dans ces îlots, moins nombreux, alignés localement et moins contrastés.
  La place du marché, les approches extérieures, les repères importants et trois
  libellés au plus structurent la lecture. Les **33 tests ciblés de cartes** et la
  compilation Razor réussissent ; le parcours visuel authentifié demeure la limite.
- Les éditions PDF de référence PNJ joueurs, trésor MJ et aventure longue ont
  été rendues en PNG. La première inspection a révélé un débordement horizontal ;
  le calcul de césure a été corrigé puis les trois pages d'aventure finales ont
  été réinspectées sans chevauchement ni texte hors cadre.
- Les quatre définitions d'agents sont parsées en TOML et les quatre skills
  sont soumis au validateur du skill-creator.
- La compilation incrémentale finale n'émet pas les avertissements du build
  précédent. Il reste **58 MUD0002** après les quatre remplacements dans
  l'atelier ; les usages obsolètes déjà signalés par l'audit restent également
  hors de ce lot. Ne pas interpréter le build incrémental comme leur résolution.

Les tests de données utilisent EF InMemory. Les tests d'accès appellent le filtre
et/ou le contrôleur directement. La connexion, les politiques HTTP, l'anti-abus
en hébergement réel, le navigateur et les migrations PostgreSQL ne sont pas
validés par ces tests.

## Administration des comptes et mot de passe (spec `docs/spec-roles-comptes-mdp.md`)

- Endpoint `api/admin/utilisateurs` (liste, détail, rôles, activation, e-mail de
  réinitialisation) réservé à la politique `Admin`, via le compte de service Keycloak.
  Liste blanche des trois rôles `wfrp4-*`, MJ/admin impliquent joueur côté serveur,
  refus (409) de rétrograder ou désactiver soi-même ou le dernier admin actif,
  compte de service masqué, erreurs Keycloak traduites en 503 sans corps brut.
- Page client `/admin/utilisateurs` et lien de menu visibles pour l'admin ; lien
  « Changer mon mot de passe » vers la console de compte Keycloak.
- Realm : `resetPasswordAllowed`, protection force brute, locale `fr`, lien valable
  15 minutes, SMTP Mailpit (service ajouté à `docker-compose.yml`).
- Comptes de test MJ et joueur : `app/keycloak/provision-local.ps1` lit
  `app/keycloak/local/test-users.json`, **ignoré par Git** (adresses et mots de passe
  réels). Le script est idempotent et sert aussi à appliquer les réglages du realm sur
  un Keycloak déjà initialisé, car l'import n'a lieu qu'à la création.
- Validé : 14 tests du contrôleur appelé directement avec un service simulé ; la
  solution compile et ses 128 tests .NET réussissent.
- **Non validé** : routes HTTP avec vrais jetons (401/403), appels réels à l'API
  d'administration Keycloak, envoi et consommation du lien e-mail, console de compte
  Keycloak, page Blazor dans un navigateur. `node` est absent de cette machine :
  `scripts/harness/verify.cjs` n'a pas été exécuté.
- Limite connue : un jeton déjà émis garde ses anciens rôles jusqu'à expiration.
- Hors périmètre : politique de mot de passe de production (le mot de passe de test
  de 4 caractères n'est accepté que sans politique) et journal d'audit en base.

## Suite ordonnée

1. **D02** : protéger compteurs XP et écritures monétaires contre les accès
   concurrents ; reproduire les collisions sur une base PostgreSQL de test.
2. **D04, D03** : relier les remboursements à leurs acquisitions ; arbitrer
   l'ajustement libre avant de modifier les règles de solde.
3. **D01, D06, D07** : compléter les réserves initiales et les référentiels,
   réconcilier le snapshot EF puis tester base vide et mise à niveau.
4. **S03–S05** : audience JWT, renouvellement JWKS et inscription récupérable.
5. **U01–U05** : vérifier les parcours réels, finir les paramètres MudBlazor,
   reprise réseau, accessibilité et visibilité des droits du designer.
6. **O01, O03, O04** : profil de production, entretien .NET, sauvegarde/restauration,
   sondes et découpage progressif des composants.

L'application n'est pas déclarée prête pour une ouverture publique à l'issue de
ce premier lot. Chaque constat conserve ses critères d'acceptation dans l'audit.
