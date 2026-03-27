# Warhammer Fantasy JdR 4e Édition — Projet d'Analyse

Ce workspace contient le texte extrait du Livre de Base de Warhammer Fantasy Roleplay 4e édition (WFRP4) en français, organisé par domaine sous forme de skills Copilot.

## Langue

Toutes les interactions concernant le contenu Warhammer doivent se faire en **français**.

## Structure

- `.github/agents/warhammer-analyst.agent.md` — Agent principal d'analyse
- `.github/skills/warhammer-*/` — Skills spécialisés par domaine (règles, personnages, carrières, etc.)
- `.github/scripts/extract_pdf.py` — Script d'extraction du PDF (PyMuPDF)

## Convention

- Les références de pages correspondent au PDF "Warhammer 4 - Livre de base.pdf"
- Les fichiers `references/*.md` contiennent le texte brut extrait, avec des marqueurs `<!-- Page N -->` pour chaque page source
