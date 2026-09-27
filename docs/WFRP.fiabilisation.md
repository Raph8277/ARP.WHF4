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
| CARTES-ALÉATOIRES | L'onglet Plans de l'Atelier MJ génère quatre familles de lieux : donjon, grotte, auberge et village/ville. Les plans de localités sont raccordés au référentiel existant de l'Atelier MJ : nom, type, population et région sont repris sans créer un second catalogue. La population et le statut (village, bourg, ville ou cité) règlent la densité, l'importance de la voirie et l'implantation : village-rue, habitat groupé, habitat dispersé ou enceinte fortifiée. Les zones urbaines sont représentées par des grappes éditables de toitures variées, des monuments plus grands et un tissu dense de maisons d'arrière-plan ; dans les villes fortifiées, les lieux structurants restent contenus dans l'enceinte. Palissades, murailles, tours, végétation, grain de terrain, cadre, rose des vents et quelques labels de quartiers renforcent le rendu aérien médiéval. Les détails tactiques peuvent être masqués afin de ne pas couvrir le plan. Les voies principales et secondaires ont été affinées et éclaircies. Le paysage est modifiable entre campagne, rivière et port ; le port ajoute bassin, quais, embarcations et rivage. La région nuance le terrain. Le village s'organise autour d'une route sinueuse et l'auberge forme un bâtiment continu. Le donjon se développe désormais de proche en proche plutôt que sur une grille : salles de tailles irrégulières, grandes salles ponctuelles, maçonneries déformées, murs épais, fissures et corridors à plusieurs coudes. Braseros, sarcophages, gravats et statues complètent ses décors. L'édition à la souris, l'annulation et les exports SVG/PNG sont conservés. | Vingt-quatre cas testent les quatre styles, dont le déterminisme, les bornes, l'accessibilité, les détails, les orientations du village, la compacité de l'auberge, la diversité des implantations du donjon, la densité issue de la population, la conservation des données et du paysage lors du clonage, ainsi que la disposition urbaine issue du statut de localité. Compilation Razor réussie ; le parcours navigateur authentifié a permis de comparer puis d'ajuster la densité, les marqueurs, les labels et la voirie d'une cité portuaire. |
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
