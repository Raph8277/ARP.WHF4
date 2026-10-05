# Harness de fiabilisation WFRP

Le point d'entrée est [AGENTS.md](../AGENTS.md). Quatre skills locaux et quatre
agents spécialisés couvrent sécurité, métier/données, Blazor et vérification.
Ils héritent du modèle courant et des permissions de la session.

## Vérification reproductible

Prérequis : Git, Node.js 22 ou ultérieur, SDK/runtime .NET 9 et dépendances restaurées.
Depuis la racine :

```powershell
dotnet restore app/Wfrp4.sln
node scripts/harness/verify.cjs
```

La restauration n'est nécessaire que si les dépendances manquent ou changent.
Le vérificateur contrôle le diff, teste son propre mécanisme, compile en Release
et exécute les tests existants et de régression. Il produit logs, TRX et une preuve
`tmp/verification/receipt.json`. Cette preuve expire si HEAD, un fichier source,
un test ou la configuration du harness change. Les sorties `bin/obj` sont exclues.
Un verrou empêche deux validations simultanées. Après une interruption brutale,
vérifier que le processus indiqué dans `running.lock` est terminé avant de retirer
ce fichier et de relancer.

Le workflow [.github/workflows/verify.yml](../.github/workflows/verify.yml) rejoue
ce contrôle dans GitHub Actions. Il sera exécuté après publication du commit ;
sa présence locale ne prouve pas qu'une CI distante a déjà réussi.

## Hooks Codex

Configuration : [.codex/hooks.json](../.codex/hooks.json).

- `SessionStart` rappelle le point d'entrée et la commande commune.
- `PostToolUse` signale les erreurs de whitespace du diff après une commande ou édition.
- `Stop` demande une validation lorsque des sources ont changé sans preuve fraîche.
  Il ne relance qu'une fois ; ensuite il signale la limite au lieu de boucler.

Les scripts ne lancent ni serveur, ni seed, ni migration et ne modifient aucun
réglage de permissions. La preuve est un garde-fou de développement, pas une
attestation inviolable. Les protections de branche et une CI restent nécessaires.

**Activation :** après ouverture du projet, utiliser `/hooks` dans le CLI Codex
pour examiner et approuver les définitions. Codex exige l'approbation du projet
et de chaque définition actuelle ; les hooks non approuvés sont ignorés. Le
présent travail ne contourne pas cette revue. La commande de vérification reste
utilisable immédiatement, indépendamment de l'activation des hooks.

Sources officielles : [hooks](https://learn.chatgpt.com/docs/hooks),
[agents spécialisés](https://learn.chatgpt.com/docs/agent-configuration/subagents),
[skills locaux](https://learn.chatgpt.com/docs/build-skills).
