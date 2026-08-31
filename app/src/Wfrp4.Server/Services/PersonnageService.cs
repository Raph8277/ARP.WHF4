using Microsoft.EntityFrameworkCore;
using Wfrp4.Infrastructure.Data;
using Wfrp4.Infrastructure.Entities;
using Wfrp4.Shared.DTOs;
using Wfrp4.Shared.Models;

namespace Wfrp4.Server.Services;

public class PersonnageService
{
    private const int AvancesCompetenceGratuitesCreation = 40;
    private const int TalentsGratuitsCreation = 1;

    private readonly Wfrp4DbContext _db;
    private readonly XPService _xpService;

    public PersonnageService(Wfrp4DbContext db, XPService xpService)
    {
        _db = db;
        _xpService = xpService;
    }

    public async Task<Personnage> CreerPersonnage(string keycloakId, CreatePersonnageRequest request)
    {
        var espece = await _db.Especes.FindAsync(request.EspeceId)
            ?? throw new InvalidOperationException("Espèce introuvable.");

        var niveauCarriere = await _db.NiveauCarrieres
            .Include(n => n.Carriere)
            .FirstOrDefaultAsync(n => n.Id == request.NiveauCarriereId)
            ?? throw new InvalidOperationException("Niveau de carrière introuvable.");

        var competenceIdsCarriere = await GetCompetenceIdsCarriere(niveauCarriere);
        var talentIdsCarriere = await GetTalentIdsCarriere(niveauCarriere);
        ValiderChoixCreation(request, competenceIdsCarriere, talentIdsCarriere);

        if (request.TitreBaseReferenceId.HasValue != request.TitreQualificatifReferenceId.HasValue)
            throw new InvalidOperationException("Le titre doit comporter une base et un qualificatif, ou rester vide.");

        if (request.TitreBaseReferenceId.HasValue)
        {
            var titreBaseValide = await _db.TitresBaseReference
                .AnyAsync(t => t.Id == request.TitreBaseReferenceId.Value && t.NiveauMaitrise <= niveauCarriere.Niveau);
            if (!titreBaseValide)
                throw new InvalidOperationException("Titre de base inaccessible pour le niveau de maîtrise actuel.");
        }

        if (request.TitreQualificatifReferenceId.HasValue)
        {
            var titreQualificatifValide = await _db.TitresQualificatifReference
                .AnyAsync(t => t.Id == request.TitreQualificatifReferenceId.Value && t.NiveauMaitrise <= niveauCarriere.Niveau);
            if (!titreQualificatifValide)
                throw new InvalidOperationException("Qualificatif de titre inaccessible pour le niveau de maîtrise actuel.");
        }

        var now = DateTime.UtcNow;
        var personnage = new Personnage
        {
            KeycloakId = keycloakId,
            Nom = request.Nom,
            Genre = request.Genre,
            EspeceId = request.EspeceId,
            CarriereCouranteId = niveauCarriere.Id,
            TitreBaseReferenceId = request.TitreBaseReferenceId,
            TitreQualificatifReferenceId = request.TitreQualificatifReferenceId,
            Motivation = request.Motivation,
            Age = request.Age,
            CouleurYeux = request.CouleurYeux,
            CouleurCheveux = request.CouleurCheveux,
            TailleCm = request.TailleCm,
            Mouvement = espece.MouvementBase,
            AvancesCompetenceGratuitesRestantes = AvancesCompetenceGratuitesCreation,
            TalentsGratuitsRestants = TalentsGratuitsCreation,
            CreatedAt = now,
            UpdatedAt = now,
        };

        // Richesse de départ (Livre de Base p. 36)
        GenererRichesseDeDepart(personnage, niveauCarriere);

        // Caractéristiques initiales
        foreach (var (code, valeur) in request.CaracteristiquesInitiales)
        {
            personnage.Caracteristiques.Add(new PersonnageCaracteristique
            {
                Code = code,
                ValeurInitiale = valeur,
                Avances = 0
            });
        }

        // Calculer attributs dérivés (Livre de Base p. 34)
        CalculerAttributsDerives(personnage, espece);

        // Première carrière
        personnage.Carrieres.Add(new PersonnageCarriere
        {
            NiveauCarriereId = niveauCarriere.Id,
            EstCourante = true,
            DateEntree = now,
        });

        // Bonus XP tirage aléatoire
        if (request.XpBonus > 0)
        {
            personnage.XpTotal = request.XpBonus;
            personnage.HistoriqueXP.Add(new HistoriqueXP
            {
                AuteurKeycloakId = keycloakId,
                Montant = request.XpBonus,
                Type = TypeXP.Gain,
                Notes = "Bonus XP création — tirage aléatoire des caractéristiques accepté.",
                CreatedAt = now,
            });
        }

        // Compétences initiales de carrière : 40 avances gratuites à la création.
        foreach (var (competenceId, avances) in request.CompetencesInitiales)
        {
            personnage.Competences.Add(new PersonnageCompetence
            {
                CompetenceId = competenceId,
                Avances = avances,
            });
            personnage.AvancesCompetenceGratuitesRestantes -= avances;
        }

        // Talent initial de carrière : 1 choix gratuit à la création.
        foreach (var talentId in request.TalentsInitiaux)
        {
            personnage.Talents.Add(new PersonnageTalent
            {
                TalentId = talentId,
                Fois = 1,
            });
            personnage.TalentsGratuitsRestants--;
        }

        await AppliquerPlancherCompetencesDeBase(personnage);

        _db.Personnages.Add(personnage);
        await _db.SaveChangesAsync();
        return personnage;
    }

