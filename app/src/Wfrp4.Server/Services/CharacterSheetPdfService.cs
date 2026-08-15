using System.Globalization;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Wfrp4.Infrastructure.Data;
using Wfrp4.Infrastructure.Entities;
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
        var personnage = await _db.Personnages
            .AsNoTracking()
            .Include(p => p.Espece)
            .Include(p => p.CarriereCourante).ThenInclude(n => n!.Carriere).ThenInclude(c => c.Classe)
            .Include(p => p.Caracteristiques)
            .Include(p => p.Competences).ThenInclude(c => c.Competence)
            .Include(p => p.Talents).ThenInclude(t => t.Talent)
            .Include(p => p.Carrieres).ThenInclude(c => c.NiveauCarriere).ThenInclude(n => n.Carriere).ThenInclude(c => c.Classe)
            .Include(p => p.Possessions)
            .FirstOrDefaultAsync(p => p.Id == personnageId, ct)
            ?? throw new InvalidOperationException("Personnage introuvable.");

        var page1Template = await File.ReadAllBytesAsync(TemplatePath("wfrp4-character-sheet-1.jpg"), ct);
        var page2Template = await File.ReadAllBytesAsync(TemplatePath("wfrp4-character-sheet-2.jpg"), ct);
        var pdf = BuildPdf(personnage, page1Template, page2Template);

        return (pdf, $"fiche-{Slug(personnage.Nom)}.pdf");
    }

    private string TemplatePath(string fileName) =>
        Path.Combine(_environment.ContentRootPath, "PdfTemplates", fileName);

    private static byte[] BuildPdf(Personnage p, byte[] page1Template, byte[] page2Template)
    {
        var firstCareer = p.Carrieres.OrderBy(c => c.DateEntree).FirstOrDefault();
        var currentLevel = p.CarriereCourante ?? firstCareer?.NiveauCarriere;
        var className = currentLevel?.Carriere?.Classe?.Nom ?? string.Empty;
        var careerName = currentLevel?.Carriere?.Nom ?? string.Empty;
        var echelonName = currentLevel?.Intitule ?? string.Empty;
        var status = currentLevel is null ? string.Empty : $"{currentLevel.Statut} {currentLevel.StatutNumerique}";
        var caracs = p.Caracteristiques.ToDictionary(c => c.Code, StringComparer.OrdinalIgnoreCase);

        var page1 = new PdfCanvas();
        page1.TemplateImage();

        page1.TextPx(152, 209, 8, p.Nom, 360);
        page1.TextPx(690, 209, 8, p.Espece.Nom, 180);
        page1.TextPx(965, 209, 8, className, 160);
        page1.TextPx(166, 239, 8, careerName, 350);
        page1.TextPx(714, 239, 8, echelonName, 160);
        page1.TextPx(965, 269, 8, status, 160);
        page1.TextPx(136, 299, 8, p.Age?.ToString(CultureInfo.InvariantCulture) ?? string.Empty, 90);
        page1.TextPx(430, 299, 8, p.TailleCm.HasValue ? $"{p.TailleCm} cm" : string.Empty, 140);
        page1.TextPx(710, 299, 8, p.CouleurCheveux ?? string.Empty, 150);
        page1.TextPx(955, 299, 8, p.CouleurYeux ?? string.Empty, 170);

        var codes = new[] { "CC", "CT", "F", "E", "I", "Ag", "Dex", "Int", "FM", "Soc" };
        var caracXs = new[] { 196, 235, 274, 314, 356, 396, 435, 475, 515, 554 };
        for (var i = 0; i < codes.Length; i++)
        {
            if (!caracs.TryGetValue(codes[i], out var c))
                continue;
            page1.TextCenteredPx(caracXs[i], 428, 8, c.ValeurInitiale.ToString(CultureInfo.InvariantCulture));
            page1.TextCenteredPx(caracXs[i], 473, 8, c.Avances.ToString(CultureInfo.InvariantCulture));
            page1.TextCenteredPx(caracXs[i], 518, 8, (c.ValeurInitiale + c.Avances).ToString(CultureInfo.InvariantCulture));
        }

        page1.TextCenteredPx(684, 389, 6, p.Destin.ToString(CultureInfo.InvariantCulture));
        page1.TextCenteredPx(684, 421, 6, p.Fortune.ToString(CultureInfo.InvariantCulture));
        page1.TextCenteredPx(754, 414, 6, p.Resilience.ToString(CultureInfo.InvariantCulture));
        page1.TextCenteredPx(836, 414, 6, p.Resolution.ToString(CultureInfo.InvariantCulture));
        page1.TextPx(900, 414, 5, p.Motivation ?? string.Empty, 70);
        page1.TextCenteredPx(1014, 414, 6, (p.XpTotal - p.XpDepense).ToString(CultureInfo.InvariantCulture));
        page1.TextCenteredPx(1074, 414, 6, p.XpDepense.ToString(CultureInfo.InvariantCulture));
        page1.TextCenteredPx(1148, 414, 6, p.XpTotal.ToString(CultureInfo.InvariantCulture));
        page1.TextCenteredPx(738, 518, 8, p.Mouvement.ToString(CultureInfo.InvariantCulture));
        page1.TextCenteredPx(872, 518, 8, (p.Mouvement * 2).ToString(CultureInfo.InvariantCulture));
        page1.TextCenteredPx(1044, 518, 8, (p.Mouvement * 4).ToString(CultureInfo.InvariantCulture));

        DrawCompetences(page1, p, caracs);
        DrawTalents(page1, p);
        page1.TextPx(705, 1115, 9, p.AmbitionCourtTerme ?? string.Empty, 365);
        page1.TextPx(705, 1195, 9, p.AmbitionLongTerme ?? string.Empty, 365);
        page1.TextPx(770, 1305, 9, p.GroupeNom ?? string.Empty, 300);
        page1.TextPx(735, 1470, 9, p.GroupeMembres ?? string.Empty, 335);

        var page2 = new PdfCanvas();
        page2.TemplateImage();
        DrawPossessions(page2, p);
        DrawArmes(page2, p);
        page2.TextPx(480, 412, 8, p.Psychologie ?? string.Empty, 330);
        page2.TextPx(480, 555, 8, p.CorruptionMutations ?? string.Empty, 330);
        page2.TextCenteredPx(525, 707, 7, p.SousCuivre.ToString(CultureInfo.InvariantCulture));
        page2.TextCenteredPx(525, 792, 7, p.PistolesArgent.ToString(CultureInfo.InvariantCulture));
        page2.TextCenteredPx(525, 858, 7, p.CouronnesOr.ToString(CultureInfo.InvariantCulture));
        page2.TextCenteredPx(936, 858, 7, p.BlessuresMax.ToString(CultureInfo.InvariantCulture));

        return OfficialSheetPdf.Write(page1.Content, page2.Content, page1Template, page2Template);
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

    private static void DrawTalents(PdfCanvas page, Personnage p)
    {
        var y = 1130;
        foreach (var talent in p.Talents.OrderBy(t => t.Talent.Nom).Take(8))
        {
            page.TextPx(105, y, 8, talent.Talent.Nom, 170);
            page.TextCenteredPx(310, y, 8, talent.Fois.ToString(CultureInfo.InvariantCulture));
            page.TextPx(355, y, 7, talent.Talent.Description ?? string.Empty, 230);
            y += 38;
        }
    }

    private static void DrawPossessions(PdfCanvas page, Personnage p)
    {
        var y = 430;
        foreach (var item in p.Possessions.Where(x => x.Type == TypePossession.Objet).OrderBy(x => x.Nom).Take(15))
        {
            page.TextPx(105, y, 7, item.Nom, 260);
            page.TextCenteredPx(410, y, 7, item.Quantite.ToString(CultureInfo.InvariantCulture));
            y += 30;
        }
    }

    private static void DrawArmes(PdfCanvas page, Personnage p)
    {
        var y = 1012;
        foreach (var item in p.Possessions.Where(x => x.Type == TypePossession.Arme).OrderBy(x => x.Nom).Take(6))
        {
            page.TextPx(105, y, 7, item.Nom, 260);
            page.TextCenteredPx(500, y, 7, item.Quantite.ToString(CultureInfo.InvariantCulture));
            y += 30;
        }
    }

    private static string Slug(string value)
    {
        var ascii = PdfText.ToAscii(value).ToLowerInvariant();
        var chars = ascii.Select(c => char.IsLetterOrDigit(c) ? c : '-').ToArray();
        return string.Join('-', new string(chars).Split('-', StringSplitOptions.RemoveEmptyEntries));
    }

    private static class OfficialSheetPdf
    {
        public static byte[] Write(string page1, string page2, byte[] page1Template, byte[] page2Template)
        {
            using var ms = new MemoryStream();
            var offsets = new List<long> { 0 };
            WriteAscii(ms, "%PDF-1.4\n");
            WriteObject(ms, offsets, 1, "<< /Type /Catalog /Pages 2 0 R >>");
            WriteObject(ms, offsets, 2, "<< /Type /Pages /Kids [4 0 R 7 0 R] /Count 2 >>");
            WriteObject(ms, offsets, 3, "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>");
            WritePage(ms, offsets, 4, 5, 6);
            WriteStream(ms, offsets, 5, page1);
            WriteImage(ms, offsets, 6, page1Template);
            WritePage(ms, offsets, 7, 8, 9);
            WriteStream(ms, offsets, 8, page2);
            WriteImage(ms, offsets, 9, page2Template);

            var xref = ms.Position;
            WriteAscii(ms, "xref\n0 10\n0000000000 65535 f \n");
            for (var i = 1; i < offsets.Count; i++)
                WriteAscii(ms, $"{offsets[i]:0000000000} 00000 n \n");
            WriteAscii(ms, $"trailer\n<< /Size 10 /Root 1 0 R >>\nstartxref\n{xref}\n%%EOF");
            return ms.ToArray();
        }

        private static void WritePage(Stream stream, List<long> offsets, int pageId, int contentId, int imageId)
        {
            offsets.Add(stream.Position);
            WriteAscii(stream,
                $"{pageId} 0 obj\n<< /Type /Page /Parent 2 0 R /MediaBox [0 0 {PageWidth.ToString(CultureInfo.InvariantCulture)} {PageHeight.ToString(CultureInfo.InvariantCulture)}] /Resources << /Font << /F1 3 0 R >> /XObject << /Bg {imageId} 0 R >> >> /Contents {contentId} 0 R >>\nendobj\n");
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
    }

    private sealed class PdfCanvas
    {
        private readonly StringBuilder _content = new();
        public string Content => _content.ToString();

        public void TemplateImage()
        {
            _content.Append("q ")
                .Append(PageWidth.ToString(CultureInfo.InvariantCulture)).Append(" 0 0 ")
                .Append(PageHeight.ToString(CultureInfo.InvariantCulture)).Append(" 0 0 cm /Bg Do Q\n");
            _content.Append("0 0 0 rg\n");
        }

        public void TextPx(int x, int y, int size, string text, int maxWidth = 220)
        {
            var lineHeight = Math.Max(size + 2, 9);
            var lineY = ToPdfY(y);
            foreach (var line in Wrap(PdfText.ToAscii(text), Math.Max(1, maxWidth / Math.Max(4, size / 2))).Take(4))
            {
                Text(PxX(x), lineY, size, line);
                lineY -= lineHeight;
            }
        }

        public void TextCenteredPx(int x, int y, int size, string text)
        {
            var estimatedWidth = PdfText.ToAscii(text).Length * size * 0.45;
            Text(PxX(x) - estimatedWidth / 2, ToPdfY(y), size, text);
        }

        private void Text(double x, double y, int size, string text)
        {
            _content.Append("BT /F1 ").Append(size).Append(" Tf ")
                .Append(x.ToString("0.##", CultureInfo.InvariantCulture)).Append(' ')
                .Append(y.ToString("0.##", CultureInfo.InvariantCulture)).Append(" Td (")
                .Append(PdfText.Escape(text)).Append(") Tj ET\n");
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
