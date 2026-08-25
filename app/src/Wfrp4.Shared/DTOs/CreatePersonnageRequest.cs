namespace Wfrp4.Shared.DTOs;

using Wfrp4.Shared.Models;

public class CreatePersonnageRequest
{
    public string Nom { get; set; } = null!;
    public GenrePersonnage Genre { get; set; } = GenrePersonnage.Masculin;
    public int EspeceId { get; set; }
    public int NiveauCarriereId { get; set; }
    public int? TitreBaseReferenceId { get; set; }
    public int? TitreQualificatifReferenceId { get; set; }
    public Dictionary<string, int> CaracteristiquesInitiales { get; set; } = new();
    public string? Motivation { get; set; }
    public int? Age { get; set; }
    public string? CouleurYeux { get; set; }
    public string? CouleurCheveux { get; set; }
    public int? TailleCm { get; set; }
    /// <summary>Bonus XP accordé pour tirage aléatoire des caractéristiques (0, 25 ou 50).</summary>
    public int XpBonus { get; set; } = 0;
    public Dictionary<int, int> CompetencesInitiales { get; set; } = new();
    public List<int> TalentsInitiaux { get; set; } = [];
}