    public async Task<Personnage> MettreAJourPersonnage(int personnageId, UpdatePersonnageRequest request)
    {
        var personnage = await _db.Personnages
            .Include(p => p.Espece)
            .Include(p => p.Caracteristiques)
            .FirstOrDefaultAsync(p => p.Id == personnageId)
            ?? throw new InvalidOperationException("Personnage introuvable.");

        personnage.Nom = request.Nom;
        personnage.Genre = request.Genre;
        personnage.Motivation = request.Motivation;
        personnage.TitreBaseReferenceId = request.TitreBaseReferenceId;
        personnage.TitreQualificatifReferenceId = request.TitreQualificatifReferenceId;
        personnage.Age = request.Age;
        personnage.CouleurYeux = request.CouleurYeux;
        personnage.CouleurCheveux = request.CouleurCheveux;
        personnage.TailleCm = request.TailleCm;
        personnage.EstActif = request.EstActif;
        personnage.CouronnesOr = request.CouronnesOr;
        personnage.PistolesArgent = request.PistolesArgent;
        personnage.SousCuivre = request.SousCuivre;
        personnage.AmbitionCourtTerme = request.AmbitionCourtTerme;
        personnage.AmbitionLongTerme = request.AmbitionLongTerme;
        personnage.GroupeNom = request.GroupeNom;
        personnage.GroupeMembres = request.GroupeMembres;
        personnage.Psychologie = request.Psychologie;
        personnage.CorruptionMutations = request.CorruptionMutations;
        personnage.UpdatedAt = DateTime.UtcNow;

        CalculerAttributsDerives(personnage, personnage.Espece);

        await _db.SaveChangesAsync();
        return personnage;
    }

    public void CalculerAttributsDerives(Personnage personnage, Espece espece)
    {
        var caracs = personnage.Caracteristiques.ToDictionary(c => c.Code);

        int GetCarac(string code) =>
            caracs.TryGetValue(code, out var c) ? c.ValeurInitiale + c.Avances : 0;

        var force = GetCarac("F");
        var endurance = GetCarac("E");
        var volonte = GetCarac("FM");

        // BlessuresMax = Bonus de F + 2 × Bonus de E + Bonus de FM (p. 34)
        personnage.BlessuresMax = (force / 10) + 2 * (endurance / 10) + (volonte / 10);
    }

