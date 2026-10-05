namespace Wfrp4.Shared.DTOs;

/// <summary>Rôles applicatifs effectifs calculés par l'API (dont super-admin) pour l'affichage client.</summary>
public class MoiDto
{
    public string Id { get; set; } = null!;
    public List<string> Roles { get; set; } = new();
}
