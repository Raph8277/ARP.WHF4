using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Wfrp4.Shared.DTOs;

namespace Wfrp4.Server.Services;

/// <summary>
/// Erreur métier levée lorsque le nom d'utilisateur ou l'email demandé est déjà pris (409 Keycloak).
/// </summary>
public class InscriptionConflictException : InvalidOperationException
{
    public InscriptionConflictException(string message) : base(message)
    {
    }
}

public class KeycloakAdminService
{
    private const string RoleJoueur = "wfrp4-joueur";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<KeycloakAdminService> _logger;

    public KeycloakAdminService(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<KeycloakAdminService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task CreerUtilisateurAsync(InscriptionRequest request, CancellationToken ct)
    {
        var authority = _configuration["Keycloak:Authority"]?.TrimEnd('/')
            ?? throw new InvalidOperationException("Configuration 'Keycloak:Authority' manquante.");
        var realm = _configuration["Keycloak:Realm"]
            ?? throw new InvalidOperationException("Configuration 'Keycloak:Realm' manquante.");

        var authorityUri = new Uri(authority);
        var adminBaseUrl = $"{authorityUri.Scheme}://{authorityUri.Authority}/admin/realms/{realm}";

        var http = _httpClientFactory.CreateClient("KeycloakAdmin");
        var accessToken = await ObtenirTokenAdminAsync(http, authority, ct);
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var userId = await CreerCompteAsync(http, adminBaseUrl, request, ct);
        await DefinirMotDePasseAsync(http, adminBaseUrl, userId, request.MotDePasse, ct);
        await AssignerRoleJoueurAsync(http, adminBaseUrl, userId, ct);
    }

    private async Task<string> ObtenirTokenAdminAsync(HttpClient http, string authority, CancellationToken ct)
    {
        var clientId = _configuration["Keycloak:AdminClientId"]
            ?? throw new InvalidOperationException("Configuration 'Keycloak:AdminClientId' manquante.");
        var clientSecret = _configuration["Keycloak:AdminClientSecret"] ?? string.Empty;

        var form = new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials",
            ["client_id"] = clientId,
            ["client_secret"] = clientSecret,
        };

        using var response = await http.PostAsync(
            $"{authority}/protocol/openid-connect/token",
            new FormUrlEncodedContent(form),
            ct);

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException("Le service d'inscription est momentanément indisponible. Veuillez réessayer plus tard.");

        var token = await response.Content.ReadFromJsonAsync<KeycloakTokenResponse>(cancellationToken: ct);
        return token?.AccessToken
            ?? throw new InvalidOperationException("Le service d'inscription est momentanément indisponible. Veuillez réessayer plus tard.");
    }

    private async Task<string> CreerCompteAsync(HttpClient http, string adminBaseUrl, InscriptionRequest request, CancellationToken ct)
    {
        var payload = new
        {
            username = request.NomUtilisateur,
            email = request.Email,
            enabled = true,
            firstName = request.Prenom,
            lastName = request.Nom,
        };

        using var response = await http.PostAsJsonAsync($"{adminBaseUrl}/users", payload, ct);

        if (response.StatusCode == HttpStatusCode.Conflict)
            throw new InscriptionConflictException("Ce nom d'utilisateur ou cet email est déjà utilisé.");

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError("Keycloak user creation failed: {StatusCode} — {Body}", (int)response.StatusCode, body);
            throw new InvalidOperationException("La création du compte a échoué. Veuillez réessayer plus tard.");
        }

        // Keycloak ne renvoie pas de corps sur la création : l'id est dans l'en-tête Location.
        var location = response.Headers.Location;
        if (location is not null)
            return location.Segments[^1].TrimEnd('/');

        // Filet de sécurité si l'en-tête Location est absent (proxy, etc.) : on relit par username.
        var utilisateurs = await http.GetFromJsonAsync<List<KeycloakUserRepresentation>>(
            $"{adminBaseUrl}/users?username={Uri.EscapeDataString(request.NomUtilisateur)}&exact=true", ct);
        var utilisateur = utilisateurs?.FirstOrDefault();

        return utilisateur?.Id
            ?? throw new InvalidOperationException("Le compte a été créé mais son identifiant n'a pas pu être déterminé.");
    }

    private static async Task DefinirMotDePasseAsync(HttpClient http, string adminBaseUrl, string userId, string motDePasse, CancellationToken ct)
    {
        var payload = new
        {
            type = "password",
            value = motDePasse,
            temporary = false,
        };

        using var response = await http.PutAsJsonAsync($"{adminBaseUrl}/users/{userId}/reset-password", payload, ct);

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException("Le compte a été créé mais la définition du mot de passe a échoué.");
    }

    private static async Task AssignerRoleJoueurAsync(HttpClient http, string adminBaseUrl, string userId, CancellationToken ct)
    {
        var role = await http.GetFromJsonAsync<KeycloakRoleRepresentation>($"{adminBaseUrl}/roles/{RoleJoueur}", ct)
            ?? throw new InvalidOperationException($"Le compte a été créé mais le rôle '{RoleJoueur}' est introuvable dans le realm Keycloak.");

        using var response = await http.PostAsJsonAsync(
            $"{adminBaseUrl}/users/{userId}/role-mappings/realm",
            new[] { role },
            ct);

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException("Le compte a été créé mais l'attribution du rôle joueur a échoué.");
    }

    private class KeycloakTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }
    }

    private class KeycloakRoleRepresentation
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
    }

    private class KeycloakUserRepresentation
    {
        public string? Id { get; set; }
    }
}