    public async Task<int> AvancerCaracteristique(int personnageId, string codeCarac, int nombrePoints = 1)
    {
        nombrePoints = Math.Clamp(nombrePoints, -50, 50);
        if (nombrePoints == 0)
            throw new InvalidOperationException("Aucune avance à appliquer.");

        var personnage = await _db.Personnages
            .Include(p => p.Caracteristiques)
            .FirstOrDefaultAsync(p => p.Id == personnageId)
            ?? throw new InvalidOperationException("Personnage introuvable.");

        var carac = personnage.Caracteristiques.FirstOrDefault(c => c.Code == codeCarac)
            ?? throw new InvalidOperationException($"Caractéristique '{codeCarac}' introuvable.");

        if (carac.Avances + nombrePoints > 50)
            throw new InvalidOperationException("Impossible de dépasser 50 avances.");
        if (carac.Avances + nombrePoints < 0)
            throw new InvalidOperationException("Impossible de réduire une caractéristique sous 0 avance.");

        var coutCalcule = _xpService.CalculerCoutCaracteristiqueTotal(carac.Avances, nombrePoints);
        var cout = PlafonnerRemboursementXp(coutCalcule, personnage.XpDepense);
        carac.Avances += nombrePoints;
        personnage.XpDepense += cout;
        personnage.UpdatedAt = DateTime.UtcNow;

        _db.HistoriqueXPs.Add(new HistoriqueXP
        {
            PersonnageId = personnageId,
            AuteurKeycloakId = personnage.KeycloakId,
            Montant = -cout,
            Type = TypeXP.Caracteristique,
            Cible = nombrePoints == 1 ? codeCarac : $"{codeCarac} ({nombrePoints:+#;-#;0})",
            CreatedAt = DateTime.UtcNow,
        });

        await _db.SaveChangesAsync();
        return cout;
    }

    public async Task<int> AvancerCompetence(int personnageId, int competenceId, int nombrePoints = 1)
    {
        nombrePoints = Math.Clamp(nombrePoints, -50, 50);
        if (nombrePoints == 0)
            throw new InvalidOperationException("Aucune avance à appliquer.");

        var personnage = await _db.Personnages
            .Include(p => p.Competences)
            .FirstOrDefaultAsync(p => p.Id == personnageId)
            ?? throw new InvalidOperationException("Personnage introuvable.");

        var competence = await _db.Competences.FindAsync(competenceId)
            ?? throw new InvalidOperationException("Compétence introuvable.");
        var minimumAvances = competence.EstAvancee ? 0 : 5;

        var comp = personnage.Competences.FirstOrDefault(c => c.CompetenceId == competenceId);
        if (comp == null)
        {
            comp = new PersonnageCompetence
            {
                PersonnageId = personnageId,
                CompetenceId = competenceId,
                Avances = minimumAvances,
            };
            personnage.Competences.Add(comp);
        }
        else if (comp.Avances < minimumAvances)
        {
            comp.Avances = minimumAvances;
        }

        if (comp.Avances + nombrePoints > 50)
            throw new InvalidOperationException("Impossible de dépasser 50 avances.");
        if (comp.Avances + nombrePoints < minimumAvances)
            throw new InvalidOperationException(competence.EstAvancee
                ? "Impossible de réduire une compétence sous 0 avance."
                : "Impossible de réduire une compétence de base sous 5 avances.");

        var avancesGratuites = nombrePoints > 0
            ? Math.Min(nombrePoints, personnage.AvancesCompetenceGratuitesRestantes)
            : 0;
        var pointsPayants = nombrePoints - avancesGratuites;
        var coutCalcule = _xpService.CalculerCoutCompetenceTotal(comp.Avances + avancesGratuites, pointsPayants);
        var cout = PlafonnerRemboursementXp(coutCalcule, personnage.XpDepense);
        comp.Avances += nombrePoints;
        personnage.AvancesCompetenceGratuitesRestantes -= avancesGratuites;
        personnage.XpDepense += cout;
        personnage.UpdatedAt = DateTime.UtcNow;

        if (cout != 0)
        {
            _db.HistoriqueXPs.Add(new HistoriqueXP
            {
                PersonnageId = personnageId,
                AuteurKeycloakId = personnage.KeycloakId,
                Montant = -cout,
                Type = TypeXP.Competence,
                Cible = nombrePoints == 1 ? competenceId.ToString() : $"{competenceId} ({nombrePoints:+#;-#;0})",
                CreatedAt = DateTime.UtcNow,
            });
        }

        await _db.SaveChangesAsync();
        return cout;
    }

