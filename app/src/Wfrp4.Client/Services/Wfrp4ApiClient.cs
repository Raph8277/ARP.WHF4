using System.Net.Http.Json;
using System.Text.Json;
using Wfrp4.Shared.DTOs;

namespace Wfrp4.Client.Services;

public class Wfrp4ApiClient
{
    private readonly HttpClient _http;

    public Wfrp4ApiClient(HttpClient http)
    {
        _http = http;
    }

    // --- Administration des comptes (admin uniquement, appliqué côté serveur) ---
    public Task<PageUtilisateursAdminDto?> GetUtilisateursAdminAsync(string? recherche, string? profil, int page, int taille) =>
        _http.GetFromJsonAsync<PageUtilisateursAdminDto>(
            $"api/admin/utilisateurs?page={page}&taille={taille}&recherche={Uri.EscapeDataString(recherche ?? string.Empty)}&profil={Uri.EscapeDataString(profil ?? string.Empty)}");

    public Task<ActiviteUtilisateurDto?> GetActiviteUtilisateurAsync(string id) =>
        _http.GetFromJsonAsync<ActiviteUtilisateurDto>($"api/admin/utilisateurs/{id}/activite");

    public async Task ModifierRolesUtilisateurAsync(string id, IEnumerable<string> roles)
    {
        using var response = await _http.PutAsJsonAsync($"api/admin/utilisateurs/{id}/roles", new ModifierRolesRequest { Roles = roles.ToList() });
        await EnsureSuccessAsync(response);
    }

    public async Task ModifierActivationUtilisateurAsync(string id, bool actif)
    {
        using var response = await _http.PutAsJsonAsync($"api/admin/utilisateurs/{id}/activation", new ModifierActivationRequest { Actif = actif });
        await EnsureSuccessAsync(response);
    }

    public async Task ReinitialiserMotDePasseUtilisateurAsync(string id)
    {
        using var response = await _http.PostAsync($"api/admin/utilisateurs/{id}/reinitialisation-mdp", null);
        await EnsureSuccessAsync(response);
    }

    // --- Parties (MJ propriétaire ou admin, appliqué côté serveur) ---
    public Task<List<PartieSummaryDto>?> GetPartiesAsync() =>
        _http.GetFromJsonAsync<List<PartieSummaryDto>>("api/parties");

    public Task<PartieDetailDto?> GetPartieAsync(int id) =>
        _http.GetFromJsonAsync<PartieDetailDto>($"api/parties/{id}");

    public async Task<PartieDetailDto?> CreerPartieAsync(EnregistrerPartieRequest request)
    {
        using var response = await _http.PostAsJsonAsync("api/parties", request);
        await EnsureSuccessAsync(response);
        return await response.Content.ReadFromJsonAsync<PartieDetailDto>();
    }

    public async Task<PartieDetailDto?> ModifierPartieAsync(int id, EnregistrerPartieRequest request)
    {
        using var response = await _http.PutAsJsonAsync($"api/parties/{id}", request);
        await EnsureSuccessAsync(response);
        return await response.Content.ReadFromJsonAsync<PartieDetailDto>();
    }

    public async Task SupprimerPartieAsync(int id)
    {
        using var response = await _http.DeleteAsync($"api/parties/{id}");
        await EnsureSuccessAsync(response);
    }

    public Task<List<UtilisateurResumeDto>?> RechercherJoueursAsync(string recherche) =>
        _http.GetFromJsonAsync<List<UtilisateurResumeDto>>($"api/parties/joueurs?recherche={Uri.EscapeDataString(recherche)}");

    public async Task<PartieDetailDto?> AjouterMembrePartieAsync(int partieId, string joueurKeycloakId)
    {
        using var response = await _http.PostAsJsonAsync($"api/parties/{partieId}/membres", new AjouterMembreRequest { JoueurKeycloakId = joueurKeycloakId });
        await EnsureSuccessAsync(response);
        return await response.Content.ReadFromJsonAsync<PartieDetailDto>();
    }

