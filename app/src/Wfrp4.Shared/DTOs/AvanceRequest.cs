using Wfrp4.Shared.Models;

namespace Wfrp4.Shared.DTOs;

public class AvanceRequest
{
    public TypeXP Type { get; set; }
    public string? CodeCaracteristique { get; set; }
    public int? CompetenceId { get; set; }
    public int? TalentId { get; set; }
    public int? NiveauCarriereId { get; set; }
}