    private async Task AppliquerPlancherCompetencesDeBase(Personnage personnage)
    {
        var competenceIdsDeBase = await _db.Competences
            .Where(c => !c.EstAvancee)
            .Select(c => c.Id)
            .ToListAsync();

        foreach (var competenceId in competenceIdsDeBase)
        {
            var competencePersonnage = personnage.Competences.FirstOrDefault(c => c.CompetenceId == competenceId);
            if (competencePersonnage == null)
            {
                personnage.Competences.Add(new PersonnageCompetence
                {
                    CompetenceId = competenceId,
                    Avances = 5,
                });
            }
            else if (competencePersonnage.Avances < 5)
            {
                competencePersonnage.Avances = 5;
            }
        }
    }

    public async Task<int> AvancerTalent(int personnageId, int talentId, int nombreFois = 1)
    {
        nombreFois = Math.Clamp(nombreFois, -50, 50);
        if (nombreFois == 0)
            throw new InvalidOperationException("Aucun talent à appliquer.");

        var personnage = await _db.Personnages
            .Include(p => p.Talents)
            .FirstOrDefaultAsync(p => p.Id == personnageId)
            ?? throw new InvalidOperationException("Personnage introuvable.");

        var talent = await _db.Talents.FindAsync(talentId)
            ?? throw new InvalidOperationException("Talent introuvable.");

        var existant = personnage.Talents.FirstOrDefault(t => t.TalentId == talentId);
        var foisActuelles = existant?.Fois ?? 0;
        if (foisActuelles + nombreFois < 0)
            throw new InvalidOperationException("Impossible de réduire un talent sous 0.");
        if (nombreFois > 0 && talent.MaxFois.HasValue && foisActuelles + nombreFois > talent.MaxFois.Value)
            throw new InvalidOperationException("Impossible de dépasser le maximum de ce talent.");

        var talentsGratuits = nombreFois > 0
            ? Math.Min(nombreFois, personnage.TalentsGratuitsRestants)
            : 0;
        var foisPayantes = nombreFois - talentsGratuits;
        var coutCalcule = _xpService.CalculerCoutTalentTotal(foisActuelles + talentsGratuits, foisPayantes);
        var cout = PlafonnerRemboursementXp(coutCalcule, personnage.XpDepense);

        if (existant != null)
        {
            existant.Fois += nombreFois;
            if (existant.Fois == 0)
                personnage.Talents.Remove(existant);
        }
        else
        {
            personnage.Talents.Add(new PersonnageTalent
            {
                PersonnageId = personnageId,
                TalentId = talentId,
                Fois = nombreFois,
            });
        }

        personnage.TalentsGratuitsRestants -= talentsGratuits;
        personnage.XpDepense += cout;
        personnage.UpdatedAt = DateTime.UtcNow;

        if (cout != 0)
        {
            _db.HistoriqueXPs.Add(new HistoriqueXP
            {
                PersonnageId = personnageId,
                AuteurKeycloakId = personnage.KeycloakId,
                Montant = -cout,
                Type = TypeXP.Talent,
                Cible = nombreFois == 1 ? talentId.ToString() : $"{talentId} ({nombreFois:+#;-#;0})",
                CreatedAt = DateTime.UtcNow,
            });
        }

        await _db.SaveChangesAsync();
        return cout;
    }

