# Specification - Edition de fiche de personnage PDF WFRP4

> Version : 0.1  
> Date : 9 aout 2026  
> Source visuelle : `docs/Warhammer 4 - Fiche de personnage.pdf`  
> Perimetre : generation, previsualisation et export d'une fiche personnage PDF a partir des donnees applicatives ARP.WHF4.

## 1. Constat technique sur la fiche source

La fiche PDF source contient 2 pages au format portrait, sans champ AcroForm et sans annotation interactive. Elle doit donc etre consideree comme un gabarit graphique statique.

Consequences :

- l'application ne peut pas remplir le PDF via des noms de champs PDF existants ;
- l'edition doit passer par une couche de superposition de texte et de marques cochees ;
- les coordonnees doivent etre gerees par l'application, idealement en coordonnees normalisees pour rester robustes si la resolution de rendu change ;
- le PDF source doit rester versionne comme modele immuable, et les PDFs generes doivent etre des exports derives.

## 2. Objectifs fonctionnels

Permettre a un utilisateur autorise de produire une fiche personnage PDF complete depuis l'application :

- previsualiser la fiche remplie depuis la page `FichePersonnage` ;
- exporter un PDF non interactif pret a imprimer ou partager ;
- conserver l'aspect de la fiche officielle fournie ;
- remplir automatiquement les champs connus depuis le modele applicatif ;
- laisser vides ou permettre la saisie manuelle des zones non encore modelisees dans l'application.

## 3. Hors perimetre initial

- creation d'un formulaire PDF interactif AcroForm ;
- edition libre de la mise en page du gabarit ;
- reconnaissance automatique par OCR des cases de la fiche ;
- signature numerique du PDF ;
- gestion complete des combats, blessures critiques, magie avancee et mutations si les donnees ne sont pas encore presentes dans le modele.

## 4. Strategie de generation PDF

### 4.1 Approche recommandee

Utiliser le PDF source comme fond de page, puis dessiner par-dessus :

1. ouvrir `docs/Warhammer 4 - Fiche de personnage.pdf` avec une librairie PDF ;
2. creer un overlay par page contenant textes, valeurs numeriques et eventuelles coches ;
3. fusionner chaque overlay avec la page source ;
4. produire un PDF final aplati.

Librairies .NET recommandees :

- **QuestPDF** : excellente pour creer des documents, moins adaptee pour ecrire par-dessus un PDF existant ;
- **PdfSharpCore** ou **PdfPig + SkiaSharp** : utile pour overlay, mais a valider selon support .NET 9 ;
- **iText 7** : techniquement solide pour overlay PDF, mais licence a verifier ;
- **API interne via Python/reportlab+pypdf** : possible pour MVP ou outil serveur separe, mais moins naturel dans une stack .NET pure.

Decision proposee pour l'application :

- MVP serveur : service `CharacterSheetPdfService` en .NET ;
- moteur PDF : choisir une librairie capable d'importer une page PDF comme fond et de dessiner texte/traits par coordonnees ;
- sortie : PDF aplati, `application/pdf`.

### 4.2 Coordonnees

La fiche source mesure environ `593.46 x 758.03` points PDF par page.

Definir toutes les zones en coordonnees normalisees :

```json
{
  "page": 1,
  "key": "identity.name",
  "x": 0.083,
  "y": 0.124,
  "w": 0.410,
  "h": 0.018,
  "align": "left",
  "fontSize": 9
}
```

Conversion :

- `pdfX = x * pageWidth`
- `pdfY = y * pageHeight`
- `pdfW = w * pageWidth`
- `pdfH = h * pageHeight`

Le systeme de coordonnees doit etre documente comme `origine bas-gauche` ou `origine haut-gauche`. Pour simplifier le mapping humain, utiliser dans les fichiers de template une origine haut-gauche, puis convertir vers le moteur PDF au rendu.

## 5. Donnees disponibles dans l'application

Source principale : `PersonnageDetailDto`.

