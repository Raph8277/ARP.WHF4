using System.ComponentModel.DataAnnotations;

namespace Wfrp4.Shared.DTOs;

public class ModifierBourseRequest
{
    [Required, RegularExpression("^[CPS]$")]
    public string Monnaie { get; set; } = "C";
    [Range(-1000000, 1000000)]
    public int Delta { get; set; }
}

public record BourseDto(int CouronnesOr, int PistolesArgent, int SousCuivre);
