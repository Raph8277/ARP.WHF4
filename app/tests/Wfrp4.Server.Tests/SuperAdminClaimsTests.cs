using System.Security.Claims;
using Microsoft.Extensions.Options;
using Wfrp4.Server.Auth;
using Wfrp4.Shared.DTOs;

namespace Wfrp4.Server.Tests;

/// <summary>Unitaire : transformation des claims, sans pipeline JWT.</summary>
public class SuperAdminClaimsTests
{
    private static KeycloakClaimsTransformation Transformation() =>
        new(Options.Create(new SuperAdminsOptions { SuperAdmins = { "raph8277@gmail.com" } }));

    private static ClaimsPrincipal Principal(string? email, string? verifie, params string[] realmRoles)
    {
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, "id") };
        if (email is not null) claims.Add(new Claim(ClaimTypes.Email, email));
        if (verifie is not null) claims.Add(new Claim("email_verified", verifie));
        if (realmRoles.Length > 0)
            claims.Add(new Claim("realm_access", $"{{\"roles\":[{string.Join(",", realmRoles.Select(r => $"\"{r}\""))}]}}"));
        return new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
    }

    [Theory]
    [InlineData("raph8277@gmail.com")]
    [InlineData("Raph8277@Gmail.com")]
    public async Task Email_verifie_declare_recoit_les_trois_roles(string email)
    {
        var user = await Transformation().TransformAsync(Principal(email, "true", RolesApplicatifs.Joueur));

        Assert.All(RolesApplicatifs.Tous, r => Assert.True(user.IsInRole(r)));
    }

    [Theory]
    [InlineData("raph8277@gmail.com", "false")]
    [InlineData("raph8277@gmail.com", null)]
    [InlineData("autre@gmail.com", "true")]
    [InlineData(null, "true")]
    public async Task Sans_email_verifie_declare_aucun_role_ajoute(string? email, string? verifie)
    {
        var user = await Transformation().TransformAsync(Principal(email, verifie, RolesApplicatifs.Joueur));

        Assert.True(user.IsInRole(RolesApplicatifs.Joueur));
        Assert.False(user.IsInRole(RolesApplicatifs.MaitreJeu));
        Assert.False(user.IsInRole(RolesApplicatifs.Admin));
    }

    [Fact]
    public async Task Les_roles_realm_restent_extraits()
    {
        var user = await Transformation().TransformAsync(Principal(null, null, RolesApplicatifs.Joueur, RolesApplicatifs.MaitreJeu));

        Assert.True(user.IsInRole(RolesApplicatifs.MaitreJeu));
        Assert.False(user.IsInRole(RolesApplicatifs.Admin));
    }
}
