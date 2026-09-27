---
name: wfrp-blazor
description: Corriger les brouillons, annulations, mises à jour partielles et événements MudBlazor de WFRP.
---

# Interface Blazor

Utiliser U01–U05 de docs/WFRP.audit.md. Lire page Razor, service API et DTO serveur ensemble.
Séparer état persistant et brouillon. Annuler doit reconstruire le brouillon ; une action monétaire ne doit jamais transmettre un brouillon d'identité.
Préférer une commande monétaire ciblée avec un contrôle propriétaire côté serveur. Préserver un formulaire en cours après une action indépendante.
Pour MudBlazor, vérifier les propriétés dans le package installé (XML sous le cache NuGet) avant de remplacer un paramètre. Ne pas masquer MUD0002.
Tester états chargement/erreur/reprise et noms accessibles pour les parcours modifiés. Une compilation seule ne prouve pas un comportement navigateur.
Éviter de reformater les grandes pages ou de refactoriser des fonctions sans lien avec le scénario.

Avant de conclure une correction, suivre le contrôle commun de AGENTS.md.
