using System.ComponentModel.DataAnnotations;

namespace Wfrp4.Shared.DTOs;

public class MjPdfExportRequest
{
    public string Type { get; set; } = "pnj";
    public string Title { get; set; } = "Atelier MJ";
    public List<MjPdfSectionDto> Sections { get; set; } = [];
    public PnjPdfLayoutConfig? Layout { get; set; }
    public MjPdfExportOptions Options { get; set; } = new();
    public List<MjPdfPnjDto> Pnjs { get; set; } = [];
    public MjPdfLootDto? Loot { get; set; }
    public MjPdfAdventureDto? Adventure { get; set; }
    public MjPdfCampaignDto? Campaign { get; set; }
}

public class MjPdfExportOptions
{
    [Required, RegularExpression("^(GameMaster|Players)$")]
    public string Audience { get; set; } = "GameMaster";

    [Required, RegularExpression("^(Compact|Standard|Detailed)$")]
    public string Density { get; set; } = "Standard";

    public bool IncludeNotes { get; set; } = true;
}

public class MjPdfPnjDto
{
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Profession { get; set; } = string.Empty;
    public string Origin { get; set; } = string.Empty;
    public string Danger { get; set; } = string.Empty;
    public string Appearance { get; set; } = string.Empty;
    public string Manner { get; set; } = string.Empty;
    public string Motivation { get; set; } = string.Empty;
    public string Secret { get; set; } = string.Empty;
    public string Hook { get; set; } = string.Empty;
    public string Profile { get; set; } = string.Empty;
    public string Weapon { get; set; } = string.Empty;
    public string Equipment { get; set; } = string.Empty;
    public string Spell { get; set; } = string.Empty;
    public MjPdfStatsDto Stats { get; set; } = new();
}

public class MjPdfLootDto
{
    public string Context { get; set; } = string.Empty;
    public string Nature { get; set; } = string.Empty;
    public string Danger { get; set; } = string.Empty;
    public string Coins { get; set; } = string.Empty;
    public List<string> Items { get; set; } = [];
    public string UsefulItem { get; set; } = string.Empty;
    public string MagicItem { get; set; } = string.Empty;
    public string MagicEffect { get; set; } = string.Empty;
    public string MagicRisk { get; set; } = string.Empty;
    public string ValueEstimate { get; set; } = string.Empty;
    public string Owner { get; set; } = string.Empty;
    public string Condition { get; set; } = string.Empty;
    public string Clue { get; set; } = string.Empty;
    public string Complication { get; set; } = string.Empty;
}

public class MjPdfAdventureDto
{
    public string Title { get; set; } = string.Empty;
    public string Theme { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public string Danger { get; set; } = string.Empty;
    public string Duration { get; set; } = string.Empty;
    public string Synopsis { get; set; } = string.Empty;
    public string NarrativeSynopsis { get; set; } = string.Empty;
    public string PlayerSynopsis { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Hook { get; set; } = string.Empty;
    public string Threat { get; set; } = string.Empty;
    public string Atmosphere { get; set; } = string.Empty;
    public string Twist { get; set; } = string.Empty;
    public string Climax { get; set; } = string.Empty;
    public string? CampaignLink { get; set; }
    public List<MjPdfPnjDto> KeyNpcs { get; set; } = [];
    public List<MjPdfAdventureActDto> Acts { get; set; } = [];
    public List<string> Resolutions { get; set; } = [];
    public List<string> Rewards { get; set; } = [];
}

public class MjPdfAdventureActDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Challenge { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
}

public class MjPdfCampaignDto
{
    public string ArcSummary { get; set; } = string.Empty;
    public string RecurringVillain { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public List<MjPdfAdventureDto> Episodes { get; set; } = [];
}

public class PnjPdfLayoutConfig
{
    [Range(0, 1000)]
    public int MarginLeft { get; set; } = 110;
    [Range(200, 1237)]
    public int MarginRight { get; set; } = 1120;
    [Range(10, 30)]
    public int TitleFontSize { get; set; } = 18;
    [Range(16, 60)]
    public int HeaderBarHeight { get; set; } = 28;
    [Range(6, 20)]
    public int HeaderFontSize { get; set; } = 12;
    [Required, RegularExpression("^#[0-9a-fA-F]{6}$")]
    public string HeaderBarColorHex { get; set; } = "#382C1F";
    [Required, RegularExpression("^#[0-9a-fA-F]{6}$")]
    public string HeaderTextColorHex { get; set; } = "#EBE0C7";
    [Range(16, 40)]
    public int ContextRowHeight { get; set; } = 24;
    [Range(6, 14)]
    public int ContextFontSize { get; set; } = 9;
    [Required, RegularExpression("^#[0-9a-fA-F]{6}$")]
    public string ContextBgColorHex { get; set; } = "#F0E8D9";
    public bool ShowEquipment { get; set; } = true;
    [Range(12, 36)]
    public int EquipmentRowHeight { get; set; } = 20;
    [Range(5, 12)]
    public int EquipmentFontSize { get; set; } = 8;
    [Range(14, 40)]
    public int StatsHeaderHeight { get; set; } = 22;
    [Range(14, 50)]
    public int StatsValueHeight { get; set; } = 26;
    [Range(5, 14)]
    public int StatsHeaderFontSize { get; set; } = 9;
    [Range(6, 18)]
    public int StatsValueFontSize { get; set; } = 11;
    [Required, RegularExpression("^#[0-9a-fA-F]{6}$")]
    public string StatsHeaderBgColorHex { get; set; } = "#E0D6C2";
    public bool ShowWounds { get; set; } = true;
    [Range(8, 28)]
    public int WoundsBoxSize { get; set; } = 16;
    [Range(1, 8)]
    public int WoundsBoxGap { get; set; } = 3;
    [Range(1, 8)]
    public int MaxWoundsRows { get; set; } = 4;
    public bool ShowNotes { get; set; } = true;
    [Range(40, 400)]
    public int NotesHeight { get; set; } = 140;
    [Range(0, 15)]
    public int NotesLineCount { get; set; } = 5;
    [Range(6, 16)]
    public int NotesFontSize { get; set; } = 11;
    [Range(0, 40)]
    public int MemberSpacing { get; set; } = 12;
    public bool GroupIdenticalMembers { get; set; } = true;
    [Range(5, 14)]
    public int RoleFontSize { get; set; } = 8;
    [Range(80, 300)]
    public int EquipmentLabelWidth { get; set; } = 160;
    [Required, RegularExpression("^(Left|Center|Right)$")]
    public string TitleAlign { get; set; } = "Left";
    [Required, RegularExpression("^(Left|Center|Right)$")]
    public string HeaderNameAlign { get; set; } = "Left";
    [Required, RegularExpression("^(Left|Center|Right)$")]
    public string ContextTextAlign { get; set; } = "Left";
    [Range(80, 400)]
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
