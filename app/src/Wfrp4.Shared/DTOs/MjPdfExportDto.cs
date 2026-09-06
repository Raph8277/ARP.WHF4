namespace Wfrp4.Shared.DTOs;

public class MjPdfExportRequest
{
    public string Type { get; set; } = "pnj";
    public string Title { get; set; } = "Atelier MJ";
    public List<MjPdfSectionDto> Sections { get; set; } = [];
    public PnjPdfLayoutConfig? Layout { get; set; }
}

public class PnjPdfLayoutConfig
{
    public int MarginLeft { get; set; } = 110;
    public int MarginRight { get; set; } = 1120;
    public int TitleFontSize { get; set; } = 18;
    public int HeaderBarHeight { get; set; } = 28;
    public int HeaderFontSize { get; set; } = 12;
    public string HeaderBarColorHex { get; set; } = "#382C1F";
    public string HeaderTextColorHex { get; set; } = "#EBE0C7";
    public int ContextRowHeight { get; set; } = 24;
    public int ContextFontSize { get; set; } = 9;
    public string ContextBgColorHex { get; set; } = "#F0E8D9";
    public bool ShowEquipment { get; set; } = true;
    public int EquipmentRowHeight { get; set; } = 20;
    public int EquipmentFontSize { get; set; } = 8;
    public int StatsHeaderHeight { get; set; } = 22;
    public int StatsValueHeight { get; set; } = 26;
    public int StatsHeaderFontSize { get; set; } = 9;
    public int StatsValueFontSize { get; set; } = 11;
    public string StatsHeaderBgColorHex { get; set; } = "#E0D6C2";
    public bool ShowWounds { get; set; } = true;
    public int WoundsBoxSize { get; set; } = 16;
    public int WoundsBoxGap { get; set; } = 3;
    public int MaxWoundsRows { get; set; } = 4;
    public bool ShowNotes { get; set; } = true;
    public int NotesHeight { get; set; } = 140;
    public int NotesLineCount { get; set; } = 5;
    public int NotesFontSize { get; set; } = 11;
    public int MemberSpacing { get; set; } = 12;
    public bool GroupIdenticalMembers { get; set; } = true;
    public int RoleFontSize { get; set; } = 8;
    public int EquipmentLabelWidth { get; set; } = 160;
    public string TitleAlign { get; set; } = "Left";
    public string HeaderNameAlign { get; set; } = "Left";
    public string ContextTextAlign { get; set; } = "Left";
    public int ContextLabelWidth { get; set; } = 200;
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
