namespace Wfrp4.Shared.DTOs;

public class AventureSauvegardeeSummaryDto
{
    public int Id { get; set; }
    public string Type { get; set; } = "";
    public string Titre { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class AventureSauvegardeeDetailDto
{
    public int Id { get; set; }
    public string Type { get; set; } = "";
    public string Titre { get; set; } = "";
    public string DataJson { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class SaveAventureRequest
{
    public string Type { get; set; } = "";
    public string Titre { get; set; } = "";
    public string DataJson { get; set; } = "";
}