| Zone fiche | Donnees disponibles | Statut |
|---|---|---|
| Nom | `Nom` | couvert |
| Race | `EspeceNom` | couvert, libelle fiche a conserver |
| Classe | `ClasseNom` | couvert |
| Carriere | `Carrieres[].CarriereNom` courante | couvert |
| Echelon | `CarriereCouranteIntitule` / `Niveau` | couvert |
| Statut | `StatutTier`, `StatutNumerique` | couvert |
| Age | `Age` | couvert |
| Taille | `TailleCm` | couvert |
| Cheveux | `CouleurCheveux` | couvert |
| Yeux | `CouleurYeux` | couvert |
| Caracteristiques initiales | `Caracteristiques[].ValeurInitiale` | couvert |
| Caracteristiques avancees | `Caracteristiques[].Avances` | couvert |
| Caracteristiques courantes | `Caracteristiques[].ValeurActuelle` | couvert |
| Destin | `Destin` | couvert |
| Chance | `Fortune` | couvert |
| Resistance | `Resilience` | couvert |
| Determination | `Resolution` | couvert |
| Motivation | `Motivation` | couvert |
| Experience actuelle | `XpRestant` | couvert |
| Experience depensee | `XpDepense` | couvert |
| Experience totale | `XpTotal` | couvert |
| Mouvement | `Mouvement` | couvert |
| Marche | derive de `Mouvement` | a calculer |
| Course | derive de `Mouvement` | a calculer |
| Competences de base | `Competences` + referentiel | partiel |
| Competences groupees/avancees | `Competences` + referentiel | partiel |
| Talents | `Talents` | couvert partiellement, description absente dans detail |
| Possessions | `Possessions` type `Objet` | couvert basique |
| Armes | `Possessions` type `Arme` + `ArmeReferenceDto` | partiel |
| Blessures | `BlessuresMax` et calculs BF/BE/BFM | partiel |
| Armure | non modelise comme equipement detaille | a ajouter |
| Points d'armure par localisation | non modelise | a ajouter |
| Richesses | non modelise | a ajouter |
| Encombrement | possessions basiques seulement | a enrichir |
| Psychologie | non modelise | a ajouter |
| Corruption et mutations | non modelise | a ajouter |
| Sorts et prieres | non modelise | a ajouter |
| Ambitions | non modelise | a ajouter |
| Groupe | non modelise | a ajouter |

## 6. Extensions de modele recommandees

### 6.1 Champs simples sur `Personnage`

Ajouter les champs suivants pour couvrir la page 1 et certains blocs narratifs :

```csharp
public string? AmbitionCourtTerme { get; set; }
public string? AmbitionLongTerme { get; set; }
public string? GroupeNom { get; set; }
public string? GroupeAmbitionCourtTerme { get; set; }
public string? GroupeAmbitionLongTerme { get; set; }
public string? GroupeMembres { get; set; }
public string? Psychologie { get; set; }
public string? CorruptionMutations { get; set; }
public int Peche { get; set; }
```

### 6.2 Richesses

Representer les richesses explicitement :

```csharp
public int SousCuivre { get; set; }
public int PistolesArgent { get; set; }
public int CouronnesOr { get; set; }
```

### 6.3 Armures

Ajouter une entite `PersonnageArmure` :

```csharp
public class PersonnageArmure
{
    public int Id { get; set; }
    public int PersonnageId { get; set; }
    public string Nom { get; set; } = null!;
    public string Localisation { get; set; } = null!;
    public int Encombrement { get; set; }
    public int PointsArmure { get; set; }
    public string? AtoutsDefauts { get; set; }
}
```

Ajouter un objet valeur calcule ou DTO :

```csharp
public class PointsArmureLocalisationsDto
{
    public int Tete { get; set; }
    public int BrasDroit { get; set; }
    public int BrasGauche { get; set; }
    public int Corps { get; set; }
    public int JambeDroite { get; set; }
    public int JambeGauche { get; set; }
    public int Bouclier { get; set; }
}
```

