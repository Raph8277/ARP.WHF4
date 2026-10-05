using Wfrp4.Shared.DTOs;

namespace Wfrp4.Server.Services;

/// <summary>Le service Keycloak est indisponible ou a refusé l'appel (mappé en 503).</summary>
public class KeycloakIndisponibleException : Exception
{
    public KeycloakIndisponibleException(string message) : base(message)
    {
    }
}

/// <summary>Administration des comptes Keycloak via le compte de service de l'application.</summary>
public interface IKeycloakUtilisateursAdmin
{
    /// <param name="profil">Filtre sur le profil le plus élevé ; null pour tous les comptes.</param>
    Task<PageUtilisateursAdminDto> ListerAsync(string? recherche, ProfilUtilisateur? profil, int page, int taille, CancellationToken ct);

    /// <summary>
    /// Comptes actifs ayant le rôle joueur, triés : utilisé par les MJ pour composer une partie.
    /// Recherche vide : tous les joueurs (jusqu'à <paramref name="max"/>).
    /// </summary>
    Task<List<UtilisateurResumeDto>> RechercherJoueursAsync(string? recherche, int max, CancellationToken ct);

    /// <summary>Renvoie null si le compte n'existe pas ou s'il s'agit d'un compte de service.</summary>
    Task<UtilisateurAdminDto?> ObtenirAsync(string id, CancellationToken ct);

    Task DefinirRolesAsync(string id, IReadOnlyCollection<string> roles, CancellationToken ct);

    Task DefinirActivationAsync(string id, bool actif, CancellationToken ct);

    Task EnvoyerReinitialisationMotDePasseAsync(string id, CancellationToken ct);

    Task<int> CompterAdminsActifsAsync(CancellationToken ct);
}
