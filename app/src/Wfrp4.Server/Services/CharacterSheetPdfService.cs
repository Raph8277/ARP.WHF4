using System.Globalization;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Wfrp4.Infrastructure.Data;
using Wfrp4.Infrastructure.Entities;
using Wfrp4.Shared.DTOs;
using Wfrp4.Shared.Models;

namespace Wfrp4.Server.Services;

public class CharacterSheetPdfService
{
    private const double PageWidth = 593.4618;
    private const double PageHeight = 758.0332;
    private const int TemplatePixelWidth = 1237;
    private const int TemplatePixelHeight = 1580;

    private readonly Wfrp4DbContext _db;
    private readonly IWebHostEnvironment _environment;

    public CharacterSheetPdfService(Wfrp4DbContext db, IWebHostEnvironment environment)
    {
        _db = db;
        _environment = environment;
    }

    public async Task<(byte[] Content, string FileName)> GenerateAsync(int personnageId, ClaimsPrincipal user, CancellationToken ct)
    {
        var personnage = await LoadPersonnageAsync(personnageId, ct);

        var page1Template = await LoadTemplateAsync("wfrp4-character-sheet-1.jpg", ct);
        var page2Template = await LoadTemplateAsync("wfrp4-character-sheet-2.jpg", ct);
        var layout = await LoadLayoutAsync(ct);
        var embeddedFonts = await LoadEmbeddedFontsAsync(layout, ct);
        var pdf = BuildPdf(personnage, page1Template, page2Template, layout, embeddedFonts);

        return (pdf, $"fiche-{Slug(personnage.Nom)}.pdf");
    }

    public async Task<(byte[] Content, string FileName)> GeneratePreviewAsync(
        int personnageId,
        ClaimsPrincipal user,
        PdfSheetLayoutDto layout,
        CancellationToken ct)
    {
        var personnage = await LoadPersonnageAsync(personnageId, ct);
        var page1Template = await LoadTemplateAsync("wfrp4-character-sheet-1.jpg", ct);
        var page2Template = await LoadTemplateAsync("wfrp4-character-sheet-2.jpg", ct);
        var mergedLayout = NormalizeLayout(layout);
        var embeddedFonts = await LoadEmbeddedFontsAsync(mergedLayout, ct);
        var pdf = BuildPdf(personnage, page1Template, page2Template, mergedLayout, embeddedFonts);

        return (pdf, $"fiche-{Slug(personnage.Nom)}-preview.pdf");
    }

    public async Task<PdfSheetLayoutDto> GetLayoutAsync(CancellationToken ct) => await LoadLayoutAsync(ct);

    public async Task<byte[]> GetTemplatePageAsync(int page, CancellationToken ct)
    {
        var fileName = page == 2 ? "wfrp4-character-sheet-2.jpg" : "wfrp4-character-sheet-1.jpg";
        return await LoadTemplateAsync(fileName, ct);
    }

    public async Task<IReadOnlyList<PdfSheetLayoutSummaryDto>> GetLayoutSummariesAsync(CancellationToken ct)
    {
        var active = await LoadLayoutAsync(ct);
        var summaries = new Dictionary<string, PdfSheetLayoutSummaryDto>(StringComparer.OrdinalIgnoreCase)
        {
            [LayoutKey(active.LayoutName)] = new()
            {
                Key = LayoutKey(active.LayoutName),
                Name = active.LayoutName,
                IsActive = true,
            },
        };

        var directory = LayoutDirectory();
        if (Directory.Exists(directory))
        {
            foreach (var file in Directory.EnumerateFiles(directory, "*.json"))
            {
                try
                {
                    await using var stream = File.OpenRead(file);
                    var layout = await JsonSerializer.DeserializeAsync<PdfSheetLayoutDto>(
                        stream,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true },
                        ct);
                    var name = NormalizeLayoutName(layout?.LayoutName);
                    var key = LayoutKey(name);
                    summaries[key] = new PdfSheetLayoutSummaryDto
                    {
                        Key = key,
                        Name = name,
                        IsActive = string.Equals(key, LayoutKey(active.LayoutName), StringComparison.OrdinalIgnoreCase),
                    };
                }
                catch (JsonException)
                {
                }
            }
        }