### 6.4 Armes equipees

Le modele actuel `PersonnagePossession` stocke seulement `Nom`, `Type`, `Quantite`. Pour remplir correctement la table Armes, il faut conserver une reference optionnelle vers `ArmeReference`.

Extension proposee :

```csharp
public int? ArmeReferenceId { get; set; }
public ArmeReference? ArmeReference { get; set; }
public bool EstEquipee { get; set; }
```

Le rendu PDF utilise alors :

- `Nom`
- `Groupe`
- `Encombrement`
- `Portee` ou `Longueur`
- `Dommage`
- concat `Qualites`, `Defauts`

### 6.5 Sorts et prieres

Ajouter une entite generique :

```csharp
public class PersonnageSortPriere
{
    public int Id { get; set; }
    public int PersonnageId { get; set; }
    public string Nom { get; set; } = null!;
    public int? NiveauLancement { get; set; }
    public string? Portee { get; set; }
    public string? Cible { get; set; }
    public string? Duree { get; set; }
    public string? Effets { get; set; }
    public bool EstPriere { get; set; }
}
```

## 7. Mapping visuel de la fiche

### 7.1 Page 1 - Identite et progression

| Bloc | Champs | Regles de rendu |
|---|---|---|
| Identite | nom, race, classe, carriere, echelon, schema de carriere, statut, age, taille, cheveux, yeux | texte simple, taille 8-10 pt, tronquer avec ellipsis si necessaire |
| Caracteristiques | CC, CT, F, E, I, Ag, Dex, Int, FM, Soc | 3 lignes : initiales, avancees, courantes |
| Destin | destin, chance | `Destin` et `Fortune` |
| Resistance | resistance, determination, motivation | `Resilience`, `Resolution`, `Motivation` |
| Experience | actuelle, depensee, totale | `XpRestant`, `XpDepense`, `XpTotal` |
| Mouvement | mouvement, marche, course | `Mouvement`, `Mouvement * 2`, `Mouvement * 4` par defaut |

### 7.2 Page 1 - Competences

La fiche separe :

- deux tables "Competences de base" pre-imprimees ;
- une table "Comp. groupees et augmentees" vide.

Regles :

- les competences dont le nom existe deja dans la table pre-imprimee remplissent uniquement les colonnes `Aug` et `Comp` ;
- `Comp = caracteristique courante associee + avances` ;
- les competences avancees ou groupees non pre-imprimees vont dans la table de droite ;
- si le nombre de lignes excede la place disponible, ajouter une page annexe optionnelle ou tronquer avec avertissement dans la previsualisation.

### 7.3 Page 1 - Talents, ambitions, groupe

| Bloc | Source | Regles |
|---|---|---|
| Talents | `Talents` | lignes `Nom`, `Fois`, `Description` si disponible |
| Ambitions | nouveaux champs `AmbitionCourtTerme`, `AmbitionLongTerme` | texte multi-ligne, max 2 lignes par champ |
| Groupe | nouveaux champs groupe | texte multi-ligne, max selon zone |

### 7.4 Page 2 - Equipement et etat

| Bloc | Source | Regles |
|---|---|---|
| Armure | `PersonnageArmure[]` | remplir 4 lignes max sur table principale |
| Points d'armure | calcul par localisation | dessiner valeurs dans les cases autour de la silhouette |
| Possessions | `Possessions` type `Objet` | liste nom + encombrement si connu |
| Psychologie | `Psychologie` | texte multi-ligne |
| Corruption et mutations | `CorruptionMutations` | texte multi-ligne |
| Richesses | `SousCuivre`, `PistolesArgent`, `CouronnesOr` | numerique |
| Encombrement | calcul arme/armure/possessions/max/total | numerique |
| Blessures | BF, BE x2, BFM, Dur a cuire, Blessures | calcul + `BlessuresMax` |
| Armes | armes equipees | 6 lignes max |
| Sorts et prieres | `PersonnageSortPriere[]` | 7 lignes max |
| Peche | `Peche` | numerique |

