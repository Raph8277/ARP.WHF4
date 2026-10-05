using Wfrp4.Shared.Models;

namespace Wfrp4.Infrastructure.Entities;

public class Partie
{
    public int Id { get; set; }
    public string Nom { get; set; } = null!;
    public string? Description { get; set; }
    public TypePartie Type { get; set; }
    public StatutPartie Statut { get; set; }
    public string MjKeycloakId { get; set; } = null!;
    public string MjNom { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<PartieMembre> Membres { get; set; } = [];
}
