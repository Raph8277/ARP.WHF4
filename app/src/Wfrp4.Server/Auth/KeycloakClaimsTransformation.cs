using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Wfrp4.Shared.DTOs;

namespace Wfrp4.Server.Auth;

public class KeycloakClaimsTransformation : IClaimsTransformation
{
    private readonly SuperAdminsOptions _superAdmins;

    public KeycloakClaimsTransformation(IOptions<SuperAdminsOptions> superAdmins)
    {
        _superAdmins = superAdmins.Value;
    }

    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        var identity = principal.Identity as ClaimsIdentity;
        if (identity == null) return Task.FromResult(principal);

        AjouterRolesRealm(principal, identity);

        // Super-admin : uniquement sur e-mail vérifié par Keycloak (ou le fournisseur social).
        var email = principal.FindFirstValue("email") ?? principal.FindFirstValue(ClaimTypes.Email);
        var verifie = string.Equals(principal.FindFirstValue("email_verified"), "true", StringComparison.OrdinalIgnoreCase);
        if (_superAdmins.EstSuperAdmin(email, verifie))
        {
            foreach (var role in RolesApplicatifs.Tous)
                AjouterRole(identity, role);
        }

        return Task.FromResult(principal);
    }

    private static void AjouterRolesRealm(ClaimsPrincipal principal, ClaimsIdentity identity)
    {
        var realmAccessClaim = principal.FindFirst("realm_access");
        if (realmAccessClaim == null) return;

        using var doc = JsonDocument.Parse(realmAccessClaim.Value);
        if (!doc.RootElement.TryGetProperty("roles", out var roles))
            return;

        foreach (var role in roles.EnumerateArray())
        {
            var roleValue = role.GetString();
            if (roleValue != null)
                AjouterRole(identity, roleValue);
        }
    }

    private static void AjouterRole(ClaimsIdentity identity, string role)
    {
        if (!identity.HasClaim(ClaimTypes.Role, role))
            identity.AddClaim(new Claim(ClaimTypes.Role, role));
    }
}
