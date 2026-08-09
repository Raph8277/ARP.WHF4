using System.Net.Http.Json;
using Wfrp4.Shared.DTOs;

namespace Wfrp4.Client.Services;

public class Wfrp4ApiClient
{
    private readonly HttpClient _http;

    public Wfrp4ApiClient(HttpClient http)
    {
        _http = http;
    }

    // --- Personnages ---
    public Task<List<PersonnageSummaryDto>?> GetMesPersonnagesAsync() =>
        _http.GetFromJsonAsync<List<PersonnageSummaryDto>>("api/personnages");

    public Task<List<PersonnageSummaryDto>?> GetPersonnagesPartagesAsync() =>
        _http.GetFromJsonAsync<List<PersonnageSummaryDto>>("api/personnages/partages-recues");

    public Task<PersonnageDetailDto?> GetPersonnageAsync(int id) =>
        _http.GetFromJsonAsync<PersonnageDetailDto>($"api/personnages/{id}");

    public Task<RevenusDto?> GetRevenusAsync(int personnageId) =>
        _http.GetFromJsonAsync<RevenusDto>($"api/personnages/{personnageId}/revenus");

    public async Task<PersonnageDetailDto?> MettreAJourPersonnageAsync(int id, UpdatePersonnageRequest request)
    {
        var response = await _http.PutAsJsonAsync($"api/personnages/{id}", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<PersonnageDetailDto>();
    }

    public async Task<PersonnageSummaryDto?> CreerPersonnageAsync(CreatePersonnageRequest request)
    {
        var response = await _http.PostAsJsonAsync("api/personnages", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<PersonnageSummaryDto>();
    }

    public async Task SupprimerPersonnageAsync(int id)
    {
        var response = await _http.DeleteAsync($"api/personnages/{id}");
        response.EnsureSuccessStatusCode();
    }

    // --- Avances ---
    public async Task AvancerAsync(int personnageId, AvanceRequest request)
    {
        var response = await _http.PostAsJsonAsync($"api/personnages/{personnageId}/avances", request);
        response.EnsureSuccessStatusCode();
    }

    public async Task OctroyerXPAsync(int personnageId, XPGrantRequest request)
    {
        var response = await _http.PostAsJsonAsync($"api/personnages/{personnageId}/xp", request);
        response.EnsureSuccessStatusCode();
    }

    // --- Partages ---
    public Task<List<PartageDto>?> GetPartagesAsync(int personnageId) =>
        _http.GetFromJsonAsync<List<PartageDto>>($"api/personnages/{personnageId}/partages");

    public async Task CreerPartageAsync(int personnageId, PartageRequest request)
    {
        var response = await _http.PostAsJsonAsync($"api/personnages/{personnageId}/partages", request);
        response.EnsureSuccessStatusCode();
    }

    public async Task SupprimerPartageAsync(int personnageId, int partageId)
    {
        var response = await _http.DeleteAsync($"api/personnages/{personnageId}/partages/{partageId}");
        response.EnsureSuccessStatusCode();
    }

    // --- Possessions ---
    public async Task<PossessionDto?> AjouterPossessionAsync(int personnageId, AjoutPossessionRequest request)
    {
        var response = await _http.PostAsJsonAsync($"api/personnages/{personnageId}/possessions", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<PossessionDto>();
    }

    public async Task SupprimerPossessionAsync(int personnageId, int possessionId)
    {
        var response = await _http.DeleteAsync($"api/personnages/{personnageId}/possessions/{possessionId}");
        response.EnsureSuccessStatusCode();
    }

    // --- Ajout compétence / talent ---
    public async Task AjouterCompetenceAsync(int personnageId, AjoutCompetenceRequest request)
    {
        var response = await _http.PostAsJsonAsync($"api/personnages/{personnageId}/competences", request);
        response.EnsureSuccessStatusCode();
    }

    public async Task AjouterTalentAsync(int personnageId, AjoutTalentRequest request)
    {
        var response = await _http.PostAsJsonAsync($"api/personnages/{personnageId}/talents", request);
        response.EnsureSuccessStatusCode();
    }

    // --- Référentiels ---
    public Task<List<EspeceDto>?> GetEspecesAsync() =>
        _http.GetFromJsonAsync<List<EspeceDto>>("api/especes");

    public Task<List<ClasseDto>?> GetClassesAsync() =>
        _http.GetFromJsonAsync<List<ClasseDto>>("api/classes");

    public Task<List<CarriereDto>?> GetCarrieresAsync() =>
        _http.GetFromJsonAsync<List<CarriereDto>>("api/carrieres");

    public Task<List<CompetenceDto>?> GetCompetencesAsync() =>
        _http.GetFromJsonAsync<List<CompetenceDto>>("api/competences");

    public Task<List<TalentDto>?> GetTalentsAsync() =>
        _http.GetFromJsonAsync<List<TalentDto>>("api/talents");

    public Task<List<ArmeReferenceDto>?> GetArmesAsync() =>
        _http.GetFromJsonAsync<List<ArmeReferenceDto>>("api/armes");
}
