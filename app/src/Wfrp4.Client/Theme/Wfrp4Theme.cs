using MudBlazor;
using MudBlazor.Utilities;

namespace Wfrp4.Client.Theme;

public static class Wfrp4Theme
{
    private static Typography BuildTypography(ThemePreset preset)
    {
        var bodyStack = FontCatalog.BuildStack(preset.BodyFont, FontCatalog.BodyFonts);
        var headingStack = FontCatalog.BuildStack(preset.HeadingFont, FontCatalog.HeadingFonts);

        return new Typography
        {
            Default = new DefaultTypography { FontFamily = bodyStack },
            Body1 = new Body1Typography { FontFamily = bodyStack },
            Body2 = new Body2Typography { FontFamily = bodyStack },
            Button = new ButtonTypography { FontFamily = bodyStack },
            Caption = new CaptionTypography { FontFamily = bodyStack },
            Overline = new OverlineTypography { FontFamily = bodyStack },
            Subtitle1 = new Subtitle1Typography { FontFamily = bodyStack },
            Subtitle2 = new Subtitle2Typography { FontFamily = bodyStack },
            H1 = new H1Typography { FontFamily = headingStack },
            H2 = new H2Typography { FontFamily = headingStack },
            H3 = new H3Typography { FontFamily = headingStack },
            H4 = new H4Typography { FontFamily = headingStack },
            H5 = new H5Typography { FontFamily = headingStack },
            H6 = new H6Typography { FontFamily = headingStack },
        };
    }

    public static List<ThemePreset> BuiltInPresets { get; } =
    [
        new()
        {
            Id = "parchemin",
            Name = "Parchemin du Vieux Monde",
            IsBuiltIn = true,
            IsDarkMode = false,
            Primary = "#8b2d1f",
            Secondary = "#b08a49",
            Tertiary = "#6b4c3b",
            Info = "#8f6941",
            Success = "#7d8f57",
            Warning = "#d39a47",
            Error = "#b14536",
            Dark = "#1a1410",
            Background = "#ddd1bb",
            BackgroundGray = "#d0c4ae",
            Surface = "#efe4d0",
            AppbarBackground = "#43342b",
            AppbarText = "#e8dcc8",
            DrawerBackground = "#2d231d",
            DrawerText = "#e0d4c0",
            DrawerIcon = "#e0d4c0",
            LinesDefault = "#67584b47",
            LinesInputs = "#67584b59",
            Divider = "#67584b38",
            DividerLight = "#67584b1f",
            TextPrimary = "#2f2218",
            TextSecondary = "#67584b",
            TextDisabled = "#67584b60",
            ActionDefault = "#b08a49",
            ActionDisabled = "#67584b60",
            ActionDisabledBackground = "#67584b1a",
        },
        new()
        {
            Id = "nuit",
            Name = "Nuit de Morrslieb",
            IsBuiltIn = true,
            IsDarkMode = true,
            Primary = "#c45a4a",
            Secondary = "#d4a95c",
            Tertiary = "#9a7862",
            Info = "#c49a6c",
            Success = "#9aad6a",
            Warning = "#e0b05a",
            Error = "#d4604e",
            Dark = "#0e0b08",
            Background = "#1e1914",
            BackgroundGray = "#252019",
            Surface = "#2d251e",
            AppbarBackground = "#1e1914",
            AppbarText = "#e0d4c0",
            DrawerBackground = "#1a1510",
            DrawerText = "#e0d4c0",
            DrawerIcon = "#e0d4c0",
            LinesDefault = "#a8988038",
            LinesInputs = "#a898804d",
            Divider = "#a898802e",
            DividerLight = "#a898801a",
            TextPrimary = "#e0d4c0",
            TextSecondary = "#a89880",
            TextDisabled = "#a8988060",
            ActionDefault = "#d4a95c",
            ActionDisabled = "#a8988060",
            ActionDisabledBackground = "#a898801a",
        },
        new()
        {
            Id = "imperial",
            Name = "Bleu Impérial Azure",
            IsBuiltIn = true,
            IsDarkMode = false,
            Primary = "#1a3a5c",
            Secondary = "#c9a84c",
            Tertiary = "#5b7a8f",
            Info = "#5b8fb9",
            Success = "#4a7c59",
            Warning = "#d4953a",
            Error = "#9b3030",
            Dark = "#0e1420",
            Background = "#e8e2d8",
            BackgroundGray = "#dbd5cb",
            Surface = "#f2ede5",
            AppbarBackground = "#1a3a5c",
            AppbarText = "#e8e2d8",
            DrawerBackground = "#162e4a",
            DrawerText = "#e0dbd3",
            DrawerIcon = "#e0dbd3",
            LinesDefault = "#4a4a6038",
            LinesInputs = "#4a4a604d",
            Divider = "#4a4a6030",
            DividerLight = "#4a4a601a",
            TextPrimary = "#1a1a2e",
            TextSecondary = "#4a4a60",
            TextDisabled = "#4a4a6060",
            ActionDefault = "#c9a84c",
            ActionDisabled = "#4a4a6060",
            ActionDisabledBackground = "#4a4a601a",
        },
        new()
        {
            Id = "warpstone",
            Name = "Lueur de Malepierre",
            IsBuiltIn = true,
            IsDarkMode = true,
            Primary = "#44cc66",
            Secondary = "#88bb44",
            Tertiary = "#55aa88",
            Info = "#55aa88",
            Success = "#66dd77",
            Warning = "#ccaa33",
            Error = "#cc4444",
            Dark = "#050a05",
            Background = "#0d1a0d",
            BackgroundGray = "#142014",
            Surface = "#1a2a1a",
            AppbarBackground = "#0d1a0d",
            AppbarText = "#c8e8c8",
            DrawerBackground = "#0a140a",
            DrawerText = "#c8e8c8",
            DrawerIcon = "#c8e8c8",
            LinesDefault = "#88aa8838",
            LinesInputs = "#88aa884d",
            Divider = "#88aa882e",
            DividerLight = "#88aa881a",
            TextPrimary = "#c8e8c8",
            TextSecondary = "#88aa88",
            TextDisabled = "#88aa8860",
            ActionDefault = "#88bb44",
            ActionDisabled = "#88aa8860",
            ActionDisabledBackground = "#88aa881a",
        },
    ];

