namespace Wfrp4.Infrastructure.Entities;

public class Espece
{
    public int Id { get; set; }
    public string Code { get; set; } = null!;
    public string Nom { get; set; } = null!;
    public int MouvementBase { get; set; }
    public string? CaracInitiales { get; set; }  // JSON
    public string? TraitsPhysiques { get; set; } // JSON
    public string? Description { get; set; }
}
