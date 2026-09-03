using System.Net.Http.Json;
using System.Text.Json;
using Wfrp4.Shared.DTOs;

namespace Wfrp4.Client.Services;

/// <summary>
/// Client HTTP dédié à l'inscription publique : volontairement distinct de <see cref="Wfrp4ApiClient"/>
/// pour ne pas passer par <see cref="ApiAuthorizationMessageHandler"/>, qui exige une session déjà authentifiée.
/// </summary>
public class InscriptionApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _http;

    public InscriptionApiClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<(bool Success, string? Erreur)> InscrireAsync(InscriptionRequest request)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("api/inscription", request);
            if (response.IsSuccessStatusCode)
                return (true, null);

            var contenu = await response.Content.ReadAsStringAsync();
            return (false, ExtraireMessageErreur(contenu) ?? "Une erreur est survenue lors de la création du compte.");
        }
        catch (Exception ex)
        {
            return (false, $"Impossible de contacter le serveur : {ex.Message}");
        }
    }

    private static string? ExtraireMessageErreur(string contenu)
    {
        if (string.IsNullOrWhiteSpace(contenu))
            return null;

        try
        {
            var erreur = JsonSerializer.Deserialize<ErreurDto>(contenu, JsonOptions);
            if (!string.IsNullOrWhiteSpace(erreur?.Error))
                return erreur.Error;
        }
        catch (JsonException)
        {
            // Pas au format { error }, on tente le format ValidationProblemDetails ci-dessous.
        }

        try
        {
            var probleme = JsonSerializer.Deserialize<ValidationProblemeDto>(contenu, JsonOptions);
            return probleme?.Errors?.Values.SelectMany(v => v).FirstOrDefault();
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private class ErreurDto
    {
        public string? Error { get; set; }
    }

    private class ValidationProblemeDto
    {
        public Dictionary<string, string[]>? Errors { get; set; }
    }
}
