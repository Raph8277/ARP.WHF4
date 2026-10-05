---
name: wfrp-domain-data
description: Fiabiliser création, XP, remboursements, attributs dérivés et migrations EF/PostgreSQL de WFRP.
---

# Règles métier et données

Utiliser D01–D07 de docs/WFRP.audit.md et le suivi courant. Tracer DTO → service → entité → historique.
Valider les dix codes de caractéristiques exacts, les valeurs autorisées, le niveau initial, la compatibilité espèce/carrière et les bonus côté serveur.
Préserver les acquisitions gratuites ; ne rembourser que les coûts payés correspondant à la mutation annulée. Le solde négatif en ajustement libre nécessite un arbitrage produit avant modification.
Mettre à jour les valeurs dérivées avec leurs sources. Tester les seuils et le retour en arrière.
Une transaction SaveChanges ne protège pas une lecture devenue obsolète. Tester une stratégie de concurrence sur PostgreSQL dédié avant de déclarer D02 clos.
Toute migration doit conserver modèle, designer et snapshot cohérents ; vérifier base vide et mise à niveau sans toucher une base existante non dédiée.
Les références métier sont dans .github/skills/warhammer-analyst/references/ et docs/ ; ne pas inventer de données de carrière absentes.

Avant de conclure une correction, suivre le contrôle commun de AGENTS.md.
