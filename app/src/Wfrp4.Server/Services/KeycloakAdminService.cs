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

public class KeycloakAdminService : IKeycloakUtilisateursAdmin
{
    private const string PrefixeCompteService = "service-account-";
    private const int DureeLienReinitialisationSecondes = 900;

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

    // --- Administration des comptes (réservée à l'admin par le contrôleur) ---

    public async Task<PageUtilisateursAdminDto> ListerAsync(string? recherche, int page, int taille, CancellationToken ct)
    {
        var (http, baseUrl) = await CreerClientAdminAsync(ct);

        // On demande un élément de plus pour savoir s'il existe une page suivante.
        var url = $"{baseUrl}/users?first={page * taille}&max={taille + 1}&briefRepresentation=false";
        if (!string.IsNullOrWhiteSpace(recherche))
            url += $"&search={Uri.EscapeDataString(recherche.Trim())}";

        var bruts = await LireAsync<List<KeycloakUserRepresentation>>(http, url, ct) ?? new();
        var pageSuivante = bruts.Count > taille;

        var resultat = new PageUtilisateursAdminDto { Page = page, Taille = taille, PageSuivante = pageSuivante };
        foreach (var brut in bruts.Take(taille).Where(u => !EstCompteService(u)))
            resultat.Utilisateurs.Add(await VersDtoAsync(http, baseUrl, brut, ct));

        return resultat;
    }

    public async Task<UtilisateurAdminDto?> ObtenirAsync(string id, CancellationToken ct)
    {
        var (http, baseUrl) = await CreerClientAdminAsync(ct);
        var brut = await ObtenirBrutAsync(http, baseUrl, id, ct);
        return brut is null || EstCompteService(brut) ? null : await VersDtoAsync(http, baseUrl, brut, ct);
    }

    public async Task DefinirRolesAsync(string id, IReadOnlyCollection<string> roles, CancellationToken ct)
    {
        var (http, baseUrl) = await CreerClientAdminAsync(ct);
        var actuels = await LireRolesApplicatifsAsync(http, baseUrl, id, ct);

        var aAjouter = new List<KeycloakRoleRepresentation>();
        foreach (var nom in roles.Except(actuels.Select(r => r.Name!)))
        {
            aAjouter.Add(await LireAsync<KeycloakRoleRepresentation>(http, $"{baseUrl}/roles/{nom}", ct)
                ?? throw new KeycloakIndisponibleException($"Le rôle '{nom}' est introuvable dans le realm Keycloak."));
        }

        var aRetirer = actuels.Where(r => !roles.Contains(r.Name!)).ToList();

        if (aAjouter.Count > 0)
            await EnvoyerAsync(http, HttpMethod.Post, $"{baseUrl}/users/{id}/role-mappings/realm", aAjouter, ct);
        if (aRetirer.Count > 0)
            await EnvoyerAsync(http, HttpMethod.Delete, $"{baseUrl}/users/{id}/role-mappings/realm", aRetirer, ct);
    }

    public async Task DefinirActivationAsync(string id, bool actif, CancellationToken ct)
    {
        var (http, baseUrl) = await CreerClientAdminAsync(ct);

        // Lecture puis réécriture de la représentation complète pour ne rien écraser côté Keycloak.
        var complet = await LireAsync<System.Text.Json.Nodes.JsonObject>(http, $"{baseUrl}/users/{id}", ct)
            ?? throw new KeycloakIndisponibleException("Compte Keycloak introuvable.");
        complet["enabled"] = actif;

        await EnvoyerAsync(http, HttpMethod.Put, $"{baseUrl}/users/{id}", complet, ct);
    }

    public async Task EnvoyerReinitialisationMotDePasseAsync(string id, CancellationToken ct)
    {
        var (http, baseUrl) = await CreerClientAdminAsync(ct);
        await EnvoyerAsync(
            http,
            HttpMethod.Put,
            $"{baseUrl}/users/{id}/execute-actions-email?lifespan={DureeLienReinitialisationSecondes}",
            new[] { "UPDATE_PASSWORD" },
            ct);
    }

