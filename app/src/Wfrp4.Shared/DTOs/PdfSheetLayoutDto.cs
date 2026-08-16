namespace Wfrp4.Shared.DTOs;

public class PdfSheetLayoutDto
{
    public string LayoutName { get; set; } = "Defaut";
    public string DefaultFont { get; set; } = "Helvetica";
    public bool RenderCharacteristicAdvances { get; set; }
    public bool RenderCharacteristicCurrent { get; set; }
    public List<PdfSheetFieldLayoutDto> Fields { get; set; } = [];
}

public class PdfSheetLayoutSummaryDto
{
    public string Key { get; set; } = null!;
    public string Name { get; set; } = null!;
    public bool IsActive { get; set; }
}

public class PdfSheetFieldLayoutDto
{
    public string Key { get; set; } = null!;
    public string Label { get; set; } = null!;
    public int Page { get; set; } = 1;
    public int X { get; set; }
    public int Y { get; set; }
    public int Size { get; set; } = 8;
    public int MaxWidth { get; set; } = 220;
    public string Align { get; set; } = "Left";
    public string? Font { get; set; }
    public bool IsBold { get; set; }
    public bool IsItalic { get; set; }
}
