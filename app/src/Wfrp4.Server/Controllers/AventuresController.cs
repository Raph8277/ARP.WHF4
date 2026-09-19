using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wfrp4.Infrastructure.Data;
using Wfrp4.Infrastructure.Entities;
using Wfrp4.Shared.DTOs;

namespace Wfrp4.Server.Controllers;

[ApiController]
[Route("api/aventures")]
[Authorize]
public class AventuresController(Wfrp4DbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var keycloakId = GetKeycloakId();
        var items = await db.AventuresSauvegardees
            .Where(a => a.KeycloakId == keycloakId)
            .OrderByDescending(a => a.UpdatedAt)
            .Select(a => new AventureSauvegardeeSummaryDto
            {
                Id = a.Id,
                Type = a.Type,
                Titre = a.Titre,
                CreatedAt = a.CreatedAt,
                UpdatedAt = a.UpdatedAt,
            })
            .ToListAsync(ct);
        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id, CancellationToken ct)
    {
        var keycloakId = GetKeycloakId();
        var entity = await db.AventuresSauvegardees
            .FirstOrDefaultAsync(a => a.Id == id && a.KeycloakId == keycloakId, ct);
        if (entity is null) return NotFound();
        return Ok(new AventureSauvegardeeDetailDto
        {
            Id = entity.Id,
            Type = entity.Type,
            Titre = entity.Titre,
            DataJson = entity.DataJson,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
        });
    }

    [HttpPost]
    public async Task<IActionResult> Save(SaveAventureRequest request, CancellationToken ct)
    {
        var keycloakId = GetKeycloakId();
        var now = DateTime.UtcNow;
        var entity = new AventureSauvegardee
        {
            KeycloakId = keycloakId,
            Type = request.Type,
            Titre = request.Titre,
            DataJson = request.DataJson,
            CreatedAt = now,
            UpdatedAt = now,
        };
        db.AventuresSauvegardees.Add(entity);
        await db.SaveChangesAsync(ct);
        return Ok(new AventureSauvegardeeSummaryDto
        {
            Id = entity.Id,
            Type = entity.Type,
            Titre = entity.Titre,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
        });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, SaveAventureRequest request, CancellationToken ct)
    {
        var keycloakId = GetKeycloakId();
        var entity = await db.AventuresSauvegardees
            .FirstOrDefaultAsync(a => a.Id == id && a.KeycloakId == keycloakId, ct);
        if (entity is null) return NotFound();
        entity.Type = request.Type;
        entity.Titre = request.Titre;
        entity.DataJson = request.DataJson;
        entity.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return Ok(new AventureSauvegardeeSummaryDto
        {
            Id = entity.Id,
            Type = entity.Type,
            Titre = entity.Titre,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
        });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var keycloakId = GetKeycloakId();
        var entity = await db.AventuresSauvegardees
            .FirstOrDefaultAsync(a => a.Id == id && a.KeycloakId == keycloakId, ct);
        if (entity is null) return NotFound();
        db.AventuresSauvegardees.Remove(entity);
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    private string GetKeycloakId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new UnauthorizedAccessException();
}
