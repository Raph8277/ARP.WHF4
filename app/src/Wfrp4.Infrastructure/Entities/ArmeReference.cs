namespace Wfrp4.Infrastructure.Entities;

public class ArmeReference
{
    public int Id { get; set; }
    public string Code { get; set; } = null!;
    public string Nom { get; set; } = null!;
    public string Groupe { get; set; } = null!;
    public string TypeArme { get; set; } = null!; // Mêlée, Distance
    public string? Prix { get; set; }
    public int Encombrement { get; set; }
    public string Dommage { get; set; } = null!;
    public string Disponibilite { get; set; } = null!;
    public string? Longueur { get; set; }
    public string? Portee { get; set; }
    public string? Qualites { get; set; }
    public string? Defauts { get; set; }
    public bool DeuxMains { get; set; }
}
