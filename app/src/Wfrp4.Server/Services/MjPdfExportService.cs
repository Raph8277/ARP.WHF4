using System.Globalization;
using System.Text;
using Wfrp4.Shared.DTOs;

namespace Wfrp4.Server.Services;

public class MjPdfExportService
{
    private const double PageWidth = 593.4618;
    private const double PageHeight = 758.0332;
    private const int TemplatePixelWidth = 1237;
    private const int TemplatePixelHeight = 1580;
    private readonly IWebHostEnvironment _environment;

    public MjPdfExportService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task<(byte[] Content, string FileName)> GenerateAsync(MjPdfExportRequest request, CancellationToken ct)
    {
        var template = await File.ReadAllBytesAsync(TemplatePath("wfrp4-character-sheet-1.jpg"), ct);
        var pages = BuildPages(request);
        var fileName = $"atelier-mj-{Slug(request.Type)}-{Slug(request.Title)}.pdf";
        return (SimplePdf.Write(pages, template), fileName);
    }

    private List<string> BuildPages(MjPdfExportRequest request)
    {
        var pages = new List<string>();
        var canvas = NewPage(request.Title, pages.Count + 1);
        var y = 250;

        foreach (var section in request.Sections)
        {
            EnsureSpace(ref canvas, pages, request.Title, ref y, 120);
            canvas.TextPx(110, y, 18, section.Title, 760, "Times-Roman", isBold: true);
            y += 38;

            foreach (var line in section.Lines)
            {
                EnsureSpace(ref canvas, pages, request.Title, ref y, 42);
                canvas.TextPx(120, y, 10, line.Label, 210, "Helvetica", isBold: true);
                canvas.TextPx(330, y, 10, line.Value, 650, "Helvetica");
                y += 34;
            }

            foreach (var stat in section.Stats)
            {
                EnsureSpace(ref canvas, pages, request.Title, ref y, 180);
                canvas.TextPx(120, y, 13, stat.Name, 420, "Times-Roman", isBold: true);
                canvas.TextPx(540, y, 10, stat.Danger, 140, "Helvetica", isBold: true);
                y += 26;
                canvas.TextPx(120, y, 9, $"{stat.Role} - {stat.Origin}", 760, "Helvetica");
                y += 34;
                DrawStats(canvas, stat, y);
                y += 78;
            }

            y += 18;
        }

        pages.Add(canvas.Content);
        return pages;
    }

    private static PdfCanvas NewPage(string title, int pageNumber)
    {
        var canvas = new PdfCanvas();
        canvas.TemplateImage();
        canvas.TextCenteredPx(620, 118, 24, "ATELIER MJ", "Times-Roman", isBold: true);
        canvas.TextCenteredPx(620, 160, 13, title, "Helvetica");
        canvas.TextPx(1040, 1450, 9, $"Page {pageNumber}", 120, "Helvetica");
        return canvas;
    }

    private static void EnsureSpace(ref PdfCanvas canvas, List<string> pages, string title, ref int y, int needed)
    {
        if (y + needed <= 1410)
            return;

        pages.Add(canvas.Content);
        canvas = NewPage(title, pages.Count + 1);
        y = 250;
    }

    private static void DrawStats(PdfCanvas canvas, MjPdfStatsDto stat, int y)
    {
        var headers = new[] { "CC", "CT", "F", "E", "I", "Ag", "Dex", "Int", "FM", "Soc", "B", "M" };
        var values = new[] { stat.WS, stat.BS, stat.S, stat.T, stat.I, stat.Ag, stat.Dex, stat.Int, stat.WP, stat.Fel, stat.Wounds, stat.Move };
        var x = 120;
        for (var i = 0; i < headers.Length; i++)
        {
            canvas.TextCenteredPx(x + (i * 75), y, 9, headers[i], "Helvetica", isBold: true);
            canvas.TextCenteredPx(x + (i * 75), y + 32, 10, values[i].ToString(CultureInfo.InvariantCulture), "Helvetica");
        }
    }

    private string TemplatePath(string fileName) =>
        Path.Combine(_environment.ContentRootPath, "PdfTemplates", fileName);

