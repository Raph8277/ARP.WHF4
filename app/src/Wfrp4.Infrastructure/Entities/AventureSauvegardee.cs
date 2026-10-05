namespace Wfrp4.Infrastructure.Entities;

public class AventureSauvegardee
{
    public int Id { get; set; }
    public string KeycloakId { get; set; } = null!;
    public string Type { get; set; } = null!;
    public string Titre { get; set; } = null!;
    public string DataJson { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
