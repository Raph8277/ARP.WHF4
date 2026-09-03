namespace Wfrp4.Shared.DTOs;

public class MjPdfExportRequest
{
    public string Type { get; set; } = "pnj";
    public string Title { get; set; } = "Atelier MJ";
    public List<MjPdfSectionDto> Sections { get; set; } = [];
}

public class MjPdfSectionDto
{
    public string Title { get; set; } = string.Empty;
    public List<MjPdfLineDto> Lines { get; set; } = [];
    public List<MjPdfStatsDto> Stats { get; set; } = [];
}

public class MjPdfLineDto
{
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

public class MjPdfStatsDto
{
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Origin { get; set; } = string.Empty;
    public string Danger { get; set; } = string.Empty;
    public int WS { get; set; }
    public int BS { get; set; }
    public int S { get; set; }
    public int T { get; set; }
    public int I { get; set; }
    public int Ag { get; set; }
    public int Dex { get; set; }
    public int Int { get; set; }
    public int WP { get; set; }
    public int Fel { get; set; }
    public int Wounds { get; set; }
    public int Move { get; set; }
}
