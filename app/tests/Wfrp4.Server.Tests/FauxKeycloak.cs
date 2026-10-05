using Wfrp4.Server.Services;
using Wfrp4.Shared.DTOs;

namespace Wfrp4.Server.Tests;

/// <summary>Service Keycloak simulé : comptes en mémoire, journal des écritures.</summary>
internal sealed class FauxKeycloak : IKeycloakUtilisateursAdmin
{
    public Dictionary<string, UtilisateurAdminDto> Comptes { get; } = new();
    public List<string> Appels { get; } = new();
    public bool Panne { get; set; }

    public Task<PageUtilisateursAdminDto> ListerAsync(string? recherche, ProfilUtilisateur? profil, int page, int taille, CancellationToken ct) =>
        Task.FromResult(new PageUtilisateursAdminDto
        {
            Utilisateurs = Comptes.Values.Where(c => profil is null || ProfilsUtilisateur.Depuis(c.Roles) == profil).ToList(),
            Page = page,
            Taille = taille,
        });

    public Task<List<UtilisateurResumeDto>> RechercherJoueursAsync(string? recherche, int max, CancellationToken ct)
    {
        if (Panne) throw new KeycloakIndisponibleException("indisponible");
        return Task.FromResult(Comptes.Values
            .Where(c => c.Actif && c.Roles.Contains(RolesApplicatifs.Joueur)
                && (string.IsNullOrWhiteSpace(recherche) || c.NomUtilisateur.Contains(recherche.Trim(), StringComparison.OrdinalIgnoreCase)))
            .OrderBy(c => c.NomUtilisateur)
            .Take(max)
            .Select(c => new UtilisateurResumeDto { Id = c.Id, NomUtilisateur = c.NomUtilisateur, Email = c.Email })
            .ToList());
    }

    public Task<UtilisateurAdminDto?> ObtenirAsync(string id, CancellationToken ct)
    {
        if (Panne) throw new KeycloakIndisponibleException("indisponible");
        return Task.FromResult(Comptes.GetValueOrDefault(id));
    }

    public Task DefinirRolesAsync(string id, IReadOnlyCollection<string> roles, CancellationToken ct)
    {
        Appels.Add($"roles:{id}:{string.Join(",", roles.OrderBy(r => r))}");
        Comptes[id].Roles = roles.ToList();
        return Task.CompletedTask;
    }

    public Task DefinirActivationAsync(string id, bool actif, CancellationToken ct)
    {
        Appels.Add($"actif:{id}:{actif}");
        Comptes[id].Actif = actif;
        return Task.CompletedTask;
    }

    public Task EnvoyerReinitialisationMotDePasseAsync(string id, CancellationToken ct)
    {
        Appels.Add($"mdp:{id}");
        return Task.CompletedTask;
    }

    public Task<int> CompterAdminsActifsAsync(CancellationToken ct) =>
        Task.FromResult(Comptes.Values.Count(c => c.Actif && c.Roles.Contains(RolesApplicatifs.Admin)));

    public static UtilisateurAdminDto Compte(string id, string nom, params string[] roles) => new()
    {
        Id = id, NomUtilisateur = nom, Email = nom + "@wfrp4.local", Actif = true, Roles = roles.ToList(),
    };
}
