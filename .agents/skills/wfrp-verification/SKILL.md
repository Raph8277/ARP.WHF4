---
name: wfrp-verification
description: Exécuter et examiner les contrôles du harness WFRP et établir les preuves de non-régression après une correction.
---

# Vérification et harness

Lire AGENTS.md et docs/WFRP.harness.md. Lancer node scripts/harness/verify.cjs depuis le dépôt après intégration.
La commande compile et exécute les tests sans restauration ni démarrage des services. Restaurer explicitement si les dépendances manquent. Un échec doit rester un échec ; ne pas supprimer un test ou masquer une alerte pour créer une preuve verte.
Les logs et TRX sont dans tmp/verification/. La preuve receipt.json est liée à HEAD et au contenu des sources, tests et scripts ; toute édition ultérieure exige une nouvelle validation.
Pour une correction sécurité/métier, vérifier qu'un test échoue sur le comportement antérieur et réussit après correction.
Le hook Stop demande au plus une continuation, pour éviter une boucle quand le SDK ou une dépendance manque. Il ne remplace pas une CI ni une protection de branche.
Mettre à jour docs/WFRP.fiabilisation.md avec scénario, test, résultat et limites. Ne pas confondre tests InMemory et PostgreSQL, ni tests unitaires et parcours HTTP.

Avant de conclure une correction, suivre le contrôle commun de AGENTS.md.
