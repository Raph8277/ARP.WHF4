namespace Wfrp4.Shared.DTOs;

/// <summary>Rôles applicatifs modifiables depuis l'administration (liste blanche).</summary>
public static class RolesApplicatifs
{
    public const string Joueur = "wfrp4-joueur";
    public const string MaitreJeu = "wfrp4-maitre-jeu";
    public const string Admin = "wfrp4-admin";

    public static readonly IReadOnlyList<string> Tous = new[] { Joueur, MaitreJeu, Admin };
}

public class UtilisateurAdminDto
{
    public string Id { get; set; } = null!;
    public string NomUtilisateur { get; set; } = null!;
    public string? Email { get; set; }
    public string? Prenom { get; set; }
    public string? Nom { get; set; }
    public bool Actif { get; set; }
    public List<string> Roles { get; set; } = new();
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