    public async Task RetirerMembrePartieAsync(int partieId, int membreId)
    {
        using var response = await _http.DeleteAsync($"api/parties/{partieId}/membres/{membreId}");
        await EnsureSuccessAsync(response);
    }

    public Task<List<PersonnageSummaryDto>?> GetPersonnagesMembreAsync(int partieId, int membreId) =>
        _http.GetFromJsonAsync<List<PersonnageSummaryDto>>($"api/parties/{partieId}/membres/{membreId}/personnages");

    public async Task<PartieDetailDto?> ChoisirPersonnageMembreAsync(int partieId, int membreId, int? personnageId)
    {
        using var response = await _http.PutAsJsonAsync($"api/parties/{partieId}/membres/{membreId}/personnage", new ChoisirPersonnageRequest { PersonnageId = personnageId });
        await EnsureSuccessAsync(response);
        return await response.Content.ReadFromJsonAsync<PartieDetailDto>();
    }

    public Task<List<ParticipationDto>?> GetParticipationsAsync() =>
        _http.GetFromJsonAsync<List<ParticipationDto>>("api/participations");

    public async Task<ParticipationDto?> ProposerPersonnageAsync(int partieId, int? personnageId)
    {
        using var response = await _http.PutAsJsonAsync($"api/participations/{partieId}/personnage", new ChoisirPersonnageRequest { PersonnageId = personnageId });
        await EnsureSuccessAsync(response);
        return await response.Content.ReadFromJsonAsync<ParticipationDto>();
    }

