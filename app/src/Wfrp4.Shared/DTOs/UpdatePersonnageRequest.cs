namespace Wfrp4.Shared.DTOs;

public class UpdatePersonnageRequest
{
    public string Nom { get; set; } = null!;
    public string? Motivation { get; set; }
    public int? Age { get; set; }
    public string? CouleurYeux { get; set; }
    public string? CouleurCheveux { get; set; }
    public int? TailleCm { get; set; }
    public bool EstActif { get; set; }
    public int CouronnesOr { get; set; }
    public int PistolesArgent { get; set; }
    public int SousCuivre { get; set; }
}