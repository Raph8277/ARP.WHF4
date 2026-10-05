using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wfrp4.Infrastructure.Data;
using Wfrp4.Server.Services;
using Wfrp4.Shared.DTOs;

namespace Wfrp4.Server.Controllers;

/// <summary>Parties auxquelles le joueur connecté participe ; il peut les quitter.</summary>
[ApiController]
[Route("api/participations")]
[Authorize(Policy = "Joueur")]
public class ParticipationsController : ControllerBase
{
    private readonly Wfrp4DbContext _db;
    private readonly PartieService _parties;
    private readonly ILogger<ParticipationsController> _logger;

    public ParticipationsController(Wfrp4DbContext db, PartieService parties, ILogger<ParticipationsController> logger)
    {
        _db = db;
        _parties = parties;
        _logger = logger;
    }

    private string GetKeycloakId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new UnauthorizedAccessException();

    [HttpGet]
    public async Task<ActionResult<List<ParticipationDto>>> Lister()
    {
        var keycloakId = GetKeycloakId();
        return await _db.PartieMembres
            .AsNoTracking()
            .Where(m => m.JoueurKeycloakId == keycloakId)
            .OrderByDescending(m => m.Partie.UpdatedAt)
            .Select(PartieService.VersParticipation)
            .ToListAsync();
    }

    /// <summary>
    /// Le joueur propose (ou retire) l'un de ses personnages actifs ; le MJ peut ensuite le changer.
    /// Même effet qu'un choix du MJ : partage XP rattaché à la partie.
    /// </summary>
    [HttpPut("{partieId:int}/personnage")]
    public async Task<ActionResult<ParticipationDto>> ProposerPersonnage(int partieId, [FromBody] ChoisirPersonnageRequest request)
    {
        var keycloakId = GetKeycloakId();
        var membre = await _db.PartieMembres
            .Include(m => m.Partie)
            .FirstOrDefaultAsync(m => m.PartieId == partieId && m.JoueurKeycloakId == keycloakId);
        if (membre is null)
            return NotFound();

        var personnageId = request?.PersonnageId;
        if (personnageId is not null
            && !await _db.Personnages.AnyAsync(p => p.Id == personnageId && p.KeycloakId == keycloakId && p.EstActif))
            return BadRequest(new { Error = "Ce personnage ne vous appartient pas ou n'est pas actif." });

        await _parties.ChoisirPersonnageAsync(membre.Partie, membre, personnageId);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Joueur {Joueur} a proposé le personnage {PersonnageId} pour la partie {PartieId}.",
            keycloakId, personnageId, partieId);
        return await _db.PartieMembres.AsNoTracking()
            .Where(m => m.Id == membre.Id)
            .Select(PartieService.VersParticipation)
            .FirstAsync();
    }

    [HttpDelete("{partieId:int}")]
    public async Task<IActionResult> Quitter(int partieId)
    {
        var keycloakId = GetKeycloakId();
        var membre = await _db.PartieMembres
            .Include(m => m.Partie)
            .FirstOrDefaultAsync(m => m.PartieId == partieId && m.JoueurKeycloakId == keycloakId);
        if (membre is null)
            return NotFound();

        await _parties.RetirerMembreAsync(membre.Partie, membre);
        _logger.LogInformation("Joueur {Joueur} a quitté la partie {PartieId}.", keycloakId, partieId);
        return NoContent();
    }
}
