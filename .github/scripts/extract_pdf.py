import fitz
import os
import re

PDF_PATH = r"G:\repos\ARP.WHF4\Warhammer 4 - Livre de base.pdf"
OUTPUT_BASE = r"G:\repos\ARP.WHF4\.github\skills"

# Chapter definitions: (skill_folder, filename, title, start_page, end_page)
# Pages are 0-indexed in PyMuPDF
CHAPTERS = [
    ("warhammer-regles", "introduction.md", "Introduction", 5, 23),
    ("warhammer-personnages", "creation-personnages.md", "Création de Personnages", 23, 45),
    ("warhammer-carrieres", "classes-carrieres.md", "Classes et Carrières", 45, 116),
    ("warhammer-competences", "competences-talents.md", "Compétences et Talents", 116, 147),
    ("warhammer-regles", "regles-base.md", "Règles de Base", 147, 171),
    ("warhammer-regles", "blessures-corruption.md", "Blessures, Corruption et Maladies", 171, 191),
    ("warhammer-regles", "entre-aventures.md", "Entre les Aventures", 191, 201),
    ("warhammer-lore", "religion-croyance.md", "Religion et Croyance", 201, 228),
    ("warhammer-magie", "magie.md", "Magie", 228, 257),
    ("warhammer-lore", "maitre-de-jeu.md", "Le Maître de Jeu", 257, 265),
    ("warhammer-lore", "reikland.md", "Le Reikland Glorieux", 265, 287),
    ("warhammer-equipement", "guide-consommateur.md", "Le Guide du Consommateur", 287, 309),
    ("warhammer-bestiaire", "bestiaire.md", "Bestiaire", 309, 343),
]

doc = fitz.open(PDF_PATH)
print(f"Opened PDF: {len(doc)} pages")

for skill_folder, filename, title, start_p, end_p in CHAPTERS:
    text_parts = []
    text_parts.append(f"# {title}\n")
    text_parts.append(f"*Pages {start_p+1} à {end_p} du Livre de Base*\n")
    
    for page_num in range(start_p, end_p):
        page = doc[page_num]
        text = page.get_text("text")
        if text.strip():
            text_parts.append(f"\n---\n<!-- Page {page_num+1} -->\n")
            text_parts.append(text)
    
    full_text = "\n".join(text_parts)
    
    out_dir = os.path.join(OUTPUT_BASE, skill_folder, "references")
    os.makedirs(out_dir, exist_ok=True)
    out_path = os.path.join(out_dir, filename)
    
    with open(out_path, "w", encoding="utf-8") as f:
        f.write(full_text)
    
    print(f"  {skill_folder}/references/{filename} ({end_p - start_p} pages, {len(full_text)} chars)")

doc.close()
print("Done!")
