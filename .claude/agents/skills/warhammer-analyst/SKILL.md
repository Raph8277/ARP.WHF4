---
name: warhammer-analyst
description: 'Warhammer Fantasy Roleplay 4e — règles, personnages, carrières, compétences, magie, équipement, bestiaire et lore. Toutes les réponses en français.'
argument-hint: 'Décrivez le sujet : règle précise, carrière, sort, créature, compétence, équipement ou contexte de lore.'
user-invocable: true
---

# Warhammer Analyst

Utiliser ce skill quand la demande concerne les règles, le contenu ou le lore de Warhammer Fantasy Roleplay 4e édition (WFRP4).

## Ce que couvre ce skill
- Règles du jeu : tests, combat, blessures, conditions, corruption, entre les aventures.
- Création de personnage : espèces, attributs, groupe/carrière initial.
- Carrières : 8 classes, 64 carrières, avancement, changement de carrière, statut.
- Compétences et talents : compétences de base/avancées, groupées, spécialisations, liste des talents.
- Magie : système de la magie, dés de lancement, 8 collèges, sorts par collège.
- Équipement : armes, armures, économie, disponibilité.
- Bestiaire : créatures, PNJ, profils, traits spéciaux.
- Lore : religion, divinités, géographie du Reikland, conseils de MJ.

## Source de référence
Fichier unique : `docs/Warhammer_4_Livre_de_base_convert.md`

## Workflow
1. Identifier la thématique (règle, personnage, carrière, compétence, magie, équipement, bestiaire, lore).
2. Utiliser `Grep` sur `docs/Warhammer_4_Livre_de_base_convert.md` pour localiser le passage.
3. Charger la section avec `Read` (paramètres `offset` / `limit`).
4. Citer la page source entre parenthèses (p. ex. `p. 47`) pour chaque fait extrait.
5. Répondre entièrement en français.

## Règles de réponse
- Toujours citer la page source.
- Ne pas inventer de règles absentes du texte.
- Si une information couvre plusieurs thèmes, chercher dans plusieurs sections du fichier.
- Pour les valeurs numériques (coûts XP, seuils, caractéristiques), reproduire exactement le tableau source.
