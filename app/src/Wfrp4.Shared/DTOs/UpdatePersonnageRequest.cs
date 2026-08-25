namespace Wfrp4.Shared.DTOs;

using Wfrp4.Shared.Models;

public class UpdatePersonnageRequest
{
    public string Nom { get; set; } = null!;
    public GenrePersonnage Genre { get; set; } = GenrePersonnage.Masculin;
    public string? Motivation { get; set; }
    public int? TitreBaseReferenceId { get; set; }
    public int? TitreQualificatifReferenceId { get; set; }
    public int? Age { get; set; }
    public string? CouleurYeux { get; set; }
    public string? CouleurCheveux { get; set; }
    public int? TailleCm { get; set; }
    public bool EstActif { get; set; }
    public int CouronnesOr { get; set; }
    public int PistolesArgent { get; set; }
    public int SousCuivre { get; set; }
    public string? AmbitionCourtTerme { get; set; }
    public string? AmbitionLongTerme { get; set; }
    public string? GroupeNom { get; set; }
    public string? GroupeMembres { get; set; }
    public string? Psychologie { get; set; }
    public string? CorruptionMutations { get; set; }
}