    public async Task<int> CompterAdminsActifsAsync(CancellationToken ct)
    {
        var (http, baseUrl) = await CreerClientAdminAsync(ct);
        var membres = await LireAsync<List<KeycloakUserRepresentation>>(
            http, $"{baseUrl}/roles/{RolesApplicatifs.Admin}/users?first=0&max=1000", ct) ?? new();
        return membres.Count(u => u.Enabled && !EstCompteService(u));
    }

    private async Task<(HttpClient Http, string BaseUrl)> CreerClientAdminAsync(CancellationToken ct)
    {
        var authority = _configuration["Keycloak:Authority"]?.TrimEnd('/')
            ?? throw new InvalidOperationException("Configuration 'Keycloak:Authority' manquante.");
        var realm = _configuration["Keycloak:Realm"]
            ?? throw new InvalidOperationException("Configuration 'Keycloak:Realm' manquante.");

        var authorityUri = new Uri(authority);
        var baseUrl = $"{authorityUri.Scheme}://{authorityUri.Authority}/admin/realms/{realm}";

        var http = _httpClientFactory.CreateClient("KeycloakAdmin");
        string token;
        try
        {
            token = await ObtenirTokenAdminAsync(http, authority, ct);
        }
        catch (InvalidOperationException ex)
        {
            throw new KeycloakIndisponibleException(ex.Message);
        }

        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return (http, baseUrl);
    }

    private static bool EstCompteService(KeycloakUserRepresentation u) =>
        u.Username?.StartsWith(PrefixeCompteService, StringComparison.OrdinalIgnoreCase) == true;

    private async Task<KeycloakUserRepresentation?> ObtenirBrutAsync(HttpClient http, string baseUrl, string id, CancellationToken ct)
    {
        if (!Guid.TryParse(id, out _))
            return null;

        using var response = await http.GetAsync($"{baseUrl}/users/{id}", ct);
        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;
        await VerifierAsync(response, ct);
        return await response.Content.ReadFromJsonAsync<KeycloakUserRepresentation>(cancellationToken: ct);
    }

    private async Task<UtilisateurAdminDto> VersDtoAsync(HttpClient http, string baseUrl, KeycloakUserRepresentation u, CancellationToken ct)
    {
        var roles = await LireRolesApplicatifsAsync(http, baseUrl, u.Id!, ct);
        return new UtilisateurAdminDto
        {
            Id = u.Id!,
            NomUtilisateur = u.Username ?? string.Empty,
            Email = u.Email,
            Prenom = u.FirstName,
            Nom = u.LastName,
            Actif = u.Enabled,
            Roles = roles.Select(r => r.Name!).OrderBy(n => n).ToList(),
        };
    }

    private async Task<List<KeycloakRoleRepresentation>> LireRolesApplicatifsAsync(HttpClient http, string baseUrl, string id, CancellationToken ct)
    {
        var roles = await LireAsync<List<KeycloakRoleRepresentation>>(http, $"{baseUrl}/users/{id}/role-mappings/realm", ct) ?? new();
        return roles.Where(r => r.Name is not null && RolesApplicatifs.Tous.Contains(r.Name)).ToList();
    }

    private async Task<T?> LireAsync<T>(HttpClient http, string url, CancellationToken ct)
    {
        using var response = await http.GetAsync(url, ct);
        await VerifierAsync(response, ct);
        return await response.Content.ReadFromJsonAsync<T>(cancellationToken: ct);
    }

    private async Task EnvoyerAsync(HttpClient http, HttpMethod methode, string url, object corps, CancellationToken ct)
    {
        using var requete = new HttpRequestMessage(methode, url) { Content = JsonContent.Create(corps) };
        using var response = await http.SendAsync(requete, ct);
        await VerifierAsync(response, ct);
    }

    private async Task VerifierAsync(HttpResponseMessage response, CancellationToken ct)
    {
        if (response.IsSuccessStatusCode)
            return;

        // Le corps brut reste dans les logs : il n'est jamais renvoyé au client.
        var corps = await response.Content.ReadAsStringAsync(ct);
        _logger.LogError("Keycloak admin API a répondu {StatusCode} — {Body}", (int)response.StatusCode, corps);
        throw new KeycloakIndisponibleException("Le service d'administration des comptes est momentanément indisponible.");
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
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public bool Enabled { get; set; }
    }
}
