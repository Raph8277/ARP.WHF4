using Wfrp4.Shared.Models;

namespace Wfrp4.Infrastructure.Entities;

public class PersonnagePossession
{
    public int Id { get; set; }
    public int PersonnageId { get; set; }
    public string Nom { get; set; } = null!;
    public TypePossession Type { get; set; }
    public int Quantite { get; set; } = 1;
}