    public async Task QuitterPartieAsync(int partieId)
    {
        using var response = await _http.DeleteAsync($"api/participations/{partieId}");
        await EnsureSuccessAsync(response);
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

    public async Task<BourseDto> ModifierBourseAsync(int id, ModifierBourseRequest request)
    {
        using var response = await _http.PostAsJsonAsync($"api/personnages/{id}/bourse", request);
        await EnsureSuccessAsync(response);
        return await response.Content.ReadFromJsonAsync<BourseDto>()
            ?? throw new InvalidOperationException("La réponse de la bourse est vide.");
    }

    public async Task<PersonnageSummaryDto?> CreerPersonnageAsync(CreatePersonnageRequest request)
    {
        var response = await _http.PostAsJsonAsync("api/personnages", request);
        await EnsureSuccessAsync(response);
        return await response.Content.ReadFromJsonAsync<PersonnageSummaryDto>();
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
            return;

        try
        {
            using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            if (body.RootElement.TryGetProperty("error", out var error)
                && !string.IsNullOrWhiteSpace(error.GetString()))
            {
                throw new HttpRequestException(error.GetString(), null, response.StatusCode);
            }
        }
        catch (JsonException)
        {
            // Fall back to the standard HTTP exception when the API did not return JSON.
        }

        response.EnsureSuccessStatusCode();
    }

    public async Task SupprimerPersonnageAsync(int id)
    {
        var response = await _http.DeleteAsync($"api/personnages/{id}");
        response.EnsureSuccessStatusCode();
    }

    public async Task<byte[]> ExporterFichePdfAsync(int id)
    {
        var response = await _http.GetAsync($"api/personnages/{id}/fiche-pdf");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsByteArrayAsync();
    }

    public async Task<byte[]> PrevisualiserFichePdfAsync(int id, PdfSheetLayoutDto layout)
    {
        var response = await _http.PostAsJsonAsync($"api/personnages/{id}/fiche-pdf/preview", layout);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsByteArrayAsync();
    }

    public Task<PdfSheetLayoutDto?> GetFichePdfLayoutAsync() =>
        _http.GetFromJsonAsync<PdfSheetLayoutDto>("api/personnages/fiche-pdf/layout");

    public async Task<byte[]> GetFichePdfTemplateAsync(int page)
    {
        var response = await _http.GetAsync($"api/personnages/fiche-pdf/template/{page}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsByteArrayAsync();
    }

    public Task<List<PdfSheetLayoutSummaryDto>?> GetFichePdfLayoutsAsync() =>
        _http.GetFromJsonAsync<List<PdfSheetLayoutSummaryDto>>("api/personnages/fiche-pdf/layouts");

    public Task<PdfSheetLayoutDto?> GetFichePdfLayoutAsync(string key) =>
        _http.GetFromJsonAsync<PdfSheetLayoutDto>($"api/personnages/fiche-pdf/layouts/{Uri.EscapeDataString(key)}");

    public async Task EnregistrerFichePdfLayoutAsync(PdfSheetLayoutDto layout)
    {
        var response = await _http.PutAsJsonAsync("api/personnages/fiche-pdf/layout", layout);
        response.EnsureSuccessStatusCode();
    }

    public async Task<byte[]> ExporterMjPdfAsync(MjPdfExportRequest request)
    {
        var response = await _http.PostAsJsonAsync("api/mj/pdf", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsByteArrayAsync();
    }

    public async Task<byte[]> GetMjPdfTemplateAsync()
    {
        var response = await _http.GetAsync("api/mj/pdf/template");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsByteArrayAsync();
    }

    // --- Avances ---
    public async Task AvancerAsync(int personnageId, AvanceRequest request)
    {
        var response = await _http.PostAsJsonAsync($"api/personnages/{personnageId}/avances", request);
        await EnsureSuccessAsync(response);
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

    // --- Sorts et parchemins ---
    public async Task AjouterSortAsync(int personnageId, AjoutSortRequest request)
    {
        var response = await _http.PostAsJsonAsync($"api/personnages/{personnageId}/sorts", request);
        await EnsureSuccessAsync(response);
    }

    public async Task AnnulerDernierPassageCarriereAsync(int personnageId)
    {
        var response = await _http.PostAsync($"api/personnages/{personnageId}/carrieres/retour", null);
        await EnsureSuccessAsync(response);
    }

    public async Task SupprimerSortAsync(int personnageId, int sortId)
    {
        var response = await _http.DeleteAsync($"api/personnages/{personnageId}/sorts/{sortId}");
        response.EnsureSuccessStatusCode();
    }

    public async Task AjouterParcheminAsync(int personnageId, AjoutParcheminRequest request)
    {
        var response = await _http.PostAsJsonAsync($"api/personnages/{personnageId}/parchemins", request);
        response.EnsureSuccessStatusCode();
    }

    public async Task SupprimerParcheminAsync(int personnageId, int parcheminId)
    {
        var response = await _http.DeleteAsync($"api/personnages/{personnageId}/parchemins/{parcheminId}");
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

    // --- Aventures sauvegardées ---
    public Task<List<AventureSauvegardeeSummaryDto>?> GetAventuresSauvegardeesAsync() =>
        _http.GetFromJsonAsync<List<AventureSauvegardeeSummaryDto>>("api/aventures");

    public Task<AventureSauvegardeeDetailDto?> GetAventureSauvegardeeAsync(int id) =>
        _http.GetFromJsonAsync<AventureSauvegardeeDetailDto>($"api/aventures/{id}");

    public async Task<AventureSauvegardeeSummaryDto?> SauvegarderAventureAsync(SaveAventureRequest request)
    {
        var response = await _http.PostAsJsonAsync("api/aventures", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<AventureSauvegardeeSummaryDto>();
    }

    public async Task SupprimerAventureSauvegardeeAsync(int id)
    {
        var response = await _http.DeleteAsync($"api/aventures/{id}");
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

    public Task<List<SortReferenceDto>?> GetSortsAsync() =>
        _http.GetFromJsonAsync<List<SortReferenceDto>>("api/sorts");

    public Task<List<CreatureReferenceDto>?> GetCreaturesAsync() =>
        _http.GetFromJsonAsync<List<CreatureReferenceDto>>("api/creatures");

    public Task<List<TitreBaseReferenceDto>?> GetTitresBaseAsync() =>
        _http.GetFromJsonAsync<List<TitreBaseReferenceDto>>("api/titres/bases");

    public Task<List<TitreQualificatifReferenceDto>?> GetTitresQualificatifsAsync() =>
        _http.GetFromJsonAsync<List<TitreQualificatifReferenceDto>>("api/titres/qualificatifs");
}
