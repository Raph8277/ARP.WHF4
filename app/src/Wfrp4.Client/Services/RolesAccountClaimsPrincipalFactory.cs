using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication.Internal;
using Wfrp4.Shared.DTOs;

namespace Wfrp4.Client.Services;

/// <summary>
/// Ajoute à l'utilisateur client les rôles effectifs renvoyés par /api/moi, pour que AuthorizeView
/// et IsInRole reflètent les droits réels. Purement ergonomique : le serveur reste seul juge.
/// </summary>
public class RolesAccountClaimsPrincipalFactory : AccountClaimsPrincipalFactory<RemoteUserAccount>
{
    public const string ClientHttp = "Wfrp4.Moi";

    private readonly IHttpClientFactory _httpClientFactory;

    public RolesAccountClaimsPrincipalFactory(IAccessTokenProviderAccessor accessor, IHttpClientFactory httpClientFactory)
        : base(accessor)
    {
        _httpClientFactory = httpClientFactory;
    }

    public override async ValueTask<ClaimsPrincipal> CreateUserAsync(RemoteUserAccount account, RemoteAuthenticationUserOptions options)
    {
        var user = await base.CreateUserAsync(account, options);
        if (user.Identity is not ClaimsIdentity { IsAuthenticated: true } identity)
            return user;

        try
        {
            var moi = await _httpClientFactory.CreateClient(ClientHttp).GetFromJsonAsync<MoiDto>("api/moi");
            foreach (var role in moi?.Roles ?? new())
            {
                if (!identity.HasClaim(identity.RoleClaimType, role))
                    identity.AddClaim(new Claim(identity.RoleClaimType, role));
            }
        }
        catch (Exception ex) when (ex is HttpRequestException or AccessTokenNotAvailableException or System.Text.Json.JsonException)
        {
            // API indisponible : l'utilisateur reste connecté, sans menus réservés.
        }

        return user;
    }
}
