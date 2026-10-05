namespace Wfrp4.Infrastructure.Entities;

public class PersonnageCarriere
{
    public int Id { get; set; }
    public int PersonnageId { get; set; }
    public int NiveauCarriereId { get; set; }
    public NiveauCarriere NiveauCarriere { get; set; } = null!;
    public bool EstCourante { get; set; }
    public DateTime DateEntree { get; set; }
    public DateTime? DateSortie { get; set; }
}
