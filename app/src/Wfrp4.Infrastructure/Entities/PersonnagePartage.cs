using Wfrp4.Shared.Models;

namespace Wfrp4.Infrastructure.Entities;

public class PersonnagePartage
{
    public int Id { get; set; }
    public int PersonnageId { get; set; }
    public Personnage Personnage { get; set; } = null!;
    public string MjKeycloakId { get; set; } = null!;
    public PermissionPartage Permission { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
}
