namespace Wfrp4.Infrastructure.Entities;

public class PersonnageCaracteristique
{
    public int PersonnageId { get; set; }
    public string Code { get; set; } = null!;  // CC, CT, F, E, I, Ag, Dex, Int, FM, Soc
    public int ValeurInitiale { get; set; }
    public int Avances { get; set; }
}
