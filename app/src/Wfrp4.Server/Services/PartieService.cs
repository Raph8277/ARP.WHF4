using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Wfrp4.Infrastructure.Data;
using Wfrp4.Infrastructure.Entities;
using Wfrp4.Shared.DTOs;
using Wfrp4.Shared.Models;

namespace Wfrp4.Server.Services;

/// <summary>
/// Cycle de vie des membres d'une partie et du partage XP qui donne au MJ l'accès au personnage choisi.
/// Un partage créé par le joueur (PartieId null) n'est jamais modifié ni supprimé ici.
/// </summary>
public class PartieService
{
    private readonly Wfrp4DbContext _db;

    public PartieService(Wfrp4DbContext db)
    {
        _db = db;
    }

    public static readonly Expression<Func<Partie, PartieSummaryDto>> VersResume = p => new PartieSummaryDto
    {
        Id = p.Id,
        Nom = p.Nom,
        Description = p.Description,
        Type = p.Type,
        Statut = p.Statut,
        MjKeycloakId = p.MjKeycloakId,
        MjNom = p.MjNom,
        NombreMembres = p.Membres.Count,
        NombrePersonnagesChoisis = p.Membres.Count(m => m.PersonnageId != null),
        UpdatedAt = p.UpdatedAt,
    };

    public static readonly Expression<Func<PartieMembre, ParticipationDto>> VersParticipation = m => new ParticipationDto
    {
        PartieId = m.PartieId,
        PartieNom = m.Partie.Nom,
        Description = m.Partie.Description,
        Type = m.Partie.Type,
        Statut = m.Partie.Statut,
        MjNom = m.Partie.MjNom,
        PersonnageId = m.PersonnageId,
        PersonnageNom = m.Personnage != null ? m.Personnage.Nom : null,
        AjouteLe = m.AjouteLe,
    };

    /// <summary>Change le personnage du membre et synchronise le partage ; n'enregistre pas.</summary>
    public async Task ChoisirPersonnageAsync(Partie partie, PartieMembre membre, int? personnageId)
    {
        if (membre.PersonnageId == personnageId)
            return;

        if (membre.PersonnageId is { } ancien)
            await DetacherPartageAsync(partie, ancien, membre.Id);

        membre.PersonnageId = personnageId;
        if (personnageId is { } nouveau)
            await AttacherPartageAsync(partie, nouveau);

        partie.UpdatedAt = DateTime.UtcNow;
    }

    public async Task RetirerMembreAsync(Partie partie, PartieMembre membre)
    {
        if (membre.PersonnageId is { } personnageId)
            await DetacherPartageAsync(partie, personnageId, membre.Id);

        _db.PartieMembres.Remove(membre);
        partie.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    public async Task SupprimerPartieAsync(Partie partie)
    {
        var membres = await _db.PartieMembres.Where(m => m.PartieId == partie.Id).ToListAsync();
        foreach (var membre in membres.Where(m => m.PersonnageId is not null))
            await DetacherPartageAsync(partie, membre.PersonnageId!.Value, membre.Id);

        _db.PartieMembres.RemoveRange(membres);
        _db.Parties.Remove(partie);
        await _db.SaveChangesAsync();
    }

    private async Task AttacherPartageAsync(Partie partie, int personnageId)
    {
        var existe = await _db.PersonnagePartages
            .AnyAsync(pp => pp.PersonnageId == personnageId && pp.MjKeycloakId == partie.MjKeycloakId);
        if (existe)
            return;

        _db.PersonnagePartages.Add(new PersonnagePartage
        {
            PersonnageId = personnageId,
            MjKeycloakId = partie.MjKeycloakId,
            Permission = PermissionPartage.XP,
            CreatedAt = DateTime.UtcNow,
            PartieId = partie.Id,
        });
    }

    /// <summary>
    /// Retire le partage créé par cette partie ; s'il sert encore à une autre partie du même MJ pour ce
    /// personnage, il y est rattaché au lieu d'être supprimé.
    /// </summary>
    private async Task DetacherPartageAsync(Partie partie, int personnageId, int membreSortantId)
    {
        var partage = await _db.PersonnagePartages
            .FirstOrDefaultAsync(pp => pp.PersonnageId == personnageId && pp.MjKeycloakId == partie.MjKeycloakId);
        if (partage is null || partage.PartieId != partie.Id)
            return;

        var autrePartieId = await _db.PartieMembres
            .Where(m => m.PersonnageId == personnageId
                && m.Id != membreSortantId
                && m.PartieId != partie.Id
                && m.Partie.MjKeycloakId == partie.MjKeycloakId)
            .Select(m => (int?)m.PartieId)
            .FirstOrDefaultAsync();

        if (autrePartieId is not null)
            partage.PartieId = autrePartieId;
        else
            _db.PersonnagePartages.Remove(partage);
    }
}
