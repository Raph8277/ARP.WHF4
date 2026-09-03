using Wfrp4.Shared.Models;

namespace Wfrp4.Infrastructure.Entities;

public class NiveauCarriere
{
    public int Id { get; set; }
    public int CarriereId { get; set; }
    public Carriere Carriere { get; set; } = null!;
    public int Niveau { get; set; }
    public string Intitule { get; set; } = null!;
    public StatutTier Statut { get; set; }
    public int StatutNumerique { get; set; }
    public string? AvancesCarac { get; set; }  // JSON
    public string? CompetenceRevenu { get; set; }
    public string? TalentsRevenu { get; set; }
    public string? Dotations { get; set; }
}
