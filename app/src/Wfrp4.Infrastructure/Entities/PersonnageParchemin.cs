namespace Wfrp4.Infrastructure.Entities;

public class PersonnageParchemin
{
    public int Id { get; set; }
    public int PersonnageId { get; set; }
    public Personnage Personnage { get; set; } = null!;
    public int SortReferenceId { get; set; }
    public SortReference SortReference { get; set; } = null!;
    public int Quantite { get; set; } = 1;
}