    public static ThemePreset DefaultPreset { get; } =
        BuiltInPresets.First(p => p.Id == "imperial");

    public static MudTheme Build(ThemePreset preset)
    {
        var primary = Hex(preset.Primary);
        var secondary = Hex(preset.Secondary);
        var tertiary = Hex(preset.Tertiary);
        var info = Hex(preset.Info);
        var success = Hex(preset.Success);
        var warning = Hex(preset.Warning);
        var error = Hex(preset.Error);
        var dark = Hex(preset.Dark);
        var background = Hex(preset.Background);
        var backgroundGray = Hex(preset.BackgroundGray);
        var surface = Hex(preset.Surface);
        var appbar = Hex(preset.AppbarBackground);
        var appbarText = Hex(preset.AppbarText);
        var drawer = Hex(preset.DrawerBackground);
        var drawerText = Hex(preset.DrawerText);
        var drawerIcon = Hex(preset.DrawerIcon);
        var linesDefault = Hex(preset.LinesDefault);
        var linesInputs = Hex(preset.LinesInputs);
        var divider = Hex(preset.Divider);
        var dividerLight = Hex(preset.DividerLight);
        var textPrimary = Hex(preset.TextPrimary);
        var textSecondary = Hex(preset.TextSecondary);
        var textDisabled = Hex(preset.TextDisabled);
        var actionDefault = Hex(preset.ActionDefault);
        var actionDisabled = Hex(preset.ActionDisabled);
        var actionDisabledBg = Hex(preset.ActionDisabledBackground);

        var contrastLight = ContrastTextFor(primary, false);
        var contrastDark = ContrastTextFor(primary, true);

        var borderRadius = $"{preset.DefaultBorderRadius}px";

        return new MudTheme
        {
            PaletteLight = new PaletteLight
            {
                Primary = primary,
                PrimaryContrastText = contrastLight,
                Secondary = secondary,
                SecondaryContrastText = contrastLight,
                Tertiary = tertiary,
                TertiaryContrastText = contrastLight,
                Info = info,
                InfoContrastText = contrastLight,
                Success = success,
                SuccessContrastText = contrastLight,
                Warning = warning,
                WarningContrastText = Darken(textPrimary),
                Error = error,
                ErrorContrastText = contrastLight,
                Dark = dark,
                DarkContrastText = Lighten(textPrimary),
                Background = background,
                BackgroundGray = backgroundGray,
                Surface = surface,
                DrawerBackground = drawer,
                DrawerText = drawerText,
                DrawerIcon = drawerIcon,
                AppbarBackground = appbar,
                AppbarText = appbarText,
                TextPrimary = textPrimary,
                TextSecondary = textSecondary,
                TextDisabled = textDisabled,
                ActionDefault = actionDefault,
                ActionDisabled = actionDisabled,
                ActionDisabledBackground = actionDisabledBg,
                LinesDefault = linesDefault,
                LinesInputs = linesInputs,
                TableLines = WithAlpha(textSecondary, 0.20),
                Divider = divider,
                DividerLight = dividerLight,
            },
            PaletteDark = new PaletteDark
            {
                Primary = Lighten(primary, 0.15),
                PrimaryContrastText = contrastDark,
                Secondary = Lighten(secondary, 0.10),
                SecondaryContrastText = contrastDark,
                Tertiary = Lighten(tertiary, 0.12),
                TertiaryContrastText = contrastDark,
                Info = Lighten(info, 0.15),
                InfoContrastText = contrastDark,
                Success = Lighten(success, 0.10),
                SuccessContrastText = contrastDark,
                Warning = Lighten(warning, 0.10),
                WarningContrastText = contrastDark,
                Error = Lighten(error, 0.10),
                ErrorContrastText = contrastDark,
                Dark = Darken(dark, 0.05),
                DarkContrastText = textPrimary,
                Background = background,
                BackgroundGray = backgroundGray,
                Surface = surface,
                DrawerBackground = drawer,
                DrawerText = drawerText,
                DrawerIcon = drawerIcon,
                AppbarBackground = appbar,
                AppbarText = appbarText,
                TextPrimary = textPrimary,
                TextSecondary = textSecondary,
                TextDisabled = textDisabled,
                ActionDefault = actionDefault,
                ActionDisabled = actionDisabled,
                ActionDisabledBackground = actionDisabledBg,
                LinesDefault = linesDefault,
                LinesInputs = linesInputs,
                TableLines = WithAlpha(textSecondary, 0.16),
                Divider = divider,
                DividerLight = dividerLight,
            },
            Typography = BuildTypography(preset),
            LayoutProperties = new LayoutProperties
            {
                DefaultBorderRadius = borderRadius,
            },
        };
    }

