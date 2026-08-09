using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wfrp4.Infrastructure.Data;
using Wfrp4.Infrastructure.Entities;
using Wfrp4.Server.Filters;
using Wfrp4.Shared.DTOs;

namespace Wfrp4.Server.Controllers;

[ApiController]
[Route("api/personnages/{id}/partages")]
[Authorize(Policy = "Joueur")]
public class PartagesController : ControllerBase
{
    private readonly Wfrp4DbContext _db;

    public PartagesController(Wfrp4DbContext db)
    {
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

    [HttpGet]
    [ServiceFilter(typeof(PersonnageOwnerFilter))]
    public async Task<ActionResult<List<PartageDto>>> GetPartages(int id)
    {
        if (!await IsOwnerOrAdmin(id))
            return Forbid();

        var partages = await _db.PersonnagePartages
            .AsNoTracking()
            .Where(pp => pp.PersonnageId == id)
            .Select(pp => new PartageDto
            {
                Id = pp.Id,
                MjKeycloakId = pp.MjKeycloakId,
                Permission = pp.Permission,
                CreatedAt = pp.CreatedAt,
                ExpiresAt = pp.ExpiresAt,
            })
            .ToListAsync();

        return Ok(partages);
    }

    [HttpPost]
    [ServiceFilter(typeof(PersonnageOwnerFilter))]
    public async Task<ActionResult<PartageDto>> CreerPartage(int id, [FromBody] PartageRequest request)
    {
        if (!await IsOwnerOrAdmin(id))
            return Forbid();

        // Vérifier que le partage n'existe pas déjà
        var exists = await _db.PersonnagePartages
            .AnyAsync(pp => pp.PersonnageId == id && pp.MjKeycloakId == request.MjKeycloakId);

        if (exists)
            return Conflict(new { Error = "Un partage existe déjà avec ce MJ." });

        var partage = new PersonnagePartage
        {
            PersonnageId = id,
            MjKeycloakId = request.MjKeycloakId,
            Permission = request.Permission,
            CreatedAt = DateTime.UtcNow,
        };

        _db.PersonnagePartages.Add(partage);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetPartages), new { id }, new PartageDto
        {
            Id = partage.Id,
            MjKeycloakId = partage.MjKeycloakId,
            Permission = partage.Permission,
            CreatedAt = partage.CreatedAt,
        });
    }

    [HttpDelete("{partageId}")]
    [ServiceFilter(typeof(PersonnageOwnerFilter))]
    public async Task<IActionResult> SupprimerPartage(int id, int partageId)
    {
        if (!await IsOwnerOrAdmin(id))
            return Forbid();

        var partage = await _db.PersonnagePartages
            .FirstOrDefaultAsync(pp => pp.Id == partageId && pp.PersonnageId == id);

        if (partage == null) return NotFound();

        _db.PersonnagePartages.Remove(partage);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
