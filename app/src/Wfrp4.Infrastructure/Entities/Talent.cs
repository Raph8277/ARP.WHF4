namespace Wfrp4.Infrastructure.Entities;

public class Talent
{
    public int Id { get; set; }
    public string Code { get; set; } = null!;
    public string Nom { get; set; } = null!;
    public int? MaxFois { get; set; }
    public bool Empilable { get; set; }
    public string? Description { get; set; }
    public string? Effet { get; set; }
}