    private static string Slug(string value)
    {
        var ascii = PdfText.ToAscii(value).ToLowerInvariant();
        var chars = ascii.Select(c => char.IsLetterOrDigit(c) ? c : '-').ToArray();
        return string.Join('-', new string(chars).Split('-', StringSplitOptions.RemoveEmptyEntries));
    }

    private static class SimplePdf
    {
        public static byte[] Write(IReadOnlyList<string> pages, byte[] template)
        {
            using var ms = new MemoryStream();
            var offsets = new List<long> { 0 };
            WriteAscii(ms, "%PDF-1.4\n");
            WriteObject(ms, offsets, 1, "<< /Type /Catalog /Pages 2 0 R >>");
            var pageIds = Enumerable.Range(0, pages.Count).Select(i => 7 + (i * 2)).ToList();
            WriteObject(ms, offsets, 2, $"<< /Type /Pages /Kids [{string.Join(' ', pageIds.Select(id => $"{id} 0 R"))}] /Count {pages.Count} >>");
            WriteImage(ms, offsets, 3, template);
            WriteObject(ms, offsets, 4, "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>");
            WriteObject(ms, offsets, 5, "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica-Bold >>");
            WriteObject(ms, offsets, 6, "<< /Type /Font /Subtype /Type1 /BaseFont /Times-Roman >>");

            for (var i = 0; i < pages.Count; i++)
            {
                var pageId = 7 + (i * 2);
                var contentId = pageId + 1;
                WritePage(ms, offsets, pageId, contentId);
                WriteStream(ms, offsets, contentId, pages[i]);
            }

            var xref = ms.Position;
            WriteAscii(ms, $"xref\n0 {offsets.Count}\n0000000000 65535 f \n");
            for (var i = 1; i < offsets.Count; i++)
                WriteAscii(ms, $"{offsets[i]:0000000000} 00000 n \n");
            WriteAscii(ms, $"trailer\n<< /Size {offsets.Count} /Root 1 0 R >>\nstartxref\n{xref}\n%%EOF");
            return ms.ToArray();
        }

        private static void WritePage(Stream stream, List<long> offsets, int pageId, int contentId)
        {
            offsets.Add(stream.Position);
            WriteAscii(stream,
                $"{pageId} 0 obj\n<< /Type /Page /Parent 2 0 R /MediaBox [0 0 {PageWidth.ToString(CultureInfo.InvariantCulture)} {PageHeight.ToString(CultureInfo.InvariantCulture)}] /Resources << /Font << /F1 4 0 R /F1B 5 0 R /F2 6 0 R >> /XObject << /Bg 3 0 R >> >> /Contents {contentId} 0 R >>\nendobj\n");
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

        public void TextPx(int x, int y, int size, string text, int maxWidth = 220, string font = "Helvetica", bool isBold = false)
        {
            var lineHeight = Math.Max(size + 3, 10);
            var lineY = ToPdfY(y);
            foreach (var line in Wrap(PdfText.ToAscii(text), Math.Max(1, maxWidth / Math.Max(4, size / 2))).Take(5))
            {
                Text(PxX(x), lineY, size, line, FontResource(font, isBold));
                lineY -= lineHeight;
            }
        }

        public void TextCenteredPx(int x, int y, int size, string text, string font = "Helvetica", bool isBold = false)
        {
            var ascii = PdfText.ToAscii(text);
            var estimatedWidth = ascii.Length * size * 0.45;
            Text(PxX(x) - estimatedWidth / 2, ToPdfY(y), size, ascii, FontResource(font, isBold));
        }

        private void Text(double x, double y, int size, string text, string fontResource)
        {
            _content.Append("BT /").Append(fontResource).Append(' ').Append(size).Append(" Tf ")
                .Append(x.ToString("0.##", CultureInfo.InvariantCulture)).Append(' ')
                .Append(y.ToString("0.##", CultureInfo.InvariantCulture)).Append(" Td (")
                .Append(PdfText.Escape(text)).Append(") Tj ET\n");
        }

        private static string FontResource(string font, bool isBold) =>
            font == "Times-Roman" ? "F2" : isBold ? "F1B" : "F1";

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
