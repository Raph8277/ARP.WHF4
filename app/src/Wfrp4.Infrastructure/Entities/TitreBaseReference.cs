namespace Wfrp4.Infrastructure.Entities;

public class TitreBaseReference
{
    public int Id { get; set; }
    public string Code { get; set; } = null!;
    public string Libelle { get; set; } = null!;
    public int Ordre { get; set; }
    public int NiveauMaitrise { get; set; } = 1;
}
