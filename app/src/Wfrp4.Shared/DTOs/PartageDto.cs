using Wfrp4.Shared.Models;

namespace Wfrp4.Shared.DTOs;

public class PartageRequest
{
    public string MjKeycloakId { get; set; } = null!;
    public PermissionPartage Permission { get; set; } = PermissionPartage.Lecture;
}

public class PartageDto
{
    public int Id { get; set; }
    public string MjKeycloakId { get; set; } = null!;
    public PermissionPartage Permission { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
}
