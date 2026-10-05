using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wfrp4.Infrastructure.Data;
using Wfrp4.Infrastructure.Entities;
using Wfrp4.Server.Services;
using Wfrp4.Shared.DTOs;

namespace Wfrp4.Server.Controllers;

/// <summary>
/// Parties (aventures, campagnes) menées par un MJ. Seul le MJ propriétaire, ou un admin, gère une
/// partie ; l'accès du MJ au personnage choisi passe par un partage XP rattaché à la partie.
/// </summary>
[ApiController]
[Route("api/parties")]
[Authorize(Policy = "MaitreJeu")]
public class PartiesController : ControllerBase
{
    public const int MaxMembres = 12;
    private const int MaxResultatsRecherche = 100;

    private readonly Wfrp4DbContext _db;
    private readonly PartieService _parties;
    private readonly IKeycloakUtilisateursAdmin _keycloak;
    private readonly ILogger<PartiesController> _logger;

    public PartiesController(Wfrp4DbContext db, PartieService parties, IKeycloakUtilisateursAdmin keycloak, ILogger<PartiesController> logger)
    {
        _db = db;
        _parties = parties;
        _keycloak = keycloak;
        _logger = logger;
    }

    private string GetKeycloakId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new UnauthorizedAccessException();

    private bool EstAdmin => User.IsInRole(RolesApplicatifs.Admin);

    private string NomAppelant =>
        User.FindFirstValue("preferred_username") ?? User.Identity?.Name ?? GetKeycloakId();

    // --- Parties ---

    [HttpGet]
    public async Task<ActionResult<List<PartieSummaryDto>>> Lister()
    {
        var keycloakId = GetKeycloakId();
        var query = _db.Parties.AsNoTracking();
        if (!EstAdmin)
            query = query.Where(p => p.MjKeycloakId == keycloakId);

        return await query
            .OrderByDescending(p => p.UpdatedAt)
            .Select(PartieService.VersResume)
            .ToListAsync();
    }

    [HttpPost]
    public async Task<ActionResult<PartieDetailDto>> Creer([FromBody] EnregistrerPartieRequest request)
    {
        var erreur = Valider(request);
        if (erreur is not null)
            return BadRequest(new { Error = erreur });

        var maintenant = DateTime.UtcNow;
        var partie = new Partie
        {
            Nom = request.Nom.Trim(),
            Description = Normaliser(request.Description),
            Type = request.Type,
            Statut = request.Statut,
            MjKeycloakId = GetKeycloakId(),
            MjNom = Tronquer(NomAppelant, 100),
            CreatedAt = maintenant,
            UpdatedAt = maintenant,
        };
        _db.Parties.Add(partie);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(Obtenir), new { id = partie.Id }, await ChargerDetailAsync(partie.Id));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PartieDetailDto>> Obtenir(int id)
    {
        var (partie, refus) = await ChargerPartieGereeAsync(id);
        if (partie is null)
            return refus!;

        return await ChargerDetailAsync(partie.Id);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<PartieDetailDto>> Modifier(int id, [FromBody] EnregistrerPartieRequest request)
    {
        var erreur = Valider(request);
        if (erreur is not null)
            return BadRequest(new { Error = erreur });

        var (partie, refus) = await ChargerPartieGereeAsync(id);
        if (partie is null)
            return refus!;

        partie.Nom = request.Nom.Trim();
        partie.Description = Normaliser(request.Description);
        partie.Type = request.Type;
        partie.Statut = request.Statut;
        partie.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return await ChargerDetailAsync(partie.Id);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Supprimer(int id)
    {
        var (partie, refus) = await ChargerPartieGereeAsync(id);
        if (partie is null)
            return refus!;

        await _parties.SupprimerPartieAsync(partie);
        _logger.LogInformation("Partie {PartieId} supprimée par {Auteur}.", id, GetKeycloakId());
        return NoContent();
    }

    // --- Membres ---

    [HttpGet("joueurs")]
    public async Task<ActionResult<List<UtilisateurResumeDto>>> RechercherJoueurs([FromQuery] string? recherche, CancellationToken ct)
    {
        if (recherche?.Length > 100)
            return BadRequest(new { Error = "Recherche trop longue." });

        try
        {
            // Recherche vide : liste de tous les joueurs actifs, pour choisir sans connaître le nom.
            var moi = GetKeycloakId();
            var joueurs = await _keycloak.RechercherJoueursAsync(recherche, MaxResultatsRecherche + 1, ct);
            return joueurs.Where(j => j.Id != moi).Take(MaxResultatsRecherche).ToList();
        }
        catch (KeycloakIndisponibleException ex)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { Error = ex.Message });
        }
    }

