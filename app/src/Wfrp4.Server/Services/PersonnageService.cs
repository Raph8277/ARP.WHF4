using Microsoft.EntityFrameworkCore;
using Wfrp4.Infrastructure.Data;
using Wfrp4.Infrastructure.Entities;
using Wfrp4.Shared.DTOs;
using Wfrp4.Shared.Models;

namespace Wfrp4.Server.Services;

public class PersonnageService
{
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

        // Compétences initiales (dépense XP)
        foreach (var (competenceId, avances) in request.CompetencesInitiales)
        {
            personnage.Competences.Add(new PersonnageCompetence
            {
                CompetenceId = competenceId,
                Avances = avances,
            });

            if (avances > 0)
            {
                var cout = _xpService.CalculerCoutCompetenceTotal(avances);
                personnage.XpDepense += cout;
                personnage.HistoriqueXP.Add(new HistoriqueXP
                {
                    AuteurKeycloakId = keycloakId,
                    Montant = -cout,
                    Type = TypeXP.Competence,
                    Cible = competenceId.ToString(),
                    Notes = $"Création — {avances} avance(s) initiale(s).",
                    CreatedAt = now,
                });
            }
        }

        // Talents initiaux (dépense XP — 100 XP chacun)
        foreach (var talentId in request.TalentsInitiaux)
        {
            personnage.Talents.Add(new PersonnageTalent
            {
                TalentId = talentId,
                Fois = 1,
            });

            var coutTalent = _xpService.CalculerCoutTalent(0);
            personnage.XpDepense += coutTalent;
            personnage.HistoriqueXP.Add(new HistoriqueXP
            {
                AuteurKeycloakId = keycloakId,
                Montant = -coutTalent,
                Type = TypeXP.Talent,
                Cible = talentId.ToString(),
                Notes = "Création — talent initial.",
                CreatedAt = now,
            });
        }

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

    public async Task<int> AvancerCaracteristique(int personnageId, string codeCarac)
    {
        var personnage = await _db.Personnages
            .Include(p => p.Caracteristiques)
            .FirstOrDefaultAsync(p => p.Id == personnageId)
            ?? throw new InvalidOperationException("Personnage introuvable.");

        var carac = personnage.Caracteristiques.FirstOrDefault(c => c.Code == codeCarac)
            ?? throw new InvalidOperationException($"Caractéristique '{codeCarac}' introuvable.");

        var cout = _xpService.CalculerCoutCaracteristique(carac.Avances);
        var xpRestant = personnage.XpTotal - personnage.XpDepense;
        if (cout > xpRestant)
            throw new InvalidOperationException($"XP insuffisant ({xpRestant} disponible, {cout} requis).");

        carac.Avances++;
        personnage.XpDepense += cout;
        personnage.UpdatedAt = DateTime.UtcNow;

        _db.HistoriqueXPs.Add(new HistoriqueXP
        {
            PersonnageId = personnageId,
            AuteurKeycloakId = personnage.KeycloakId,
            Montant = -cout,
            Type = TypeXP.Caracteristique,
            Cible = codeCarac,
            CreatedAt = DateTime.UtcNow,
        });

        await _db.SaveChangesAsync();
        return cout;
    }

    public async Task<int> AvancerCompetence(int personnageId, int competenceId)
    {
        var personnage = await _db.Personnages
            .Include(p => p.Competences)
            .FirstOrDefaultAsync(p => p.Id == personnageId)
            ?? throw new InvalidOperationException("Personnage introuvable.");

        var comp = personnage.Competences.FirstOrDefault(c => c.CompetenceId == competenceId);
        if (comp == null)
        {
            comp = new PersonnageCompetence
            {
                PersonnageId = personnageId,
                CompetenceId = competenceId,
                Avances = 0,
            };
            personnage.Competences.Add(comp);
        }

        var cout = _xpService.CalculerCoutCompetence(comp.Avances);
        var xpRestant = personnage.XpTotal - personnage.XpDepense;
        if (cout > xpRestant)
            throw new InvalidOperationException($"XP insuffisant ({xpRestant} disponible, {cout} requis).");

        comp.Avances++;
        personnage.XpDepense += cout;
        personnage.UpdatedAt = DateTime.UtcNow;

        _db.HistoriqueXPs.Add(new HistoriqueXP
        {
            PersonnageId = personnageId,
            AuteurKeycloakId = personnage.KeycloakId,
            Montant = -cout,
            Type = TypeXP.Competence,
            Cible = competenceId.ToString(),
            CreatedAt = DateTime.UtcNow,
        });

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
}
