using Wfrp4.Shared.Models;

namespace Wfrp4.Infrastructure.Entities;

public class HistoriqueXP
{
    public int Id { get; set; }
    public int PersonnageId { get; set; }
    public Personnage Personnage { get; set; } = null!;
    public string AuteurKeycloakId { get; set; } = null!;
    public int Montant { get; set; }
    public TypeXP Type { get; set; }
    public string? Cible { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}
