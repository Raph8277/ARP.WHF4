namespace Wfrp4.Infrastructure.Entities;

public class SortReference
{
    public int Id { get; set; }
    public string Code { get; set; } = null!;
    public string Nom { get; set; } = null!;
    public string Categorie { get; set; } = null!;
    public string? Domaine { get; set; }
    public int? Cn { get; set; }
    public string? Portee { get; set; }
    public string? Cible { get; set; }
    public string? Duree { get; set; }
    public string? Resume { get; set; }
}
