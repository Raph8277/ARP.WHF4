---
name: enrichir-lore
description: "Enrichir les données de lore WFRP4 — ajouter des villes, rencontres, factions religieuses, PNJ, ou lieux au générateur d'aventures et aux fichiers de référence."
---

# /enrichir-lore

Enrichir les pools de données du générateur d'aventures WFRP4 et les fichiers de référence de lore.

## Usage

```
/enrichir-lore                    # menu interactif des catégories
/enrichir-lore villes Bretonnie   # ajouter des villes en Bretonnie
/enrichir-lore rencontres Kislev  # ajouter des rencontres au Kislev
/enrichir-lore religion Morr      # enrichir les hooks de Morr
/enrichir-lore pnj                # ajouter prénoms/noms/professions/secrets
```

## What You Must Do When Invoked

### 1. Identifier la catégorie

Catégories disponibles :
- **villes** — Ajouter des villes/lieux à `AdvLocations` dans MjTools.razor + `docs/lore-geographie-vieux-monde.md`
- **rencontres** — Ajouter des rencontres à `AdvEncounters` dans MjTools.razor + `docs/lore-rencontres-vieux-monde.md`
- **religion** — Ajouter des hooks à `AdvReligiousFactions` dans MjTools.razor + `docs/lore-religions-vieux-monde.md`
- **pnj** — Ajouter prénoms/noms/professions/secrets dans MjTools.razor
- **lieux** — Enrichir `AdvLocationNamePrefixes`, `AdvLocationDescriptions`, etc.
- **narration** — Enrichir synopsis, hooks, twists, climax, resolutions
- **factions** — Enrichir `docs/lore-factions-organisations.md`

### 2. Consulter les fichiers de référence

Toujours lire les fichiers de lore existants pour éviter les doublons :
- `docs/lore-geographie-vieux-monde.md`
- `docs/lore-religions-vieux-monde.md`
- `docs/lore-rencontres-vieux-monde.md`
- `docs/lore-factions-organisations.md`

### 3. Consulter le code existant

Lire la section pertinente de `app/src/Wfrp4.Client/Pages/MjTools.razor` :
- Utiliser `Grep` pour trouver le dictionnaire/liste cible
- Vérifier les entrées existantes pour éviter les doublons

### 4. Générer le contenu

Règles de contenu :
- Tout en français (sans accents dans les identifiants C#)
- Fidèle au ton sombre et gritty de WFRP4
- S'appuyer sur le lore officiel du Vieux Monde
- Chaque entrée doit être distincte et évocatrice
- Les rencontres incluent un nombre de créatures (ex: `1d6+2`)
- Les villes incluent une description courte de leur caractère

### 5. Modifier les fichiers

**Toujours modifier les deux cibles** :
1. Le fichier .md de référence (docs/lore-*.md) — pour les agents et la documentation
2. Le fichier MjTools.razor — pour le générateur côté client

### 6. Compiler et vérifier

```bash
cd app/src/Wfrp4.Client && dotnet build --no-restore
```

Reporter les erreurs et les corriger avant de terminer.

## Sources de lore

- Livre de base WFRP4 : `docs/Warhammer_4_Livre_de_base_convert.md`
- Bestiaire : `docs/bestiaire-wfrp4.md`
- Sorts : `docs/sorts-wfrp4.md`
- Site externe (si accessible) : `https://wjrf.bimondiens.com`