    public async Task OctroyerXP(int personnageId, string mjKeycloakId, XPGrantRequest request)
    {
        var personnage = await _db.Personnages.FindAsync(personnageId)
            ?? throw new InvalidOperationException("Personnage introuvable.");

        personnage.XpTotal += request.Montant;
        personnage.UpdatedAt = DateTime.UtcNow;

        _db.HistoriqueXPs.Add(new HistoriqueXP
        {
            PersonnageId = personnageId,
            AuteurKeycloakId = mjKeycloakId,
            Montant = request.Montant,
            Type = TypeXP.Gain,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow,
        });

        await _db.SaveChangesAsync();
    }

    private static int PlafonnerRemboursementXp(int cout, int xpDepense) =>
        cout < 0 ? Math.Max(cout, -xpDepense) : cout;

    private static void GenererRichesseDeDepart(Personnage personnage, NiveauCarriere niveau)
    {
        var rng = Random.Shared;
        var rang = niveau.StatutNumerique;

        switch (niveau.Statut)
        {
            case StatutTier.Bronze:
                var nbDes = 2 * rang;
                var totalCuivre = 0;
                for (var i = 0; i < nbDes; i++)
                    totalCuivre += rng.Next(1, 11);
                personnage.SousCuivre = totalCuivre;
                break;

            case StatutTier.Argent:
                var totalArgent = 0;
                for (var i = 0; i < rang; i++)
                    totalArgent += rng.Next(1, 11);
                personnage.PistolesArgent = totalArgent;
                break;

            case StatutTier.Or:
                personnage.CouronnesOr = rang;
                break;
        }
    }

    private async Task<HashSet<int>> GetCompetenceIdsCarriere(NiveauCarriere niveauCarriere)
    {
        var codes = ParseCodes(niveauCarriere.CompetenceRevenu);
        if (codes.Count == 0) return [];

        return (await _db.Competences
                .Where(c => codes.Contains(c.Code))
                .Select(c => c.Id)
                .ToListAsync())
            .ToHashSet();
    }

    private async Task<HashSet<int>> GetTalentIdsCarriere(NiveauCarriere niveauCarriere)
    {
        var codes = ParseCodes(niveauCarriere.TalentsRevenu);
        if (codes.Count == 0) return [];

        return (await _db.Talents
                .Where(t => codes.Contains(t.Code))
                .Select(t => t.Id)
                .ToListAsync())
            .ToHashSet();
    }

    private static HashSet<string> ParseCodes(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return [];
        return raw.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    private static void ValiderChoixCreation(
        CreatePersonnageRequest request,
        HashSet<int> competenceIdsCarriere,
        HashSet<int> talentIdsCarriere)
    {
        var totalAvances = request.CompetencesInitiales.Values.Sum();
        if (competenceIdsCarriere.Count > 0)
        {
            if (!request.CompetencesInitiales.Keys.ToHashSet().SetEquals(competenceIdsCarriere))
                throw new InvalidOperationException("Les 8 compétences de carrière doivent être présentes à la création.");
            if (totalAvances != AvancesCompetenceGratuitesCreation)
                throw new InvalidOperationException("Répartissez exactement 40 avances gratuites de compétence.");
        }
        else if (request.CompetencesInitiales.Count > 0)
        {
            throw new InvalidOperationException("Aucune compétence de carrière n'est disponible pour cette création.");
        }

        if (request.CompetencesInitiales.Values.Any(v => v < 0 || v > 10))
            throw new InvalidOperationException("Chaque compétence de carrière doit recevoir entre 0 et 10 avances.");

        if (talentIdsCarriere.Count > 0)
        {
            if (request.TalentsInitiaux.Count != TalentsGratuitsCreation)
                throw new InvalidOperationException("Choisissez exactement 1 talent de carrière gratuit.");
            if (request.TalentsInitiaux.Any(t => !talentIdsCarriere.Contains(t)))
                throw new InvalidOperationException("Le talent choisi doit appartenir aux talents de carrière.");
        }
        else if (request.TalentsInitiaux.Count > 0)
        {
            throw new InvalidOperationException("Aucun talent de carrière n'est disponible pour cette création.");
        }
    }
}
