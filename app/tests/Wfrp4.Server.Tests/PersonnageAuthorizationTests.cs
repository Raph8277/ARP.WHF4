using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Wfrp4.Infrastructure.Data;
using Wfrp4.Infrastructure.Entities;
using Wfrp4.Server.Controllers;
using Wfrp4.Server.Filters;
using Wfrp4.Server.Services;
using Wfrp4.Shared.Models;

namespace Wfrp4.Server.Tests;

public class PersonnageAuthorizationTests
{
    [Theory]
    [InlineData("owner", false, false, false, false, true)]
    [InlineData("admin", true, false, false, false, true)]
    [InlineData("reader", false, true, false, false, false)]
    [InlineData("grant", false, true, true, false, false)]
    [InlineData("expired", false, true, false, true, false)]
    [InlineData("stranger", false, false, false, false, false)]
    public async Task Supprimer_possession_respecte_les_droits(
        string user, bool admin, bool mj, bool xp, bool expired, bool allowed)
    {
        await using var db = new Wfrp4DbContext(new DbContextOptionsBuilder<Wfrp4DbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        db.Personnages.Add(new Personnage { Id = 1, Nom = "Test", KeycloakId = "owner" });
        db.PersonnagePossessions.Add(new PersonnagePossession { Id = 1, PersonnageId = 1, Nom = "Épée" });
        if (mj) db.PersonnagePartages.Add(new PersonnagePartage {
            PersonnageId = 1, MjKeycloakId = user,
            Permission = xp ? PermissionPartage.XP : PermissionPartage.Lecture,
            ExpiresAt = expired ? DateTime.UtcNow.AddDays(-1) : DateTime.UtcNow.AddDays(1),
        });
        await db.SaveChangesAsync();
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, user), new(ClaimTypes.Role, "wfrp4-joueur") };
        if (admin) claims.Add(new(ClaimTypes.Role, "wfrp4-admin"));
        if (mj) claims.Add(new(ClaimTypes.Role, "wfrp4-maitre-jeu"));
        var http = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(claims, "test")) };
        var controller = new PersonnagesController(db, new PersonnageService(db, new XPService()), new SortAccessService(db)) {
            ControllerContext = new ControllerContext { HttpContext = http },
        };
        var action = new ActionContext(http, new RouteData(), new ActionDescriptor());
        var context = new ActionExecutingContext(action, new List<IFilterMetadata>(),
            new Dictionary<string, object?> { ["id"] = 1 }, controller);
        IActionResult? result = null;
        await new PersonnageOwnerFilter(db).OnActionExecutionAsync(context, async () => {
            result = await controller.SupprimerPossession(1, 1);
            return new ActionExecutedContext(action, new List<IFilterMetadata>(), controller) { Result = result };
        });
        result ??= context.Result;
        if (allowed) Assert.IsType<NoContentResult>(result);
        else Assert.IsType<ForbidResult>(result);
        Assert.Equal(!allowed, await db.PersonnagePossessions.AnyAsync(p => p.Id == 1));
    }
}
