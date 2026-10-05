using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Wfrp4.Infrastructure.Data;
using Wfrp4.Shared.Models;

namespace Wfrp4.Server.Filters;

public class PersonnageOwnerFilter : IAsyncActionFilter
{
    private readonly Wfrp4DbContext _db;

    public PersonnageOwnerFilter(Wfrp4DbContext db)
    {
        _db = db;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var user = context.HttpContext.User;
        var keycloakId = user.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(keycloakId))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        // Admin a accès à tout
        if (user.IsInRole("wfrp4-admin"))
        {
            await next();
            return;
        }

        // Vérifier le paramètre de route 'id'
        if (!context.ActionArguments.TryGetValue("id", out var idObj) || idObj is not int personnageId)
        {
            await next();
            return;
        }

        var personnage = await _db.Personnages
            .AsNoTracking()
            .Select(p => new { p.Id, p.KeycloakId })
            .FirstOrDefaultAsync(p => p.Id == personnageId);

        if (personnage == null)
        {
            context.Result = new NotFoundResult();
            return;
        }

        // Propriétaire direct
        if (personnage.KeycloakId == keycloakId)
        {
            await next();
            return;
        }

        // Vérifier partage MJ
        if (user.IsInRole("wfrp4-maitre-jeu"))
        {
            var partage = await _db.PersonnagePartages
                .AsNoTracking()
                .AnyAsync(pp => pp.PersonnageId == personnageId
                    && pp.MjKeycloakId == keycloakId
                    && (pp.ExpiresAt == null || pp.ExpiresAt > DateTime.UtcNow));

            if (partage)
            {
                await next();
                return;
            }
        }

        context.Result = new ForbidResult();
    }
}
