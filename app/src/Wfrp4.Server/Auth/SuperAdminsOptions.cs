namespace Wfrp4.Server.Auth;

/// <summary>
/// Propriétaires de l'application, désignés par e-mail vérifié (section "Administration").
/// Ils reçoivent les rôles joueur, MJ et admin quel que soit leur mapping Keycloak.
/// </summary>
public class SuperAdminsOptions
{
    public const string Section = "Administration";

    public List<string> SuperAdmins { get; set; } = new();

    public bool EstSuperAdmin(string? email, bool emailVerifie) =>
        emailVerifie
        && !string.IsNullOrWhiteSpace(email)
        && SuperAdmins.Any(s => string.Equals(s?.Trim(), email.Trim(), StringComparison.OrdinalIgnoreCase));
}
