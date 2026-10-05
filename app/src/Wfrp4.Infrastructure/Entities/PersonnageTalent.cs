namespace Wfrp4.Infrastructure.Entities;

public class PersonnageTalent
{
    public int PersonnageId { get; set; }
    public int TalentId { get; set; }
    public Talent Talent { get; set; } = null!;
    public int Fois { get; set; }
}
