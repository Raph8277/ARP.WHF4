using System.Text;
using System.Text.Json;
using Microsoft.JSInterop;
using MudBlazor;
using MudBlazor.Utilities;

namespace Wfrp4.Client.Theme;

public sealed class UserThemeService
{
    private const string PresetsKey = "wfrp4_theme_presets";
    private const string ActiveKey = "wfrp4_theme_active";
    private const string DefaultThemeVersionKey = "wfrp4_theme_default_version";
    private const string DefaultThemeVersion = "imperial-azure-1";
    private readonly IJSRuntime _js;

    private List<ThemePreset> _userPresets = [];
    private ThemePreset _active = Wfrp4Theme.DefaultPreset;

    public UserThemeService(IJSRuntime js) => _js = js;

    public event Action? OnChange;

    public ThemePreset ActivePreset => _active;
    public bool IsDarkMode => _active.IsDarkMode;
    public MudTheme CurrentTheme { get; private set; } = Wfrp4Theme.Build(Wfrp4Theme.DefaultPreset);

    public IReadOnlyList<ThemePreset> AllPresets
    {
        get
        {
            var list = new List<ThemePreset>(Wfrp4Theme.BuiltInPresets);
            list.AddRange(_userPresets);
            return list;
        }
    }

    public async Task LoadAsync()
    {
        var presetsJson = await _js.InvokeAsync<string?>("localStorage.getItem", PresetsKey);
        if (!string.IsNullOrEmpty(presetsJson))
            _userPresets = JsonSerializer.Deserialize<List<ThemePreset>>(presetsJson) ?? [];

        var defaultThemeVersion = await _js.InvokeAsync<string?>("localStorage.getItem", DefaultThemeVersionKey);
        if (defaultThemeVersion != DefaultThemeVersion)
        {
            _active = Wfrp4Theme.DefaultPreset;
            RebuildTheme();
            await PersistActiveAsync();
            await PersistDefaultThemeVersionAsync();
            await ApplyAllAsync();
            return;
        }

        var activeId = await _js.InvokeAsync<string?>("localStorage.getItem", ActiveKey);
        var shouldPersistActive = string.IsNullOrEmpty(activeId);
        if (!string.IsNullOrEmpty(activeId))
        {
            var found = AllPresets.FirstOrDefault(p => p.Id == activeId);
            if (found != null)
            {
                _active = found;
            }
            else
            {
                _active = Wfrp4Theme.DefaultPreset;
                shouldPersistActive = true;
            }
        }

        RebuildTheme();
        if (shouldPersistActive)
            await PersistActiveAsync();

        await ApplyAllAsync();
    }

    public async Task SelectPresetAsync(string presetId)
    {
        var preset = AllPresets.FirstOrDefault(p => p.Id == presetId);
        if (preset == null) return;

        _active = preset;
        RebuildTheme();
        await PersistActiveAsync();
        await ApplyAllAsync();
        OnChange?.Invoke();
    }

    public async Task ToggleDarkModeAsync()
    {
        _active.IsDarkMode = !_active.IsDarkMode;
        RebuildTheme();
        if (!_active.IsBuiltIn)
            await PersistPresetsAsync();
        await PersistActiveAsync();
        await ApplyAllAsync();
        OnChange?.Invoke();
    }

    public async Task UpdateActivePresetAsync(ThemePreset updated)
    {
        if (_active.IsBuiltIn) return;

        var idx = _userPresets.FindIndex(p => p.Id == updated.Id);
        if (idx >= 0)
            _userPresets[idx] = updated;

        _active = updated;
        RebuildTheme();
        await PersistPresetsAsync();
        await ApplyAllAsync();
        OnChange?.Invoke();
    }

    public async Task<ThemePreset> CreatePresetAsync(ThemePreset source)
    {
        var newPreset = source.Clone();
        _userPresets.Add(newPreset);
        _active = newPreset;
        RebuildTheme();
        await PersistPresetsAsync();
        await PersistActiveAsync();
        await ApplyAllAsync();
        OnChange?.Invoke();
        return newPreset;
    }