    private static MudColor Hex(string hex) => new(hex);

    private static MudColor ContrastTextFor(MudColor color, bool darkMode)
    {
        var luminance = (0.299 * color.R + 0.587 * color.G + 0.114 * color.B) / 255.0;
        return darkMode
            ? new MudColor(luminance > 0.5 ? "#1a1210" : "#e8dcc8")
            : new MudColor(luminance > 0.5 ? "#2c1c12" : "#f4e9d7");
    }

    private static MudColor Lighten(MudColor c, double amount = 0.4)
    {
        var r = (byte)Math.Min(255, c.R + (255 - c.R) * amount);
        var g = (byte)Math.Min(255, c.G + (255 - c.G) * amount);
        var b = (byte)Math.Min(255, c.B + (255 - c.B) * amount);
        return new MudColor(r, g, b, (byte)255);
    }

    private static MudColor Darken(MudColor c, double amount = 0.2)
    {
        var r = (byte)(c.R * (1 - amount));
        var g = (byte)(c.G * (1 - amount));
        var b = (byte)(c.B * (1 - amount));
        return new MudColor(r, g, b, (byte)255);
    }

    private static MudColor WithAlpha(MudColor c, double alpha)
    {
        return new MudColor(c.R, c.G, c.B, (byte)(alpha * 255));
    }
}
