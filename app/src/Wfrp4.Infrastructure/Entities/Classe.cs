namespace Wfrp4.Infrastructure.Entities;

public class Classe
{
    public int Id { get; set; }
    public string Code { get; set; } = null!;
    public string Nom { get; set; } = null!;
    public ICollection<Carriere> Carrieres { get; set; } = [];
}
