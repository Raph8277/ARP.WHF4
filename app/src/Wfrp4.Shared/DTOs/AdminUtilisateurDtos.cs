namespace Wfrp4.Shared.DTOs;

/// <summary>Rôles applicatifs modifiables depuis l'administration (liste blanche).</summary>
public static class RolesApplicatifs
{
    public const string Joueur = "wfrp4-joueur";
    public const string MaitreJeu = "wfrp4-maitre-jeu";
    public const string Admin = "wfrp4-admin";

    public static readonly IReadOnlyList<string> Tous = new[] { Joueur, MaitreJeu, Admin };
}

/// <summary>Profil hiérarchique affiché dans l'administration ; chaque profil porte les rôles inférieurs.</summary>
public enum ProfilUtilisateur
{
    Aucun,
    Joueur,
    MaitreJeu,
    Admin,
}

public static class ProfilsUtilisateur
{
    public static ProfilUtilisateur Depuis(IEnumerable<string> roles)
    {
        var set = roles as ICollection<string> ?? roles.ToList();
        if (set.Contains(RolesApplicatifs.Admin)) return ProfilUtilisateur.Admin;
        if (set.Contains(RolesApplicatifs.MaitreJeu)) return ProfilUtilisateur.MaitreJeu;
        if (set.Contains(RolesApplicatifs.Joueur)) return ProfilUtilisateur.Joueur;
        return ProfilUtilisateur.Aucun;
    }

    public static List<string> Roles(ProfilUtilisateur profil) => profil switch
    {
        ProfilUtilisateur.Admin => new() { RolesApplicatifs.Joueur, RolesApplicatifs.MaitreJeu, RolesApplicatifs.Admin },
        ProfilUtilisateur.MaitreJeu => new() { RolesApplicatifs.Joueur, RolesApplicatifs.MaitreJeu },
        ProfilUtilisateur.Joueur => new() { RolesApplicatifs.Joueur },
        _ => new(),
    };

    public static string Libelle(ProfilUtilisateur profil) => profil switch
    {
        ProfilUtilisateur.Admin => "Administrateur",
        ProfilUtilisateur.MaitreJeu => "Maître de jeu",
        ProfilUtilisateur.Joueur => "Joueur",
        _ => "Aucun rôle",
    };
}

public class UtilisateurAdminDto
{
    public string Id { get; set; } = null!;
    public string NomUtilisateur { get; set; } = null!;
    public string? Email { get; set; }
    public bool EmailVerifie { get; set; }
    public bool Actif { get; set; }
    public List<string> Roles { get; set; } = new();

    /// <summary>Administrateur déclaré par configuration : rôles effectifs non modifiables depuis l'interface.</summary>
    public bool SuperAdmin { get; set; }
}

public class ActiviteUtilisateurDto
{
    public int NombrePersonnages { get; set; }
    public List<PartieSummaryDto> PartiesMenees { get; set; } = new();
    public List<ParticipationDto> Participations { get; set; } = new();
}

public class PageUtilisateursAdminDto
{
    public List<UtilisateurAdminDto> Utilisateurs { get; set; } = new();
    public int Page { get; set; }
    public int Taille { get; set; }
    public bool PageSuivante { get; set; }
}

public class ModifierRolesRequest
{
    public List<string> Roles { get; set; } = new();
}

public class ModifierActivationRequest
{
    public bool Actif { get; set; }
}