    public async Task DuplicateActiveAsync()
    {
        await CreatePresetAsync(_active);
    }

    public async Task DeletePresetAsync(string presetId)
    {
        var preset = _userPresets.FirstOrDefault(p => p.Id == presetId);
        if (preset == null) return;

        _userPresets.Remove(preset);
        if (_active.Id == presetId)
        {
            _active = Wfrp4Theme.DefaultPreset;
            RebuildTheme();
        }

        await PersistPresetsAsync();
        await PersistActiveAsync();
        await ApplyAllAsync();
        OnChange?.Invoke();
    }

    public async Task RenamePresetAsync(string presetId, string newName)
    {
        var preset = _userPresets.FirstOrDefault(p => p.Id == presetId);
        if (preset == null) return;

        preset.Name = newName;
        await PersistPresetsAsync();
        OnChange?.Invoke();
    }

    private void RebuildTheme()
    {
        CurrentTheme = Wfrp4Theme.Build(_active);
    }

    private async Task ApplyAllAsync()
    {
        if (IsDarkMode)
            await _js.InvokeVoidAsync("eval", "document.body.classList.add('mud-theme-dark')");
        else
            await _js.InvokeVoidAsync("eval", "document.body.classList.remove('mud-theme-dark')");

        await LoadGoogleFontsAsync();
        await ApplyDynamicCssVarsAsync();
    }

    private async Task ApplyDynamicCssVarsAsync()
    {
        var p = _active;
        var dark = p.IsDarkMode;

        var bg = new MudColor(p.Background);
        var surface = new MudColor(p.Surface);
        var primary = new MudColor(p.Primary);
        var secondary = new MudColor(p.Secondary);
        var textSecondary = new MudColor(p.TextSecondary);

        var surfaceLighter = Lighten(surface, 0.06);
        var surfaceDarker = Darken(surface, 0.06);
        var bgDarker = Darken(bg, 0.08);
        var primaryLighter = Lighten(primary, 0.15);
        var primaryDarker = Darken(primary, 0.15);
        var secondaryLighter = Lighten(secondary, 0.12);
        var secondaryDarker = Darken(secondary, 0.15);

        var sb = new StringBuilder("(function(){var s=document.body.style;");

        void Set(string name, string value)
        {
            sb.Append($"s.setProperty('{name}','{value}');");
        }

        Set("--old-world-panel", Rgba(surface, 0.94));
        Set("--old-world-panel-strong", Rgba(surface, 0.98));
        Set("--old-world-paper", p.Surface);
        Set("--old-world-paper-shadow", dark ? "rgba(0,0,0,0.35)" : Rgba(Darken(bg, 0.6), 0.16));
        Set("--old-world-kicker", dark ? Hex(Lighten(secondary, 0.2)) : Hex(Darken(secondary, 0.15)));
        Set("--old-world-hero-bg",
            $"linear-gradient(135deg, {Rgba(surface, 0.98)}, {Rgba(bg, 0.95)})");
        Set("--old-world-panel-bg",
            $"linear-gradient(180deg, {Rgba(surface, 0.98)}, {Rgba(surfaceDarker, 0.96)})");
        Set("--old-world-stat-bg",
            $"linear-gradient(180deg, {Rgba(surfaceLighter, 0.99)}, {Rgba(surface, 0.97)})");
        Set("--old-world-input-bg", Rgba(dark ? surface : surfaceLighter, 0.78));
        Set("--old-world-table-bg", Rgba(surface, 0.45));
        Set("--old-world-boot-bg",
            $"linear-gradient(180deg, {Rgba(surface, 0.98)}, {Rgba(bg, 0.98)})");
        Set("--old-world-btn-primary",
            $"linear-gradient(180deg, {Hex(dark ? primaryLighter : primary)}, {Hex(dark ? primary : primaryDarker)})");
        Set("--old-world-btn-secondary",
            $"linear-gradient(180deg, {Hex(dark ? secondaryLighter : secondary)}, {Hex(dark ? secondary : secondaryDarker)})");
        Set("--old-world-hero-text", p.TextSecondary);
        Set("--old-world-card-hover-border", Rgba(secondary, 0.35));
        Set("--old-world-card-hover-shadow", dark ? "rgba(0,0,0,0.40)" : Rgba(Darken(bg, 0.6), 0.22));
        Set("--old-world-border-subtle", Rgba(secondary, dark ? 0.18 : 0.22));
        Set("--old-world-chip-bg", Rgba(surface, 0.14));
        Set("--old-world-nav-hover-bg",
            $"linear-gradient(90deg, {Rgba(primary, 0.16)}, {Rgba(secondary, 0.08)})");
        Set("--old-world-hero-glow",
            $"radial-gradient(circle, {Rgba(secondary, 0.12)}, transparent 65%)");

        var bgBase = Hex(bg);
        var bgDark = Hex(bgDarker);
        var priTint = dark ? 0.08 : 0.1;
        var secTint = dark ? 0.06 : 0.12;
        Set("--wfrp4-body-bg",
            $"radial-gradient(circle at top, {Rgba(primary, priTint)}, transparent 28%), " +
            $"radial-gradient(circle at 85% 12%, {Rgba(secondary, secTint)}, transparent 24%), " +
            $"linear-gradient(180deg, {bgBase} 0%, {bgDark} 55%, {Hex(Darken(bg, 0.12))} 100%)");

        // Font stacks
        var bodyStack = FontCatalog.BuildStack(p.BodyFont, FontCatalog.BodyFonts);
        var headingStack = FontCatalog.BuildStack(p.HeadingFont, FontCatalog.HeadingFonts);
        Set("--wfrp4-font-body", string.Join(", ", bodyStack.Select(f => f.Contains(' ') ? $"\\'{f}\\'" : f)));
        Set("--wfrp4-font-heading", string.Join(", ", headingStack.Select(f => f.Contains(' ') ? $"\\'{f}\\'" : f)));

        sb.Append("})()");
        await _js.InvokeVoidAsync("eval", sb.ToString());
    }

