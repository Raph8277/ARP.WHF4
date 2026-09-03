namespace Wfrp4.Infrastructure.Entities;

public class Competence
{
    public int Id { get; set; }
    public string Code { get; set; } = null!;
    public string Nom { get; set; } = null!;
    public string Caracteristique { get; set; } = null!;
    public bool EstAvancee { get; set; }
    public bool EstGroupee { get; set; }
    public string? Description { get; set; }
}
