using Wfrp4.Shared.Models;

namespace Wfrp4.Shared.DTOs;

public class RevenusDto
{
    public StatutTier Tier { get; set; }
    public int Rang { get; set; }
    public string Monnaie { get; set; } = null!;
    public string Formule { get; set; } = null!;
    public int RevenuMin { get; set; }
    public int RevenuMax { get; set; }
}