        return summaries.Values
            .OrderByDescending(s => s.IsActive)
            .ThenBy(s => s.Name)
            .ToList();
    }

    public async Task<PdfSheetLayoutDto> GetNamedLayoutAsync(string key, CancellationToken ct)
    {
        var path = NamedLayoutPath(key);
        if (!File.Exists(path))
            return await LoadLayoutAsync(ct);

        await using var stream = File.OpenRead(path);
        var layout = await JsonSerializer.DeserializeAsync<PdfSheetLayoutDto>(
            stream,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true },
            ct);

        return NormalizeLayout(layout);
    }

    public async Task SaveLayoutAsync(PdfSheetLayoutDto layout, CancellationToken ct)
    {
        var normalized = NormalizeLayout(layout);
        var json = JsonSerializer.Serialize(normalized, new JsonSerializerOptions { WriteIndented = true });
        Directory.CreateDirectory(LayoutDirectory());
        await File.WriteAllTextAsync(NamedLayoutPath(LayoutKey(normalized.LayoutName)), json, Encoding.UTF8, ct);
        await File.WriteAllTextAsync(LayoutPath(), json, Encoding.UTF8, ct);
    }

    private static PdfSheetLayoutDto NormalizeLayout(PdfSheetLayoutDto? layout)
    {
        layout ??= DefaultLayout();
        var defaultFont = NormalizeFont(layout.DefaultFont);
        var defaults = DefaultLayout().Fields.ToDictionary(f => f.Key, StringComparer.OrdinalIgnoreCase);
        var fields = layout.Fields
            .Where(f => defaults.ContainsKey(f.Key))
            .Select(f =>
            {
                var known = defaults[f.Key];
                return new PdfSheetFieldLayoutDto
                {
                    Key = known.Key,
                    Label = known.Label,
                    Page = known.Page,
                    X = Math.Clamp(f.X, 0, TemplatePixelWidth),
                    Y = Math.Clamp(f.Y, 0, TemplatePixelHeight),
                    Size = Math.Clamp(f.Size, 4, 20),
                    MaxWidth = Math.Clamp(f.MaxWidth, 20, TemplatePixelWidth),
                    Align = NormalizeAlign(f.Align),
                    Font = NormalizeOptionalFont(f.Font),
                    IsBold = f.IsBold,
                    IsItalic = f.IsItalic,
                };
            })
            .OrderBy(f => f.Page)
            .ThenBy(f => f.Y)
            .ThenBy(f => f.X)
            .ToList();

        var merged = MergeLayout(fields);
        return new PdfSheetLayoutDto
        {
            LayoutName = NormalizeLayoutName(layout.LayoutName),
            DefaultFont = defaultFont,
            RenderCharacteristicAdvances = layout.RenderCharacteristicAdvances,
            RenderCharacteristicCurrent = layout.RenderCharacteristicCurrent,
            Fields = merged,
        };
    }

    private string TemplatePath(string fileName) =>
        Path.Combine(_environment.ContentRootPath, "PdfTemplates", fileName);

    private string LayoutPath() =>
        Path.Combine(_environment.ContentRootPath, "PdfTemplates", "wfrp4-character-sheet-layout.json");

    private string LayoutDirectory() =>
        Path.Combine(_environment.ContentRootPath, "PdfTemplates", "Layouts");

    private string NamedLayoutPath(string key) =>
        Path.Combine(LayoutDirectory(), $"{LayoutKey(key)}.json");

    private string FontPath(string fileName) =>
        Path.Combine(_environment.ContentRootPath, "PdfTemplates", "Fonts", fileName);

    private async Task<Personnage> LoadPersonnageAsync(int personnageId, CancellationToken ct) =>
        await _db.Personnages
            .AsNoTracking()
            .Include(p => p.Espece)
            .Include(p => p.CarriereCourante).ThenInclude(n => n!.Carriere).ThenInclude(c => c.Classe)
            .Include(p => p.Caracteristiques)
            .Include(p => p.Competences).ThenInclude(c => c.Competence)
            .Include(p => p.Talents).ThenInclude(t => t.Talent)
            .Include(p => p.Carrieres).ThenInclude(c => c.NiveauCarriere).ThenInclude(n => n.Carriere).ThenInclude(c => c.Classe)
            .Include(p => p.Possessions)
            .Include(p => p.Sorts).ThenInclude(s => s.SortReference)
            .Include(p => p.Parchemins).ThenInclude(p => p.SortReference)
            .FirstOrDefaultAsync(p => p.Id == personnageId, ct)
            ?? throw new InvalidOperationException("Personnage introuvable.");

    private async Task<byte[]> LoadTemplateAsync(string fileName, CancellationToken ct)
    {
        var path = TemplatePath(fileName);
        if (!File.Exists(path))
            throw new FileNotFoundException($"Modele PDF officiel introuvable : {fileName}.", path);

        return await File.ReadAllBytesAsync(path, ct);
    }

    private async Task<IReadOnlyDictionary<string, EmbeddedPdfFont>> LoadEmbeddedFontsAsync(PdfSheetLayoutDto layout, CancellationToken ct)
    {
        var requestedFonts = layout.Fields
            .Select(f => f.Font)
            .Append(layout.DefaultFont)
            .Where(f => !string.IsNullOrWhiteSpace(f))
            .Select(NormalizeFont)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var fonts = new Dictionary<string, EmbeddedPdfFont>(StringComparer.OrdinalIgnoreCase);
        if (requestedFonts.Contains("Cinzel Decorative"))
        {
            var path = FontPath("CinzelDecorative-Regular.ttf");
            if (File.Exists(path))
            {
                fonts["Cinzel Decorative"] = new EmbeddedPdfFont(
                    "Cinzel Decorative",
                    "CinzelDecorative",
                    "F4",
                    await File.ReadAllBytesAsync(path, ct));
            }
        }

        return fonts;
    }

    private async Task<PdfSheetLayoutDto> LoadLayoutAsync(CancellationToken ct)
    {
        var defaults = DefaultLayout();
        var path = LayoutPath();
        if (!File.Exists(path))
            return defaults;

        try
        {
            await using var stream = File.OpenRead(path);
            var layout = await JsonSerializer.DeserializeAsync<PdfSheetLayoutDto>(
                stream,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true },
                ct);

            return NormalizeLayout(layout);
        }
        catch (JsonException)
        {
            return defaults;
        }
    }

    private static byte[] BuildPdf(
        Personnage p,
        byte[] page1Template,
        byte[] page2Template,
        PdfSheetLayoutDto layout,
        IReadOnlyDictionary<string, EmbeddedPdfFont> embeddedFonts)
    {
        var fields = layout.Fields.ToDictionary(f => f.Key, StringComparer.OrdinalIgnoreCase);
        var defaultFont = NormalizeFont(layout.DefaultFont);
        var firstCareer = p.Carrieres.OrderBy(c => c.DateEntree).FirstOrDefault();
        var currentLevel = p.CarriereCourante ?? firstCareer?.NiveauCarriere;
        var className = currentLevel?.Carriere?.Classe?.Nom ?? string.Empty;
        var careerName = currentLevel?.Carriere?.Nom ?? string.Empty;
        var echelonName = currentLevel?.Intitule ?? string.Empty;
        var status = currentLevel is null ? string.Empty : $"{currentLevel.Statut} {currentLevel.StatutNumerique}";
        var caracs = p.Caracteristiques.ToDictionary(c => c.Code, StringComparer.OrdinalIgnoreCase);

        var page1 = new PdfCanvas(embeddedFonts);
        page1.TemplateImage();

        DrawField(page1, fields, "identity.name", p.Nom, defaultFont);
        DrawField(page1, fields, "identity.race", p.Espece.Nom, defaultFont);
        DrawField(page1, fields, "identity.class", className, defaultFont);
        DrawField(page1, fields, "identity.career", careerName, defaultFont);
        DrawField(page1, fields, "identity.echelon", echelonName, defaultFont);
        DrawField(page1, fields, "identity.status", status, defaultFont);
        DrawField(page1, fields, "identity.age", p.Age?.ToString(CultureInfo.InvariantCulture) ?? string.Empty, defaultFont);
        DrawField(page1, fields, "identity.height", p.TailleCm.HasValue ? $"{p.TailleCm} cm" : string.Empty, defaultFont);
        DrawField(page1, fields, "identity.hair", p.CouleurCheveux ?? string.Empty, defaultFont);
        DrawField(page1, fields, "identity.eyes", p.CouleurYeux ?? string.Empty, defaultFont);

        var codes = new[] { "CC", "CT", "F", "E", "I", "Ag", "Dex", "Int", "FM", "Soc" };
        foreach (var code in codes)
        {
            if (!caracs.TryGetValue(code, out var c))
                continue;
            DrawField(page1, fields, $"carac.{code}.initial", c.ValeurInitiale.ToString(CultureInfo.InvariantCulture), defaultFont);
            if (layout.RenderCharacteristicAdvances)
                DrawField(page1, fields, $"carac.{code}.advance", c.Avances.ToString(CultureInfo.InvariantCulture), defaultFont);
            if (layout.RenderCharacteristicCurrent)
                DrawField(page1, fields, $"carac.{code}.current", (c.ValeurInitiale + c.Avances).ToString(CultureInfo.InvariantCulture), defaultFont);
        }

        DrawField(page1, fields, "destiny.destin", p.Destin.ToString(CultureInfo.InvariantCulture), defaultFont);
        DrawField(page1, fields, "destiny.fortune", p.Fortune.ToString(CultureInfo.InvariantCulture), defaultFont);
        DrawField(page1, fields, "resistance.resilience", p.Resilience.ToString(CultureInfo.InvariantCulture), defaultFont);
        DrawField(page1, fields, "resistance.resolution", p.Resolution.ToString(CultureInfo.InvariantCulture), defaultFont);
        DrawField(page1, fields, "resistance.motivation", p.Motivation ?? string.Empty, defaultFont);
        DrawField(page1, fields, "xp.current", (p.XpTotal - p.XpDepense).ToString(CultureInfo.InvariantCulture), defaultFont);
        DrawField(page1, fields, "xp.spent", p.XpDepense.ToString(CultureInfo.InvariantCulture), defaultFont);
        DrawField(page1, fields, "xp.total", p.XpTotal.ToString(CultureInfo.InvariantCulture), defaultFont);
        DrawField(page1, fields, "movement.base", p.Mouvement.ToString(CultureInfo.InvariantCulture), defaultFont);
        DrawField(page1, fields, "movement.walk", (p.Mouvement * 2).ToString(CultureInfo.InvariantCulture), defaultFont);
        DrawField(page1, fields, "movement.run", (p.Mouvement * 4).ToString(CultureInfo.InvariantCulture), defaultFont);

        DrawCompetences(page1, p, caracs);
        DrawTalents(page1, p, fields, defaultFont);
        DrawField(page1, fields, "ambition.short", p.AmbitionCourtTerme ?? string.Empty, defaultFont);
        DrawField(page1, fields, "ambition.long", p.AmbitionLongTerme ?? string.Empty, defaultFont);
        DrawField(page1, fields, "group.name", p.GroupeNom ?? string.Empty, defaultFont);
        DrawField(page1, fields, "group.members", p.GroupeMembres ?? string.Empty, defaultFont);

        var page2 = new PdfCanvas(embeddedFonts);
        page2.TemplateImage();
        DrawPossessions(page2, p, fields, defaultFont);
        DrawArmes(page2, p, fields, defaultFont);
        DrawSortsEtPrieres(page2, p, fields, defaultFont);
        DrawField(page2, fields, "psychology", p.Psychologie ?? string.Empty, defaultFont);
        DrawField(page2, fields, "corruption", p.CorruptionMutations ?? string.Empty, defaultFont);
        DrawField(page2, fields, "wealth.brass", p.SousCuivre.ToString(CultureInfo.InvariantCulture), defaultFont);
        DrawField(page2, fields, "wealth.silver", p.PistolesArgent.ToString(CultureInfo.InvariantCulture), defaultFont);
        DrawField(page2, fields, "wealth.gold", p.CouronnesOr.ToString(CultureInfo.InvariantCulture), defaultFont);
        DrawField(page2, fields, "wounds.max", p.BlessuresMax.ToString(CultureInfo.InvariantCulture), defaultFont);

        return OfficialSheetPdf.Write(page1.Content, page2.Content, page1Template, page2Template, embeddedFonts.Values);
    }

    private static void DrawField(PdfCanvas page, IReadOnlyDictionary<string, PdfSheetFieldLayoutDto> fields, string key, string text, string defaultFont)
    {
        if (!fields.TryGetValue(key, out var field))
            return;

        var font = field.Font ?? defaultFont;
        if (field.Align.Equals("Center", StringComparison.OrdinalIgnoreCase))
            page.TextCenteredPx(field.X, field.Y, field.Size, text, font, field.IsBold, field.IsItalic);
        else
            page.TextPx(field.X, field.Y, field.Size, text, field.MaxWidth, font, field.IsBold, field.IsItalic);
    }

    private static void DrawCompetences(PdfCanvas page, Personnage p, IReadOnlyDictionary<string, PersonnageCaracteristique> caracs)
    {
        var baseLeft = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["Art"] = 641, ["Athletisme"] = 671, ["Calme"] = 701, ["Charme"] = 731,
            ["Chevaucher"] = 761, ["Commandement"] = 791, ["Conduite d'attelage"] = 821,
            ["Corps a corps (base)"] = 851, ["Corps a corps"] = 881, ["Discretion"] = 911,
            ["Divertissement"] = 941, ["Emprise animaux"] = 971, ["Escalade"] = 1001
        };

        var baseMiddle = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["Esquive"] = 641, ["Intimidation"] = 671, ["Intuition"] = 701, ["Marchandage"] = 731,
            ["Navigation"] = 761, ["Pari"] = 791, ["Perception"] = 821, ["Ragot"] = 851,
            ["Ramer"] = 881, ["Resistance"] = 911, ["Resistance a l'alcool"] = 941,
            ["Subornation"] = 971, ["Survie en exterieur"] = 1001
        };

        var advancedY = 620;
        foreach (var pc in p.Competences.OrderBy(c => c.Competence.Nom))
        {
            var name = PdfText.ToAscii(pc.Competence.Nom);
            var total = pc.Avances + (caracs.TryGetValue(pc.Competence.Caracteristique, out var carac)
                ? carac.ValeurInitiale + carac.Avances
                : 0);

            if (baseLeft.TryGetValue(name, out var y))
            {
                page.TextCenteredPx(345, y, 8, pc.Avances.ToString(CultureInfo.InvariantCulture));
                page.TextCenteredPx(405, y, 8, total.ToString(CultureInfo.InvariantCulture));
            }
            else if (baseMiddle.TryGetValue(name, out y))
            {
                page.TextCenteredPx(745, y, 8, pc.Avances.ToString(CultureInfo.InvariantCulture));
                page.TextCenteredPx(805, y, 8, total.ToString(CultureInfo.InvariantCulture));
            }
            else if (advancedY < 1010)
            {
                page.TextPx(860, advancedY, 7, pc.Competence.Nom, 155);
                page.TextCenteredPx(1042, advancedY, 7, pc.Competence.Caracteristique);
                page.TextCenteredPx(1110, advancedY, 7, pc.Avances.ToString(CultureInfo.InvariantCulture));
                page.TextCenteredPx(1170, advancedY, 7, total.ToString(CultureInfo.InvariantCulture));
                advancedY += 30;
            }
        }
    }

    private static void DrawTalents(
        PdfCanvas page,
        Personnage p,
        IReadOnlyDictionary<string, PdfSheetFieldLayoutDto> fields,
        string defaultFont)
    {
        var index = 1;
        foreach (var talent in p.Talents.OrderBy(t => t.Talent.Nom).Take(8))
        {
            DrawField(page, fields, $"talent.{index}.name", talent.Talent.Nom, defaultFont);
            DrawField(page, fields, $"talent.{index}.count", talent.Fois.ToString(CultureInfo.InvariantCulture), defaultFont);
            DrawField(page, fields, $"talent.{index}.description", talent.Talent.Description ?? string.Empty, defaultFont);
            index++;
        }
    }

    private static void DrawPossessions(
        PdfCanvas page,
        Personnage p,
        IReadOnlyDictionary<string, PdfSheetFieldLayoutDto> fields,
        string defaultFont)
    {
        var index = 1;
        foreach (var item in p.Possessions.Where(x => x.Type == TypePossession.Objet).OrderBy(x => x.Nom).Take(15))
        {
            DrawField(page, fields, $"possession.{index}.name", item.Nom, defaultFont);
            DrawField(page, fields, $"possession.{index}.quantity", item.Quantite.ToString(CultureInfo.InvariantCulture), defaultFont);
            index++;
        }
    }

    private static void DrawArmes(
        PdfCanvas page,
        Personnage p,
        IReadOnlyDictionary<string, PdfSheetFieldLayoutDto> fields,
        string defaultFont)
    {
        var index = 1;
        foreach (var item in p.Possessions.Where(x => x.Type == TypePossession.Arme).OrderBy(x => x.Nom).Take(6))
        {
            DrawField(page, fields, $"weapon.{index}.name", item.Nom, defaultFont);
            DrawField(page, fields, $"weapon.{index}.quantity", item.Quantite.ToString(CultureInfo.InvariantCulture), defaultFont);
            index++;
        }
    }

    private static void DrawSortsEtPrieres(
        PdfCanvas page,
        Personnage p,
        IReadOnlyDictionary<string, PdfSheetFieldLayoutDto> fields,
        string defaultFont)
    {
        var lignes = p.Sorts
            .Select(s => new SortPdfLine(
                s.SortReference.Nom,
                s.SortReference.Cn,
                s.SortReference.Portee,
                s.SortReference.Cible,
                s.SortReference.Duree,
                s.SortReference.Resume))
            .Concat(p.Parchemins.Select(p => new SortPdfLine(
                p.Quantite > 1 ? $"{{#P}} {p.SortReference.Nom} (x{p.Quantite})" : $"{{#P}} {p.SortReference.Nom}",
                p.SortReference.Cn,
                p.SortReference.Portee,
                p.SortReference.Cible,
                p.SortReference.Duree,
                p.SortReference.Resume)))
            .OrderBy(l => l.Nom.StartsWith("{#P}", StringComparison.Ordinal) ? 1 : 0)
            .ThenBy(l => l.Nom, StringComparer.OrdinalIgnoreCase)
            .Take(7)
            .ToList();

        var index = 1;
        foreach (var ligne in lignes)
        {
            DrawField(page, fields, $"spell.{index}.name", ligne.Nom, defaultFont);
            DrawField(page, fields, $"spell.{index}.ni", ligne.Cn?.ToString(CultureInfo.InvariantCulture) ?? string.Empty, defaultFont);
            DrawField(page, fields, $"spell.{index}.range", ligne.Portee ?? string.Empty, defaultFont);
            DrawField(page, fields, $"spell.{index}.target", ligne.Cible ?? string.Empty, defaultFont);
            DrawField(page, fields, $"spell.{index}.duration", ligne.Duree ?? string.Empty, defaultFont);
            DrawField(page, fields, $"spell.{index}.effects", ligne.Resume ?? string.Empty, defaultFont);
            index++;
        }
    }

    private sealed record SortPdfLine(
        string Nom,
        int? Cn,
        string? Portee,
        string? Cible,
        string? Duree,
        string? Resume);

    private static string Slug(string value)
    {
        var ascii = PdfText.ToAscii(value).ToLowerInvariant();
        var chars = ascii.Select(c => char.IsLetterOrDigit(c) ? c : '-').ToArray();
        return string.Join('-', new string(chars).Split('-', StringSplitOptions.RemoveEmptyEntries));
    }

    private static string NormalizeLayoutName(string? name) =>
        string.IsNullOrWhiteSpace(name) ? "Defaut" : name.Trim();

    private static string LayoutKey(string? name)
    {
        var normalized = NormalizeLayoutName(name);
        var ascii = PdfText.ToAscii(normalized).ToLowerInvariant();
        var chars = ascii.Select(c => char.IsLetterOrDigit(c) ? c : '-').ToArray();
        return string.Join('-', new string(chars).Split('-', StringSplitOptions.RemoveEmptyEntries));
    }

    private static List<PdfSheetFieldLayoutDto> MergeLayout(IEnumerable<PdfSheetFieldLayoutDto> overrides)
    {
        var byKey = overrides.ToDictionary(f => f.Key, StringComparer.OrdinalIgnoreCase);
        return DefaultLayout().Fields
            .Select(defaultField =>
            {
                if (!byKey.TryGetValue(defaultField.Key, out var custom))
                    return defaultField;

                return new PdfSheetFieldLayoutDto
                {
                    Key = defaultField.Key,
                    Label = defaultField.Label,
                    Page = defaultField.Page,
                    X = custom.X,
                    Y = custom.Y,
                    Size = custom.Size,
                    MaxWidth = custom.MaxWidth,
                    Align = NormalizeAlign(custom.Align),
                    Font = NormalizeOptionalFont(custom.Font),
                    IsBold = custom.IsBold,
                    IsItalic = custom.IsItalic,
                };
            })
            .ToList();
    }

    private static string NormalizeAlign(string? align) =>
        string.Equals(align, "Center", StringComparison.OrdinalIgnoreCase) ? "Center" : "Left";

    private static string NormalizeFont(string? font) => font?.Trim() switch
    {
        "Inter" => "Inter",
        "Roboto" => "Roboto",
        "Open Sans" => "Open Sans",
        "Lato" => "Lato",
        "Nunito" => "Nunito",
        "Source Sans 3" => "Source Sans 3",
        "Merriweather" => "Merriweather",
        "Lora" => "Lora",
        "Cormorant Garamond" => "Cormorant Garamond",
        "Playfair Display" => "Playfair Display",
        "Cinzel" => "Cinzel",
        "EB Garamond" => "EB Garamond",
        "Cinzel Decorative" => "Cinzel Decorative",
        "MedievalSharp" => "MedievalSharp",
        "Uncial Antiqua" => "Uncial Antiqua",
        "Montserrat" => "Montserrat",
        "Oswald" => "Oswald",
        "Raleway" => "Raleway",
        "Poppins" => "Poppins",
        "Bebas Neue" => "Bebas Neue",
        "Times-Roman" => "Times-Roman",
        "Courier" => "Courier",
        _ => "Helvetica",
    };

    private static string? NormalizeOptionalFont(string? font)
    {
        if (string.IsNullOrWhiteSpace(font) || string.Equals(font, "Default", StringComparison.OrdinalIgnoreCase))
            return null;

        return NormalizeFont(font);
    }

    private static PdfSheetLayoutDto DefaultLayout()
    {
        var fields = new List<PdfSheetFieldLayoutDto>
        {
            Field("identity.name", "Identite - Nom", 1, 152, 209, 8, 360),
            Field("identity.race", "Identite - Race", 1, 690, 209, 8, 180),
            Field("identity.class", "Identite - Classe", 1, 965, 209, 8, 160),
            Field("identity.career", "Identite - Carriere", 1, 166, 239, 8, 350),
            Field("identity.echelon", "Identite - Echelon", 1, 714, 239, 8, 160),
            Field("identity.status", "Identite - Statut", 1, 965, 269, 8, 160),
            Field("identity.age", "Identite - Age", 1, 136, 299, 8, 90),
            Field("identity.height", "Identite - Taille", 1, 430, 299, 8, 140),
            Field("identity.hair", "Identite - Cheveux", 1, 710, 299, 8, 150),
            Field("identity.eyes", "Identite - Yeux", 1, 955, 299, 8, 170),

            Field("destiny.destin", "Destin - Destin", 1, 684, 389, 6, 60, "Center"),
            Field("destiny.fortune", "Destin - Chance", 1, 684, 421, 6, 60, "Center"),
            Field("resistance.resilience", "Resistance - Resilience", 1, 754, 414, 6, 60, "Center"),
            Field("resistance.resolution", "Resistance - Determination", 1, 836, 414, 6, 60, "Center"),
            Field("resistance.motivation", "Resistance - Motivation", 1, 900, 414, 5, 70),
            Field("xp.current", "Experience - Actuelle", 1, 1014, 414, 6, 60, "Center"),
            Field("xp.spent", "Experience - Depensee", 1, 1074, 414, 6, 60, "Center"),
            Field("xp.total", "Experience - Totale", 1, 1148, 414, 6, 60, "Center"),
            Field("movement.base", "Mouvement - Base", 1, 738, 518, 8, 60, "Center"),
            Field("movement.walk", "Mouvement - Marche", 1, 872, 518, 8, 60, "Center"),
            Field("movement.run", "Mouvement - Course", 1, 1044, 518, 8, 60, "Center"),

            Field("ambition.short", "Ambitions - Court terme", 1, 705, 1115, 9, 365),
            Field("ambition.long", "Ambitions - Long terme", 1, 705, 1195, 9, 365),
            Field("group.name", "Groupe - Nom", 1, 770, 1305, 9, 300),
            Field("group.members", "Groupe - Membres", 1, 735, 1470, 9, 335),

            Field("psychology", "Page 2 - Psychologie", 2, 480, 412, 8, 330),
            Field("corruption", "Page 2 - Corruption et mutations", 2, 480, 555, 8, 330),
            Field("wealth.brass", "Page 2 - Sous de cuivre", 2, 525, 707, 7, 60, "Center"),
            Field("wealth.silver", "Page 2 - Pistoles d'argent", 2, 525, 792, 7, 60, "Center"),
            Field("wealth.gold", "Page 2 - Couronnes d'or", 2, 525, 858, 7, 60, "Center"),
            Field("wounds.max", "Page 2 - Blessures max", 2, 936, 858, 7, 60, "Center"),
        };

        var codes = new[] { "CC", "CT", "F", "E", "I", "Ag", "Dex", "Int", "FM", "Soc" };
        var caracXs = new[] { 196, 235, 274, 314, 356, 396, 435, 475, 515, 554 };
        for (var i = 0; i < codes.Length; i++)
        {
            fields.Add(Field($"carac.{codes[i]}.initial", $"Caracteristiques - {codes[i]} initiale", 1, caracXs[i], 428, 8, 60, "Center"));
            fields.Add(Field($"carac.{codes[i]}.advance", $"Caracteristiques - {codes[i]} avances", 1, caracXs[i], 473, 8, 60, "Center"));
            fields.Add(Field($"carac.{codes[i]}.current", $"Caracteristiques - {codes[i]} courante", 1, caracXs[i], 518, 8, 60, "Center"));
        }

        var talentY = 1130;
        for (var i = 1; i <= 8; i++)
        {
            fields.Add(Field($"talent.{i}.name", $"Talents - Ligne {i} nom", 1, 105, talentY, 8, 170));
            fields.Add(Field($"talent.{i}.count", $"Talents - Ligne {i} prises", 1, 310, talentY, 8, 45, "Center"));
            fields.Add(Field($"talent.{i}.description", $"Talents - Ligne {i} description", 1, 355, talentY, 7, 230));
            talentY += 38;
        }

        var possessionY = 430;
        for (var i = 1; i <= 15; i++)
        {
            fields.Add(Field($"possession.{i}.name", $"Possessions - Ligne {i} nom", 2, 105, possessionY, 7, 260));
            fields.Add(Field($"possession.{i}.quantity", $"Possessions - Ligne {i} quantite", 2, 410, possessionY, 7, 45, "Center"));
            possessionY += 30;
        }

        var weaponY = 1012;
        for (var i = 1; i <= 6; i++)
        {
            fields.Add(Field($"weapon.{i}.name", $"Armes - Ligne {i} nom", 2, 105, weaponY, 7, 260));
            fields.Add(Field($"weapon.{i}.quantity", $"Armes - Ligne {i} quantite", 2, 500, weaponY, 7, 45, "Center"));
            weaponY += 30;
        }

        var spellY = 1284;
        for (var i = 1; i <= 7; i++)
        {
            fields.Add(Field($"spell.{i}.name", $"Sorts et prieres - Ligne {i} nom", 2, 105, spellY, 7, 205));
            fields.Add(Field($"spell.{i}.ni", $"Sorts et prieres - Ligne {i} NI", 2, 355, spellY, 7, 45, "Center"));
            fields.Add(Field($"spell.{i}.range", $"Sorts et prieres - Ligne {i} portee", 2, 405, spellY, 7, 85));
            fields.Add(Field($"spell.{i}.target", $"Sorts et prieres - Ligne {i} cible", 2, 498, spellY, 7, 80));
            fields.Add(Field($"spell.{i}.duration", $"Sorts et prieres - Ligne {i} duree", 2, 588, spellY, 7, 80));
            fields.Add(Field($"spell.{i}.effects", $"Sorts et prieres - Ligne {i} effets", 2, 680, spellY, 6, 380));
            spellY += 30;
        }

        return new PdfSheetLayoutDto
        {
            LayoutName = "Defaut",
            DefaultFont = "Helvetica",
            Fields = fields,
        };
    }

    private static PdfSheetFieldLayoutDto Field(
        string key,
        string label,
        int page,
        int x,
        int y,
        int size,
        int maxWidth,
        string align = "Left",
        string? font = null) =>
        new()
        {
            Key = key,
            Label = label,
            Page = page,
            X = x,
            Y = y,
            Size = size,
            MaxWidth = maxWidth,
            Align = align,
            Font = font,
        };

    private sealed record EmbeddedPdfFont(string Name, string BaseFontName, string ResourceName, byte[] Bytes);

    private static class OfficialSheetPdf
    {
        public static byte[] Write(
            string page1,
            string page2,
            byte[] page1Template,
            byte[] page2Template,
            IEnumerable<EmbeddedPdfFont> embeddedFonts)
        {
            var fonts = embeddedFonts.ToList();
            using var ms = new MemoryStream();
            var offsets = new List<long> { 0 };
            WriteAscii(ms, "%PDF-1.4\n");
            WriteObject(ms, offsets, 1, "<< /Type /Catalog /Pages 2 0 R >>");
            WriteObject(ms, offsets, 2, "<< /Type /Pages /Kids [4 0 R 7 0 R] /Count 2 >>");
            WriteObject(ms, offsets, 3, "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>");
            var fontResources = BuildFontResources(fonts);
            WritePage(ms, offsets, 4, 5, 6, fontResources);
            WriteStream(ms, offsets, 5, page1);
            WriteImage(ms, offsets, 6, page1Template);
            WritePage(ms, offsets, 7, 8, 9, fontResources);
            WriteStream(ms, offsets, 8, page2);
            WriteImage(ms, offsets, 9, page2Template);
            WriteObject(ms, offsets, 10, "<< /Type /Font /Subtype /Type1 /BaseFont /Times-Roman >>");
            WriteObject(ms, offsets, 11, "<< /Type /Font /Subtype /Type1 /BaseFont /Courier >>");
            WriteObject(ms, offsets, 12, "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica-Bold >>");
            WriteObject(ms, offsets, 13, "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica-Oblique >>");
            WriteObject(ms, offsets, 14, "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica-BoldOblique >>");
            WriteObject(ms, offsets, 15, "<< /Type /Font /Subtype /Type1 /BaseFont /Times-Bold >>");
            WriteObject(ms, offsets, 16, "<< /Type /Font /Subtype /Type1 /BaseFont /Times-Italic >>");
            WriteObject(ms, offsets, 17, "<< /Type /Font /Subtype /Type1 /BaseFont /Times-BoldItalic >>");
            WriteObject(ms, offsets, 18, "<< /Type /Font /Subtype /Type1 /BaseFont /Courier-Bold >>");
            WriteObject(ms, offsets, 19, "<< /Type /Font /Subtype /Type1 /BaseFont /Courier-Oblique >>");
            WriteObject(ms, offsets, 20, "<< /Type /Font /Subtype /Type1 /BaseFont /Courier-BoldOblique >>");

            var nextId = 21;
            foreach (var font in fonts)
            {
                var fontFileId = nextId++;
                var descriptorId = nextId++;
                var fontId = nextId++;
                WriteBinaryStream(ms, offsets, fontFileId, font.Bytes);
                WriteObject(ms, offsets, descriptorId,
                    $"<< /Type /FontDescriptor /FontName /{font.BaseFontName} /Flags 32 /Ascent 900 /Descent -250 /CapHeight 700 /ItalicAngle 0 /StemV 80 /FontBBox [-100 -250 1200 950] /FontFile2 {fontFileId} 0 R >>");
                WriteObject(ms, offsets, fontId,
                    $"<< /Type /Font /Subtype /TrueType /BaseFont /{font.BaseFontName} /Encoding /WinAnsiEncoding /FirstChar 32 /LastChar 255 /Widths [{BuildDefaultWidths()}] /FontDescriptor {descriptorId} 0 R >>");
            }

            var xref = ms.Position;
            WriteAscii(ms, $"xref\n0 {nextId}\n0000000000 65535 f \n");
            for (var i = 1; i < offsets.Count; i++)
                WriteAscii(ms, $"{offsets[i]:0000000000} 00000 n \n");
            WriteAscii(ms, $"trailer\n<< /Size {nextId} /Root 1 0 R >>\nstartxref\n{xref}\n%%EOF");
            return ms.ToArray();
        }

        private static string BuildFontResources(IReadOnlyList<EmbeddedPdfFont> fonts)
        {
            var builder = new StringBuilder(
                "/F1 3 0 R /F2 10 0 R /F3 11 0 R " +
                "/F1B 12 0 R /F1I 13 0 R /F1BI 14 0 R " +
                "/F2B 15 0 R /F2I 16 0 R /F2BI 17 0 R " +
                "/F3B 18 0 R /F3I 19 0 R /F3BI 20 0 R");
            var fontId = 23;
            foreach (var font in fonts)
            {
                builder.Append(" /").Append(font.ResourceName).Append(' ').Append(fontId).Append(" 0 R");
                fontId += 3;
            }

            return builder.ToString();
        }

        private static void WritePage(Stream stream, List<long> offsets, int pageId, int contentId, int imageId, string fontResources)
        {
            offsets.Add(stream.Position);
            WriteAscii(stream,
                $"{pageId} 0 obj\n<< /Type /Page /Parent 2 0 R /MediaBox [0 0 {PageWidth.ToString(CultureInfo.InvariantCulture)} {PageHeight.ToString(CultureInfo.InvariantCulture)}] /Resources << /Font << {fontResources} >> /XObject << /Bg {imageId} 0 R >> >> /Contents {contentId} 0 R >>\nendobj\n");
        }

        private static void WriteObject(Stream stream, List<long> offsets, int id, string body)
        {
            offsets.Add(stream.Position);
            WriteAscii(stream, $"{id} 0 obj\n{body}\nendobj\n");
        }

        private static void WriteStream(Stream stream, List<long> offsets, int id, string content)
        {
            var bytes = Encoding.ASCII.GetBytes(content);
            offsets.Add(stream.Position);
            WriteAscii(stream, $"{id} 0 obj\n<< /Length {bytes.Length} >>\nstream\n");
            stream.Write(bytes);
            WriteAscii(stream, "\nendstream\nendobj\n");
        }

        private static void WriteBinaryStream(Stream stream, List<long> offsets, int id, byte[] bytes)
        {
            offsets.Add(stream.Position);
            WriteAscii(stream, $"{id} 0 obj\n<< /Length {bytes.Length} >>\nstream\n");
            stream.Write(bytes);
            WriteAscii(stream, "\nendstream\nendobj\n");
        }

        private static void WriteImage(Stream stream, List<long> offsets, int id, byte[] imageBytes)
        {
            offsets.Add(stream.Position);
            WriteAscii(stream,
                $"{id} 0 obj\n<< /Type /XObject /Subtype /Image /Width {TemplatePixelWidth} /Height {TemplatePixelHeight} /ColorSpace /DeviceRGB /BitsPerComponent 8 /Filter /DCTDecode /Length {imageBytes.Length} >>\nstream\n");
            stream.Write(imageBytes);
            WriteAscii(stream, "\nendstream\nendobj\n");
        }

        private static void WriteAscii(Stream stream, string value)
        {
            var bytes = Encoding.ASCII.GetBytes(value);
            stream.Write(bytes);
        }

        private static string BuildDefaultWidths() =>
            string.Join(' ', Enumerable.Repeat("600", 224));
    }

    private sealed class PdfCanvas
    {
        private readonly IReadOnlyDictionary<string, EmbeddedPdfFont> _embeddedFonts;
        private readonly StringBuilder _content = new();
        public string Content => _content.ToString();

        public PdfCanvas(IReadOnlyDictionary<string, EmbeddedPdfFont> embeddedFonts)
        {
            _embeddedFonts = embeddedFonts;
        }

        public void TemplateImage()
        {
            _content.Append("q ")
                .Append(PageWidth.ToString(CultureInfo.InvariantCulture)).Append(" 0 0 ")
                .Append(PageHeight.ToString(CultureInfo.InvariantCulture)).Append(" 0 0 cm /Bg Do Q\n");
            _content.Append("0 0 0 rg\n");
        }

        public void TextPx(
            int x,
            int y,
            int size,
            string text,
            int maxWidth = 220,
            string font = "Helvetica",
            bool isBold = false,
            bool isItalic = false)
        {
            var lineHeight = Math.Max(size + 2, 9);
            var lineY = ToPdfY(y);
            foreach (var line in Wrap(PdfText.ToAscii(text), Math.Max(1, maxWidth / Math.Max(4, size / 2))).Take(4))
            {
                Text(PxX(x), lineY, size, line, font, isBold, isItalic);
                lineY -= lineHeight;
            }
        }

        public void TextCenteredPx(
            int x,
            int y,
            int size,
            string text,
            string font = "Helvetica",
            bool isBold = false,
            bool isItalic = false)
        {
            var estimatedWidth = PdfText.ToAscii(text).Length * size * 0.45;
            Text(PxX(x) - estimatedWidth / 2, ToPdfY(y), size, text, font, isBold, isItalic);
        }

        private void Text(double x, double y, int size, string text, string font, bool isBold, bool isItalic)
        {
            var escapedText = PdfText.Escape(text);
            var usesEmbeddedFont = _embeddedFonts.ContainsKey(NormalizeFont(font));
            AppendTextOperation(x, y, size, escapedText, FontResource(font, isBold, isItalic), isItalic && usesEmbeddedFont);
            if (isBold && usesEmbeddedFont)
                AppendTextOperation(x + 0.28, y, size, escapedText, FontResource(font, isBold: false, isItalic), isItalic);
        }

        private void AppendTextOperation(double x, double y, int size, string escapedText, string fontResource, bool isItalic)
        {
            _content.Append("BT /").Append(fontResource).Append(' ').Append(size).Append(" Tf ");
            if (isItalic)
            {
                _content.Append("1 0 0.22 1 ")
                    .Append(x.ToString("0.##", CultureInfo.InvariantCulture)).Append(' ')
                    .Append(y.ToString("0.##", CultureInfo.InvariantCulture)).Append(" Tm (");
            }
            else
            {
                _content.Append(x.ToString("0.##", CultureInfo.InvariantCulture)).Append(' ')
                    .Append(y.ToString("0.##", CultureInfo.InvariantCulture)).Append(" Td (");
            }

            _content
                .Append(escapedText).Append(") Tj ET\n");
        }

        private string FontResource(string font, bool isBold = false, bool isItalic = false)
        {
            var normalized = NormalizeFont(font);
            if (_embeddedFonts.TryGetValue(normalized, out var embeddedFont))
                return embeddedFont.ResourceName;

            var baseResource = normalized switch
            {
                "Merriweather" or "Lora" or "Cormorant Garamond" or "Playfair Display" or "Cinzel" or "EB Garamond"
                    or "Cinzel Decorative" or "MedievalSharp" or "Uncial Antiqua" => "F2",
                "Times-Roman" => "F2",
                "Courier" => "F3",
                _ => "F1",
            };

            return (baseResource, isBold, isItalic) switch
            {
                ("F1", true, true) => "F1BI",
                ("F1", true, false) => "F1B",
                ("F1", false, true) => "F1I",
                ("F2", true, true) => "F2BI",
                ("F2", true, false) => "F2B",
                ("F2", false, true) => "F2I",
                ("F3", true, true) => "F3BI",
                ("F3", true, false) => "F3B",
                ("F3", false, true) => "F3I",
                _ => baseResource,
            };
        }

        private static double PxX(int x) => x * PageWidth / TemplatePixelWidth;
        private static double ToPdfY(int y) => PageHeight - (y * PageHeight / TemplatePixelHeight);

        private static IEnumerable<string> Wrap(string? text, int width)
        {
            text = string.IsNullOrWhiteSpace(text) || text == "N/A" ? string.Empty : text.Trim();
            if (text.Length == 0)
                yield break;

            while (text.Length > width)
            {
                var cut = text.LastIndexOf(' ', width);
                if (cut <= 0)
                    cut = width;
                yield return text[..cut].Trim();
                text = text[cut..].Trim();
            }
            yield return text;
        }
    }

    private static class PdfText
    {
        public static string Escape(string value) =>
            ToAscii(value).Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)");

        public static string ToAscii(string value)
        {
            var normalized = value.Normalize(NormalizationForm.FormD);
            var builder = new StringBuilder(normalized.Length);
            foreach (var c in normalized)
            {
                var category = CharUnicodeInfo.GetUnicodeCategory(c);
                if (category == UnicodeCategory.NonSpacingMark)
                    continue;
                builder.Append(c is >= ' ' and <= '~' ? c : '-');
            }
            return builder.ToString();
        }
    }
}