## 8. API cible

### 8.1 Export direct

```http
GET /api/personnages/{id}/fiche-pdf
Authorization: Bearer <token>
Accept: application/pdf
```

Reponse :

- `200 OK`, `Content-Type: application/pdf`
- `Content-Disposition: inline; filename="fiche-{nom-personnage}.pdf"`
- `403 Forbidden` si l'utilisateur n'a pas acces au personnage
- `404 Not Found` si le personnage n'existe pas

Autorisation :

- proprietaire : export complet ;
- MJ avec partage lecture ou XP : export autorise ;
- admin : export autorise.

### 8.2 Previsualisation

```http
GET /api/personnages/{id}/fiche-pdf/preview?page=1
Accept: image/png
```

Option utile pour l'UI :

- rendre une image PNG basse resolution pour previsualisation rapide ;
- eviter de telecharger le PDF complet a chaque modification.

### 8.3 Donnees manuelles

```http
PUT /api/personnages/{id}/fiche-pdf-data
Content-Type: application/json
```

Payload :

```json
{
  "ambitionCourtTerme": "...",
  "ambitionLongTerme": "...",
  "groupeNom": "...",
  "psychologie": "...",
  "corruptionMutations": "...",
  "peche": 0
}
```

Cette route peut etre fusionnee avec l'edition generale du personnage si l'application prefere un seul endpoint `PUT /api/personnages/{id}`.

## 9. UX cible

Depuis `FichePersonnage.razor` :

- bouton principal "Fiche PDF" ou "Editer la fiche PDF" dans la barre d'actions de la fiche personnage ;
- bouton "Exporter PDF" dans la barre d'actions ;
- option "Previsualiser la fiche" ouvrant un dialogue plein ecran ;
- onglet ou panneau "Fiche PDF" pour completer les champs narratifs non deduits ;
- avertissements non bloquants si certaines zones ne peuvent pas etre remplies :
  - aucune armure renseignee ;
  - armes sans reference detaillee ;
  - ambitions vides ;
  - sorts/prieres non modelises.

Le mode MJ partage doit afficher l'export mais pas l'edition des champs du proprietaire, sauf permission explicite future.

### 9.1 Point d'entree de l'edition PDF

L'edition de la fiche PDF doit etre lancable depuis au moins un point d'entree visible :

- **prioritaire** : bouton contextuel sur la page detail d'un personnage, dans le bloc d'actions principal, libelle "Fiche PDF" ou "Editer la fiche PDF" avec une icone PDF/document ;
- **optionnel** : entree de menu laterale "Fiches PDF" si un ecran global liste les exports ou modeles disponibles.

Comportement attendu du bouton :

- proprietaire avec droit de modification : ouvre l'edition des champs PDF et la previsualisation ;
- MJ avec partage lecture/XP : ouvre la previsualisation et l'export en lecture seule ;
- utilisateur sans droit : le bouton n'est pas affiche ou l'appel serveur retourne `403`.

Le bouton ne doit pas remplacer l'action existante d'edition du personnage. Les libelles doivent distinguer clairement :

- "Editer la fiche" : edition des donnees applicatives du personnage ;
- "Fiche PDF" / "Editer la fiche PDF" : preparation, previsualisation et export du document imprimable.

## 10. Service applicatif propose

```csharp
public interface ICharacterSheetPdfService
{
    Task<byte[]> GenerateAsync(int personnageId, ClaimsPrincipal user, CancellationToken ct);
    Task<byte[]> PreviewPageAsync(int personnageId, int page, ClaimsPrincipal user, CancellationToken ct);
}
```

Responsabilites :

- charger `Personnage` avec toutes les navigations requises ;
- verifier les droits via le meme modele que `PersonnageOwnerFilter` ;
- construire un `CharacterSheetViewModel` stable ;
- appliquer le mapping vers le template PDF ;
- retourner un PDF aplati.

## 11. Template de mapping

