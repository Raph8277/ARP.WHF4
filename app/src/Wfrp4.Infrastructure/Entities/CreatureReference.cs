namespace Wfrp4.Infrastructure.Entities;

public class CreatureReference
{
    public int Id { get; set; }
    public string Code { get; set; } = null!;
    public string Nom { get; set; } = null!;
    public string Categorie { get; set; } = null!;
    public int M { get; set; }
    public int CC { get; set; }
    public int CT { get; set; }
    public int F { get; set; }
    public int E { get; set; }
    public int I { get; set; }
    public int Ag { get; set; }
    public int Dex { get; set; }
    public int Int { get; set; }
    public int FM { get; set; }
    public int Soc { get; set; }
    public int B { get; set; }
    public string Traits { get; set; } = null!;
    public string? TraitsOptionnels { get; set; }
    public int Page { get; set; }
}
