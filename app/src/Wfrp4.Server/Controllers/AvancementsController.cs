using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wfrp4.Infrastructure.Data;
using Wfrp4.Server.Filters;
using Wfrp4.Server.Services;
using Wfrp4.Shared.DTOs;
using Wfrp4.Shared.Models;

namespace Wfrp4.Server.Controllers;

[ApiController]
[Route("api/personnages/{id}")]
[Authorize(Policy = "Joueur")]
public class AvancementsController : ControllerBase
{
    private readonly PersonnageService _personnageService;
    private readonly Wfrp4DbContext _db;

    public AvancementsController(PersonnageService personnageService, Wfrp4DbContext db)
    {
        _personnageService = personnageService;
        _db = db;
    }

    private string GetKeycloakId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new UnauthorizedAccessException();

    private async Task<bool> IsOwnerOrAdmin(int personnageId)
    {
        var keycloakId = GetKeycloakId();
        return User.IsInRole("wfrp4-admin") || await _db.Personnages.AnyAsync(p => p.Id == personnageId && p.KeycloakId == keycloakId);
    }

    [HttpPost("avances")]
    [ServiceFilter(typeof(PersonnageOwnerFilter))]
    public async Task<IActionResult> AvancerPersonnage(int id, [FromBody] AvanceRequest request)
    {
        if (!await IsOwnerOrAdmin(id))
            return Forbid();

        try
        {
            int cout = request.Type switch
            {
                TypeXP.Caracteristique when request.CodeCaracteristique != null =>
                    await _personnageService.AvancerCaracteristique(id, request.CodeCaracteristique, request.NombrePoints),
                TypeXP.Competence when request.CompetenceId.HasValue =>
                    await _personnageService.AvancerCompetence(id, request.CompetenceId.Value, request.NombrePoints),
                TypeXP.Talent when request.TalentId.HasValue =>
                    await _personnageService.AvancerTalent(id, request.TalentId.Value, request.NombrePoints),
                TypeXP.Carriere when request.NiveauCarriereId.HasValue =>
                    await _personnageService.AvancerCarriere(id, request.NiveauCarriereId.Value),
                _ => throw new InvalidOperationException("Type d'avance non supporté."),
            };

            return Ok(new { CoutXP = cout });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpPost("carrieres/retour")]
    [ServiceFilter(typeof(PersonnageOwnerFilter))]
    public async Task<IActionResult> AnnulerDernierPassageCarriere(int id)
    {
        if (!await IsOwnerOrAdmin(id))
            return Forbid();

        try
        {
            var remboursement = await _personnageService.AnnulerDernierPassageCarriere(id);
            return Ok(new { RemboursementXP = remboursement });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpPost("xp")]
    [Authorize(Policy = "MaitreJeu")]
    [ServiceFilter(typeof(PersonnageOwnerFilter))]
    public async Task<IActionResult> OctroyerXP(int id, [FromBody] XPGrantRequest request)
    {
        var mjKeycloakId = GetKeycloakId();

        // Vérifier que le MJ a la permission XP
        var hasPermission = await _db.PersonnagePartages
            .AnyAsync(pp => pp.PersonnageId == id
                && pp.MjKeycloakId == mjKeycloakId
                && pp.Permission == PermissionPartage.XP
                && (pp.ExpiresAt == null || pp.ExpiresAt > DateTime.UtcNow));

        if (!hasPermission && !User.IsInRole("wfrp4-admin"))
            return Forbid();

        await _personnageService.OctroyerXP(id, mjKeycloakId, request);
        return Ok();
    }
}
