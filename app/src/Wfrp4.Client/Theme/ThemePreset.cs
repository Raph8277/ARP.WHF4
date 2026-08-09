using System.Text.Json.Serialization;
using MudBlazor;

namespace Wfrp4.Client.Theme;

public sealed class ThemePreset
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N")[..8];
    public string Name { get; set; } = "Nouveau thème";
    public bool IsBuiltIn { get; set; }
    public bool IsDarkMode { get; set; }

    // --- Couleurs d'accentuation ---
    public string Primary { get; set; } = "#8b2d1f";
    public string Secondary { get; set; } = "#b08a49";
    public string Tertiary { get; set; } = "#6b4c3b";
    public string Info { get; set; } = "#8f6941";
    public string Success { get; set; } = "#7d8f57";
    public string Warning { get; set; } = "#d39a47";
    public string Error { get; set; } = "#b14536";
    public string Dark { get; set; } = "#1a1410";

    // --- Surfaces ---
    public string Background { get; set; } = "#ddd1bb";
    public string BackgroundGray { get; set; } = "#d0c4ae";
    public string Surface { get; set; } = "#efe4d0";

    // --- Composants ---
    public string AppbarBackground { get; set; } = "#43342b";
    public string AppbarText { get; set; } = "#e8dcc8";
    public string DrawerBackground { get; set; } = "#2d231d";
    public string DrawerText { get; set; } = "#e0d4c0";
    public string DrawerIcon { get; set; } = "#e0d4c0";

    // --- Lignes & diviseurs ---
    public string LinesDefault { get; set; } = "#67584b47";
    public string LinesInputs { get; set; } = "#67584b59";
    public string Divider { get; set; } = "#67584b38";
    public string DividerLight { get; set; } = "#67584b1f";

    // --- Texte & actions ---
    public string TextPrimary { get; set; } = "#2f2218";
    public string TextSecondary { get; set; } = "#67584b";
    public string TextDisabled { get; set; } = "#67584b60";
    public string ActionDefault { get; set; } = "#b08a49";
    public string ActionDisabled { get; set; } = "#67584b60";
    public string ActionDisabledBackground { get; set; } = "#67584b1a";

    // --- Polices ---
    public string BodyFont { get; set; } = "Inter";
    public string HeadingFont { get; set; } = "Cormorant Garamond";

    // --- Mise en page ---
    public int DefaultBorderRadius { get; set; } = 4;
    public int AppBarElevation { get; set; } = 0;
    public int DrawerElevation { get; set; } = 0;
    public int DefaultElevation { get; set; } = 1;

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public DrawerClipMode DrawerClipMode { get; set; } = DrawerClipMode.Always;

    public ThemePreset Clone() => new()
    {
        Id = Guid.NewGuid().ToString("N")[..8],
        Name = Name + " (copie)",
        IsBuiltIn = false,
        IsDarkMode = IsDarkMode,
        Primary = Primary,
        Secondary = Secondary,
        Tertiary = Tertiary,
        Info = Info,
        Success = Success,
        Warning = Warning,
        Error = Error,
        Dark = Dark,
        Background = Background,
        BackgroundGray = BackgroundGray,
        Surface = Surface,
        AppbarBackground = AppbarBackground,
        AppbarText = AppbarText,
        DrawerBackground = DrawerBackground,
        DrawerText = DrawerText,
        DrawerIcon = DrawerIcon,
        LinesDefault = LinesDefault,
        LinesInputs = LinesInputs,
        Divider = Divider,
        DividerLight = DividerLight,
        TextPrimary = TextPrimary,
        TextSecondary = TextSecondary,
        TextDisabled = TextDisabled,
        ActionDefault = ActionDefault,
        ActionDisabled = ActionDisabled,
        ActionDisabledBackground = ActionDisabledBackground,
        BodyFont = BodyFont,
        HeadingFont = HeadingFont,
        DefaultBorderRadius = DefaultBorderRadius,
        AppBarElevation = AppBarElevation,
        DrawerElevation = DrawerElevation,
        DefaultElevation = DefaultElevation,
        DrawerClipMode = DrawerClipMode,
    };
}
