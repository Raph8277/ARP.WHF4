namespace Wfrp4.Infrastructure.Entities;

public class Carriere
{
    public int Id { get; set; }
    public string Code { get; set; } = null!;
    public string Nom { get; set; } = null!;
    public int ClasseId { get; set; }
    public Classe Classe { get; set; } = null!;
    public string? EspecesAutorisees { get; set; }
    public ICollection<NiveauCarriere> Niveaux { get; set; } = [];
}