    private async Task PersistPresetsAsync()
    {
        var json = JsonSerializer.Serialize(_userPresets);
        await _js.InvokeVoidAsync("localStorage.setItem", PresetsKey, json);
    }

    private async Task PersistActiveAsync()
    {
        await _js.InvokeVoidAsync("localStorage.setItem", ActiveKey, _active.Id);
    }

    private async Task PersistDefaultThemeVersionAsync()
    {
        await _js.InvokeVoidAsync("localStorage.setItem", DefaultThemeVersionKey, DefaultThemeVersion);
    }

    private async Task LoadGoogleFontsAsync()
    {
        var url = FontCatalog.GoogleFontsUrl(_active.BodyFont, _active.HeadingFont);
        if (string.IsNullOrEmpty(url)) return;

        await _js.InvokeVoidAsync("eval", $$"""
            (function() {
                var id = 'wfrp4-google-fonts';
                var existing = document.getElementById(id);
                if (existing) existing.remove();
                var link = document.createElement('link');
                link.id = id;
                link.rel = 'stylesheet';
                link.href = '{{url}}';
                document.head.appendChild(link);
            })();
        """);
    }

    private static string Rgba(MudColor c, double alpha) =>
        $"rgba({c.R},{c.G},{c.B},{alpha.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)})";

    private static string Hex(MudColor c) =>
        $"#{c.R:x2}{c.G:x2}{c.B:x2}";

    private static MudColor Lighten(MudColor c, double amount = 0.4) =>
        new((byte)Math.Min(255, c.R + (255 - c.R) * amount),
            (byte)Math.Min(255, c.G + (255 - c.G) * amount),
            (byte)Math.Min(255, c.B + (255 - c.B) * amount), (byte)255);

    private static MudColor Darken(MudColor c, double amount = 0.2) =>
        new((byte)(c.R * (1 - amount)),
            (byte)(c.G * (1 - amount)),
            (byte)(c.B * (1 - amount)), (byte)255);
}
