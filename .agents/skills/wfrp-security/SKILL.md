---
name: wfrp-security
description: Corriger les autorisations des personnages, les partages et les limites de génération PDF de WFRP.
---

# Sécurité API et PDF

Suivre S01–S05 dans docs/WFRP.audit.md et le suivi courant dans docs/WFRP.fiabilisation.md.
Lire le filtre PersonnageOwnerFilter, la policy du contrôleur et le contrôle dans chaque action : passer le filtre ne signifie pas pouvoir écrire.
Tester propriétaire, tiers, administrateur, MJ Lecture, MJ XP et partage expiré ; vérifier aussi que la ressource appartient au personnage demandé.
Pour les PDF, valider avant lecture du template et allocation : valeurs géométriques, volumes imbriqués, nombre de pages, annulation. Conserver les exports ordinaires.
Les tests de contrôleur appelés directement ne valident pas le middleware HTTP. Indiquer ce qui reste à couvrir avec Keycloak.
Ne jamais utiliser un payload massif pour prouver une absence de limite ; tester le rejet en amont.

Avant de conclure une correction, suivre le contrôle commun de AGENTS.md.