    [HttpPost("{id:int}/membres")]
    public async Task<ActionResult<PartieDetailDto>> AjouterMembre(int id, [FromBody] AjouterMembreRequest request, CancellationToken ct)
    {
        var (partie, refus) = await ChargerPartieGereeAsync(id);
        if (partie is null)
            return refus!;

        var joueurId = request?.JoueurKeycloakId?.Trim();
        if (string.IsNullOrEmpty(joueurId))
            return BadRequest(new { Error = "Joueur requis." });
        if (joueurId == partie.MjKeycloakId)
            return BadRequest(new { Error = "Le MJ ne peut pas être joueur de sa propre partie." });
        if (await _db.PartieMembres.AnyAsync(m => m.PartieId == id && m.JoueurKeycloakId == joueurId, ct))
            return Conflict(new { Error = "Ce joueur fait déjà partie de la partie." });
        if (await _db.PartieMembres.CountAsync(m => m.PartieId == id, ct) >= MaxMembres)
            return Conflict(new { Error = $"Une partie compte au plus {MaxMembres} joueurs." });

        // L'identifiant reçu ne prouve rien : le compte doit exister, être actif et joueur.
        UtilisateurAdminDto? compte;
        try
        {
            compte = await _keycloak.ObtenirAsync(joueurId, ct);
        }
        catch (KeycloakIndisponibleException ex)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { Error = ex.Message });
        }

        if (compte is null || !compte.Actif || !(compte.Roles.Contains(RolesApplicatifs.Joueur) || compte.SuperAdmin))
            return BadRequest(new { Error = "Ce compte n'est pas un joueur actif." });

        _db.PartieMembres.Add(new PartieMembre
        {
            PartieId = id,
            JoueurKeycloakId = compte.Id,
            JoueurNom = Tronquer(compte.NomUtilisateur, 100),
            AjouteLe = DateTime.UtcNow,
        });
        partie.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Partie {PartieId} : joueur {Joueur} ajouté par {Auteur}.", id, compte.Id, GetKeycloakId());
        return await ChargerDetailAsync(id);
    }

    [HttpDelete("{id:int}/membres/{membreId:int}")]
    public async Task<IActionResult> RetirerMembre(int id, int membreId)
    {
        var (partie, refus) = await ChargerPartieGereeAsync(id);
        if (partie is null)
            return refus!;

        var membre = await _db.PartieMembres.FirstOrDefaultAsync(m => m.Id == membreId && m.PartieId == id);
        if (membre is null)
            return NotFound();

        await _parties.RetirerMembreAsync(partie, membre);
        _logger.LogInformation("Partie {PartieId} : joueur {Joueur} retiré par {Auteur}.", id, membre.JoueurKeycloakId, GetKeycloakId());
        return NoContent();
    }

    [HttpGet("{id:int}/membres/{membreId:int}/personnages")]
    public async Task<ActionResult<List<PersonnageSummaryDto>>> PersonnagesDuMembre(int id, int membreId)
    {
        var (partie, refus) = await ChargerPartieGereeAsync(id);
        if (partie is null)
            return refus!;

        var membre = await _db.PartieMembres.AsNoTracking().FirstOrDefaultAsync(m => m.Id == membreId && m.PartieId == id);
        if (membre is null)
            return NotFound();

        // Résumé seulement, et uniquement pour un membre de la partie gérée.
        return await _db.Personnages
            .AsNoTracking()
            .Where(p => p.KeycloakId == membre.JoueurKeycloakId && p.EstActif)
            .OrderBy(p => p.Nom)
            .Select(p => new PersonnageSummaryDto
            {
                Id = p.Id,
                Nom = p.Nom,
                Genre = p.Genre,
                EspeceNom = p.Espece.Nom,
                CarriereCouranteIntitule = p.CarriereCourante != null ? p.CarriereCourante.Intitule : null,
                XpTotal = p.XpTotal,
                XpDepense = p.XpDepense,
                EstActif = p.EstActif,
                CreatedAt = p.CreatedAt,
            })
            .ToListAsync();
    }

    [HttpPut("{id:int}/membres/{membreId:int}/personnage")]
    public async Task<ActionResult<PartieDetailDto>> ChoisirPersonnage(int id, int membreId, [FromBody] ChoisirPersonnageRequest request)
    {
        var (partie, refus) = await ChargerPartieGereeAsync(id);
        if (partie is null)
            return refus!;

        var membre = await _db.PartieMembres.FirstOrDefaultAsync(m => m.Id == membreId && m.PartieId == id);
        if (membre is null)
            return NotFound();

        var personnageId = request?.PersonnageId;
        if (personnageId is not null)
        {
            // Le personnage doit appartenir au joueur membre : jamais déduit du DTO.
            var valide = await _db.Personnages.AnyAsync(p => p.Id == personnageId && p.KeycloakId == membre.JoueurKeycloakId && p.EstActif);
            if (!valide)
                return BadRequest(new { Error = "Ce personnage n'appartient pas à ce joueur." });
        }

        await _parties.ChoisirPersonnageAsync(partie, membre, personnageId);
        await _db.SaveChangesAsync();

        _logger.LogInformation(
            "Partie {PartieId} : personnage {PersonnageId} choisi pour le membre {MembreId} par {Auteur}.",
            id, personnageId, membreId, GetKeycloakId());
        return await ChargerDetailAsync(id);
    }

    // --- Interne ---

    /// <summary>Renvoie la partie si l'appelant en est le MJ ou est admin ; sinon 404 ou 403.</summary>
    private async Task<(Partie? Partie, ActionResult? Refus)> ChargerPartieGereeAsync(int id)
    {
        var partie = await _db.Parties.FirstOrDefaultAsync(p => p.Id == id);
        if (partie is null)
            return (null, NotFound());
        if (partie.MjKeycloakId != GetKeycloakId() && !EstAdmin)
            return (null, Forbid());
        return (partie, null);
    }

    private async Task<PartieDetailDto> ChargerDetailAsync(int id)
    {
        var resume = await _db.Parties.AsNoTracking().Where(p => p.Id == id).Select(PartieService.VersResume).FirstAsync();
        var membres = await _db.PartieMembres
            .AsNoTracking()
            .Where(m => m.PartieId == id)
            .OrderBy(m => m.JoueurNom)
            .Select(m => new PartieMembreDto
            {
                Id = m.Id,
                JoueurKeycloakId = m.JoueurKeycloakId,
                JoueurNom = m.JoueurNom,
                PersonnageId = m.PersonnageId,
                PersonnageNom = m.Personnage != null ? m.Personnage.Nom : null,
                PersonnageEspece = m.Personnage != null ? m.Personnage.Espece.Nom : null,
                PersonnageCarriere = m.Personnage != null && m.Personnage.CarriereCourante != null ? m.Personnage.CarriereCourante.Intitule : null,
                PersonnageActif = m.Personnage != null && m.Personnage.EstActif,
                AjouteLe = m.AjouteLe,
            })
            .ToListAsync();

        return new PartieDetailDto
        {
            Id = resume.Id,
            Nom = resume.Nom,
            Description = resume.Description,
            Type = resume.Type,
            Statut = resume.Statut,
            MjKeycloakId = resume.MjKeycloakId,
            MjNom = resume.MjNom,
            NombreMembres = resume.NombreMembres,
            NombrePersonnagesChoisis = resume.NombrePersonnagesChoisis,
            UpdatedAt = resume.UpdatedAt,
            Membres = membres,
        };
    }

    private static string? Valider(EnregistrerPartieRequest? request)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.Nom) || request.Nom.Trim().Length < 2)
            return "Le nom de la partie est requis (2 caractères minimum).";
        if (request.Nom.Trim().Length > 120)
            return "Le nom de la partie est limité à 120 caractères.";
        if (request.Description?.Length > 2000)
            return "La description est limitée à 2000 caractères.";
        if (!Enum.IsDefined(request.Type) || !Enum.IsDefined(request.Statut))
            return "Type ou statut de partie inconnu.";
        return null;
    }

    private static string? Normaliser(string? texte) =>
        string.IsNullOrWhiteSpace(texte) ? null : texte.Trim();

    private static string Tronquer(string texte, int max) =>
        texte.Length <= max ? texte : texte[..max];
}
