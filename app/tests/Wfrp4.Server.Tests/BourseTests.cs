using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wfrp4.Infrastructure.Data;
using Wfrp4.Infrastructure.Entities;
using Wfrp4.Server.Controllers;
using Wfrp4.Server.Services;
using Wfrp4.Shared.DTOs;

namespace Wfrp4.Server.Tests;

public class BourseTests
{
    [Theory]
    [InlineData("owner", false, true)]
    [InlineData("admin", true, true)]
    [InlineData("mj", false, false)]
    public async Task Bourse_ne_modifie_que_la_monnaie_et_controle_le_proprietaire(string user, bool admin, bool allowed)
    {
        await using var db = CreateDb();
        var personnage = new Personnage { Id = 1, KeycloakId = "owner", Nom = "Identité conservée",
            Motivation = "Motivation conservée", CouronnesOr = 10, PistolesArgent = 20, SousCuivre = 30 };
        db.Add(personnage);
        await db.SaveChangesAsync();
        var result = await Controller(db, user, admin).ModifierBourse(1, new() { Monnaie = "C", Delta = 1 });
        if (allowed) Assert.Equal(new BourseDto(11, 20, 30), result.Value);
        else Assert.IsType<ForbidResult>(result.Result);
        db.ChangeTracker.Clear();
        var saved = await db.Personnages.SingleAsync();
        Assert.Equal(allowed ? 11 : 10, saved.CouronnesOr);
        Assert.Equal("Identité conservée", saved.Nom);
        Assert.Equal("Motivation conservée", saved.Motivation);
        Assert.Equal(20, saved.PistolesArgent);
        Assert.Equal(30, saved.SousCuivre);
    }

    [Theory]
    [InlineData("C", -20, 0)]
    [InlineData("P", 3, 13)]
    [InlineData("S", -1, 9)]
    public async Task Bourse_accepte_les_trois_monnaies_et_borne_a_zero(string monnaie, int delta, int expected)
    {
        await using var db = CreateDb();
        db.Add(new Personnage { Id = 1, KeycloakId = "owner", Nom = "Test", CouronnesOr = 10, PistolesArgent = 10, SousCuivre = 10 });
        await db.SaveChangesAsync();
        var result = await Controller(db, "owner").ModifierBourse(1, new() { Monnaie = monnaie, Delta = delta });
        Assert.NotNull(result.Value);
        Assert.Equal(expected, monnaie switch { "C" => result.Value.CouronnesOr, "P" => result.Value.PistolesArgent, _ => result.Value.SousCuivre });
    }

    [Theory]
    [InlineData("X", 1)]
    [InlineData("C", int.MaxValue)]
    [InlineData("C", 1)]
    public async Task Bourse_refuse_les_entrees_invalides_et_le_debordement(string monnaie, int delta)
    {
        await using var db = CreateDb();
        db.Add(new Personnage { Id = 1, KeycloakId = "owner", Nom = "Test", CouronnesOr = int.MaxValue });
        await db.SaveChangesAsync();
        var result = await Controller(db, "owner").ModifierBourse(1, new() { Monnaie = monnaie, Delta = delta });
        Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(int.MaxValue, (await db.Personnages.SingleAsync()).CouronnesOr);
    }

    private static Wfrp4DbContext CreateDb() => new(new DbContextOptionsBuilder<Wfrp4DbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
    private static PersonnagesController Controller(Wfrp4DbContext db, string user, bool admin = false)
    {
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, user), new(ClaimTypes.Role, "wfrp4-joueur"), new(ClaimTypes.Role, "wfrp4-maitre-jeu") };
        if (admin) claims.Add(new(ClaimTypes.Role, "wfrp4-admin"));
        return new(db, new(db, new XPService()), new(db)) {
            ControllerContext = new() { HttpContext = new DefaultHttpContext { User = new(new ClaimsIdentity(claims, "test")) } },
        };
    }
}
