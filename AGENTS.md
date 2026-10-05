# ARP.WHF4 — travail sur l'application

Application .NET 9 : Blazor WebAssembly/MudBlazor, API ASP.NET Core,
EF Core/PostgreSQL et Keycloak. Solution : `app/Wfrp4.sln`.

## Routage

Lire uniquement le skill utile sous `.agents/skills/` :
- `wfrp-security` : droits HTTP, partages, authentification, génération PDF.
- `wfrp-domain-data` : création, XP, invariants, EF et migrations.
- `wfrp-blazor` : état des formulaires, appels API et composants MudBlazor.
- `wfrp-verification` : tests, hooks, validation et compte rendu de fiabilisation.

Les agents correspondants sont définis dans `.codex/agents/`. Conserver le
modèle et l'effort hérités. Lorsqu'une délégation est demandée, attribuer des
fichiers distincts à chaque agent ; le coordinateur exécute seul la validation
de la solution après intégration, pour éviter les builds concurrents.

## Invariants du projet

- Un partage Lecture ou XP ne donne aucun droit de modifier la fiche, son
  équipement ou ses sorts. L'octroi XP a son propre contrôle de permission.
- Les DTO client ne prouvent ni un droit ni la validité d'une règle métier.
- Préserver le comportement d'ajustement libre des XP tant qu'un arbitrage
  produit ne l'a pas remplacé. Ne pas inventer les référentiels de carrière.
- Toute mutation d'XP doit rester cohérente avec son historique. EF InMemory
  ne prouve pas l'isolation, les contraintes ou les migrations PostgreSQL.
- Ne pas lancer migrations, seeds ou remise à zéro contre une base existante
  pour vérifier une correction. Les tests d'intégration utilisent une base dédiée.
- Préserver les fichiers et réglages locaux existants, notamment
  `.claude/settings.local.json`. Ne pas publier ni déployer implicitement.

## Boucle de correction

Partir d'un scénario de `docs/WFRP.audit.md`, écrire un test de régression
observable pour une correction métier/sécurité, appliquer la correction puis
exécuter `node scripts/harness/verify.cjs`. Les dépendances doivent avoir été
restaurées ; le vérificateur ne démarre aucun service et n'applique aucune migration.
Les logs et la preuve liée au contenu courant sont dans `tmp/verification/`.

Conserver l'audit comme état historique. Mettre le suivi des corrections et les
limites de validation dans `docs/WFRP.fiabilisation.md`. Distinguer tests unitaires,
contrôleurs appelés directement, intégration HTTP et tests navigateur.