Stocker le mapping dans un fichier versionne :

`app/src/Wfrp4.Server/PdfTemplates/wfrp4-character-sheet.mapping.json`

Structure :

```json
{
  "template": "Warhammer 4 - Fiche de personnage.pdf",
  "pageSize": {
    "width": 593.4618,
    "height": 758.0332
  },
  "fields": [
    {
      "key": "identity.name",
      "source": "Nom",
      "page": 1,
      "x": 0.083,
      "y": 0.124,
      "w": 0.410,
      "h": 0.018,
      "fontSize": 9,
      "align": "left"
    }
  ]
}
```

Le mapping doit etre teste par rendu visuel. Les coordonnees de cette specification sont indicatives ; le calibrage final doit etre fait a partir de captures PNG de la fiche source.

## 12. Tests et validation

### 12.1 Tests unitaires

- transformation `PersonnageDetailDto` -> `CharacterSheetViewModel` ;
- calcul `Marche = Mouvement * 2` et `Course = Mouvement * 4` ;
- calcul `Comp` des competences ;
- calcul BF, BE, BFM ;
- formatage statut : `Bronze 3`, `Argent 1`, etc. ;
- troncature et retour ligne dans les zones texte.

### 12.2 Tests d'integration

- `GET /api/personnages/{id}/fiche-pdf` retourne un PDF pour le proprietaire ;
- retourne `403` pour un utilisateur non autorise ;
- retourne un PDF pour un MJ avec partage ;
- le PDF genere contient 2 pages ;
- le PDF genere est lisible par une librairie PDF ;
- aucun champ obligatoire connu ne provoque d'exception quand il est nul.

### 12.3 Validation visuelle

Pour chaque evolution significative :

1. generer un PDF pour un personnage de demonstration ;
2. rendre les pages en PNG via Poppler ;
3. verifier visuellement :
   - alignement dans les cases ;
   - absence de texte coupe ;
   - contraste suffisant ;
   - pas de chevauchement avec les traits du gabarit ;
   - lisibilite a l'impression.

## 13. Plan de realisation

### Phase 1 - Export automatique minimal

- ajouter le bouton contextuel "Fiche PDF" sur `FichePersonnage.razor` ;
- creer `CharacterSheetPdfService` ;
- ajouter route `GET /api/personnages/{id}/fiche-pdf` ;
- remplir page 1 avec identite, caracteristiques, XP, mouvement, competences, talents ;
- remplir page 2 avec possessions, armes basiques et blessures max ;
- ajouter tests d'integration d'autorisation et generation.

### Phase 2 - Donnees manuelles fiche

- ajouter ambitions, groupe, psychologie, corruption/mutations, richesses, peche ;
- exposer l'edition dans l'UI ;
- completer le mapping page 1/page 2.

### Phase 3 - Equipement detaille

- enrichir armes equipees avec reference ;
- ajouter armures et points d'armure par localisation ;
- calculer encombrement detaille ;
- remplir les blocs armure, PA et armes.

### Phase 4 - Magie et annexes

- ajouter sorts/prieres ;
- gerer les debordements par page annexe optionnelle ;
- ajouter une previsualisation PNG dans l'UI.

## 14. Criteres d'acceptation MVP

- Un proprietaire peut exporter sa fiche depuis l'application.
- La page detail d'un personnage expose un bouton visible permettant de lancer la preparation de la fiche PDF.
- Le PDF genere comporte 2 pages et conserve le visuel du gabarit source.
- Les zones suivantes sont remplies : nom, race, classe, carriere, echelon, statut, age, taille, cheveux, yeux, caracteristiques, destin/chance, resistance/determination/motivation, XP, mouvement, competences, talents, possessions, armes basiques, blessures.
- Un MJ peut exporter une fiche partagee en lecture.
- Un utilisateur sans droit ne peut pas exporter la fiche.
- Les tests serveur passent.
- Un rendu PNG de validation ne montre pas de texte manifestement decale, coupe ou illisible.
