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
        var theme = PdfTheme.For(request.Type);
        var template = await File.ReadAllBytesAsync(TemplatePath(theme.TemplateFileName), ct);
        var pages = BuildPages(request, theme);
        var fileName = $"atelier-mj-{Slug(request.Type)}-{Slug(request.Title)}.pdf";
        return (SimplePdf.Write(pages, template), fileName);
    }

    private List<string> BuildPages(MjPdfExportRequest request, PdfTheme theme)
    {
        var slug = Slug(request.Type);
        if (slug is "groupe-pnj" or "pnj")
            return BuildPnjPages(request, theme);

        var pages = new List<string>();
        var canvas = NewPage(request.Title, pages.Count + 1, theme);
        var y = theme.ContentStartY;

        foreach (var section in request.Sections)
        {
            EnsureSpace(ref canvas, pages, request.Title, theme, ref y, 120);
            canvas.TextPx(110, y, 18, section.Title, 760, "Times-Roman", isBold: true);
            y += 38;

            foreach (var line in section.Lines)
            {
                EnsureSpace(ref canvas, pages, request.Title, theme, ref y, 42);
                canvas.TextPx(120, y, 10, line.Label, 210, "Helvetica", isBold: true);
                canvas.TextPx(330, y, 10, line.Value, 650, "Helvetica");
                y += 34;
            }

            foreach (var stat in section.Stats)
            {
                EnsureSpace(ref canvas, pages, request.Title, theme, ref y, 180);
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

    private List<string> BuildPnjPages(MjPdfExportRequest request, PdfTheme theme)
    {
        var cfg = request.Layout ?? new PnjPdfLayoutConfig();
        var pages = new List<string>();
        var canvas = NewPage(request.Title, pages.Count + 1, theme);
        var y = theme.ContentStartY;
        var left = cfg.MarginLeft;
        var right = cfg.MarginRight;
        var tableW = right - left;

        foreach (var section in request.Sections)
        {
            EnsureSpace(ref canvas, pages, request.Title, theme, ref y, 50);
            if (cfg.TitleAlign == "Center")
                canvas.TextCenteredPx(left + tableW / 2, y, cfg.TitleFontSize, section.Title, "Times-Roman", isBold: true);
            else
                canvas.TextPx(cfg.TitleAlign == "Right" ? right - 10 : left, y, cfg.TitleFontSize, section.Title, 800, "Times-Roman", isBold: true);
            y += cfg.TitleFontSize + 14;

            var contextLines = section.Lines.Where(l =>
                !l.Label.Contains(" - armement") &&
                !l.Label.Contains(" - equipement") &&
                !l.Label.Contains(" - sort")).ToList();

            if (contextLines.Count > 0)
            {
                var rowH = cfg.ContextRowHeight;
                EnsureSpace(ref canvas, pages, request.Title, theme, ref y, rowH * contextLines.Count + 10);
                var (cr, cg, cb) = HexToRgb(cfg.ContextBgColorHex);
                canvas.FillRect(left, y, tableW, rowH * contextLines.Count, cr, cg, cb);
                canvas.Rect(left, y, tableW, rowH * contextLines.Count);
                var ctxPad = VCenter(rowH, cfg.ContextFontSize);
                for (var i = 0; i < contextLines.Count; i++)
                {
                    var rowY = y + i * rowH;
                    if (i > 0)
                        canvas.Line(left, rowY, right, rowY, 0.3);
                    var labelW = cfg.ContextLabelWidth;
                    if (cfg.ContextTextAlign == "Center")
                    {
                        canvas.TextCenteredPx(left + tableW / 2, rowY + ctxPad, cfg.ContextFontSize, $"{contextLines[i].Label}  {contextLines[i].Value}", "Helvetica");
                    }
                    else
                    {
                        canvas.TextPx(left + 10, rowY + ctxPad, cfg.ContextFontSize, contextLines[i].Label, labelW, "Helvetica", isBold: true);
                        canvas.TextPx(left + labelW + 10, rowY + ctxPad, cfg.ContextFontSize, contextLines[i].Value, 750, "Helvetica");
                    }
                }
                y += rowH * contextLines.Count + 12;
            }

            var equipLines = section.Lines.Where(l =>
                l.Label.Contains(" - armement") ||
                l.Label.Contains(" - equipement") ||
                l.Label.Contains(" - sort")).ToList();

            var grouped = new List<(MjPdfStatsDto Stat, int Count, List<MjPdfLineDto> Equip)>();
            var equipIdx = 0;
            foreach (var stat in section.Stats)
            {
                var memberEquip = equipLines.Skip(equipIdx).Take(3).ToList();
                equipIdx += 3;

                if (cfg.GroupIdenticalMembers)
                {
                    var existing = grouped.FirstOrDefault(g =>
                        g.Stat.Name == stat.Name && g.Stat.Role == stat.Role &&
                        g.Stat.WS == stat.WS && g.Stat.BS == stat.BS && g.Stat.S == stat.S &&
                        g.Stat.T == stat.T && g.Stat.Wounds == stat.Wounds);

                    if (existing.Stat != null)
                    {
                        grouped[grouped.IndexOf(existing)] = (existing.Stat, existing.Count + 1, existing.Equip);
                        continue;
                    }
                }

                grouped.Add((stat, 1, memberEquip));
            }

            foreach (var (stat, count, memberEquip) in grouped)
            {
                EnsureSpace(ref canvas, pages, request.Title, theme, ref y, 200);

                var nameLabel = count > 1 ? $"{stat.Name} (x{count})" : stat.Name;
                var (hr, hg, hb) = HexToRgb(cfg.HeaderBarColorHex);
                canvas.FillRect(left, y, tableW, cfg.HeaderBarHeight, hr, hg, hb);
                var (htr, htg, htb) = HexToRgb(cfg.HeaderTextColorHex);
                canvas.SetTextColor(htr, htg, htb);
                var namePad = VCenter(cfg.HeaderBarHeight, cfg.HeaderFontSize);
                var dangerFont = Math.Max(cfg.HeaderFontSize - 3, 7);
                var dangerPad = VCenter(cfg.HeaderBarHeight, dangerFont);
                if (cfg.HeaderNameAlign == "Center")
                    canvas.TextCenteredPx(left + tableW / 2, y + namePad, cfg.HeaderFontSize, nameLabel, "Times-Roman", isBold: true);
                else
                    canvas.TextPx(left + 10, y + namePad, cfg.HeaderFontSize, nameLabel, 500, "Times-Roman", isBold: true);
                canvas.TextPx(right - 180, y + dangerPad, dangerFont, stat.Danger, 160, "Helvetica", isBold: true);
                canvas.SetTextColor(0, 0, 0);
                y += cfg.HeaderBarHeight;
                canvas.TextPx(left + 10, y + 2, cfg.RoleFontSize, $"{stat.Role} - {stat.Origin}", 600, "Helvetica");
                y += cfg.RoleFontSize + 8;

                if (cfg.ShowEquipment)
                {
                    var equipNonEmpty = memberEquip.Where(l => !string.IsNullOrWhiteSpace(l.Value) && l.Value != "Aucun").ToList();
                    if (equipNonEmpty.Count > 0)
                    {
                        var eqRowH = cfg.EquipmentRowHeight;
                        canvas.Rect(left, y, tableW, eqRowH * equipNonEmpty.Count);
                        for (var i = 0; i < equipNonEmpty.Count; i++)
                        {
                            var rowY = y + i * eqRowH;
                            if (i > 0)
                                canvas.Line(left, rowY, right, rowY, 0.2);
                            var shortLabel = equipNonEmpty[i].Label;
                            var dashIdx = shortLabel.LastIndexOf(" - ", StringComparison.Ordinal);
                            if (dashIdx >= 0)
                                shortLabel = shortLabel[(dashIdx + 3)..];
                            var eqPad = VCenter(eqRowH, cfg.EquipmentFontSize);
                            canvas.TextPx(left + 10, rowY + eqPad, cfg.EquipmentFontSize, shortLabel, cfg.EquipmentLabelWidth, "Helvetica", isBold: true);
                            canvas.TextPx(left + cfg.EquipmentLabelWidth + 10, rowY + eqPad, cfg.EquipmentFontSize, equipNonEmpty[i].Value, 700, "Helvetica");
                        }
                        y += eqRowH * equipNonEmpty.Count + 4;
                    }
                }

                DrawStatsTable(canvas, stat, left, y, tableW, cfg);
                y += cfg.StatsHeaderHeight + cfg.StatsValueHeight + 10;

                if (cfg.ShowWounds)
                {
                    DrawWoundsTracker(canvas, stat, left, y, count, cfg);
                    var rowsNeeded = Math.Min(count, cfg.MaxWoundsRows);
                    y += 16 + rowsNeeded * (cfg.WoundsBoxSize + 6) + 4;
                }

                y += cfg.MemberSpacing;
            }

            if (cfg.ShowNotes)
            {
                EnsureSpace(ref canvas, pages, request.Title, theme, ref y, cfg.NotesHeight + 40);
                canvas.TextPx(left, y, cfg.NotesFontSize, "Notes de jeu", 400, "Times-Roman", isBold: true);
                y += 20;
                canvas.Rect(left, y, tableW, cfg.NotesHeight);
                var lineSpacing = cfg.NotesLineCount > 0 ? cfg.NotesHeight / (cfg.NotesLineCount + 1) : 26;
                for (var i = 1; i <= cfg.NotesLineCount; i++)
                    canvas.Line(left, y + i * lineSpacing, right, y + i * lineSpacing, 0.15);
                y += cfg.NotesHeight + 10;
            }
        }

        pages.Add(canvas.Content);
        return pages;
    }

    private static void DrawStatsTable(PdfCanvas canvas, MjPdfStatsDto stat, int left, int y, int tableW, PnjPdfLayoutConfig? cfg = null)
    {
        cfg ??= new PnjPdfLayoutConfig();
        var headers = new[] { "CC", "CT", "F", "E", "I", "Ag", "Dex", "Int", "FM", "Soc", "B", "M" };
        var values = new[] { stat.WS, stat.BS, stat.S, stat.T, stat.I, stat.Ag, stat.Dex, stat.Int, stat.WP, stat.Fel, stat.Wounds, stat.Move };
        var colW = tableW / headers.Length;
        var headerH = cfg.StatsHeaderHeight;
        var valueH = cfg.StatsValueHeight;

        var (sr, sg, sb) = HexToRgb(cfg.StatsHeaderBgColorHex);
        canvas.FillRect(left, y, tableW, headerH, sr, sg, sb);
        canvas.Rect(left, y, tableW, headerH + valueH);
        canvas.Line(left, y + headerH, left + tableW, y + headerH, 0.5);

        var headerPad = VCenter(headerH, cfg.StatsHeaderFontSize);
        var valuePad = VCenter(valueH, cfg.StatsValueFontSize);
        for (var i = 0; i < headers.Length; i++)
        {
            var colX = left + i * colW;
            if (i > 0)
                canvas.Line(colX, y, colX, y + headerH + valueH, 0.3);
            canvas.TextCenteredPx(colX + colW / 2, y + headerPad, cfg.StatsHeaderFontSize, headers[i], "Helvetica", isBold: true);
            canvas.TextCenteredPx(colX + colW / 2, y + headerH + valuePad, cfg.StatsValueFontSize, values[i].ToString(CultureInfo.InvariantCulture), "Helvetica");
        }
    }

    private static void DrawWoundsTracker(PdfCanvas canvas, MjPdfStatsDto stat, int left, int y, int count = 1, PnjPdfLayoutConfig? cfg = null)
    {
        cfg ??= new PnjPdfLayoutConfig();
        canvas.TextPx(left + 10, y + 2, 8, $"Blessures (max {stat.Wounds})", 250, "Helvetica", isBold: true);

        y += 16;
        var boxSize = cfg.WoundsBoxSize;
        var boxGap = cfg.WoundsBoxGap;
        var boxCount = Math.Min(stat.Wounds, 25);
        var startX = left + 10;
        var rowsNeeded = Math.Min(count, cfg.MaxWoundsRows);

        for (var row = 0; row < rowsNeeded; row++)
        {
            if (rowsNeeded > 1)
                canvas.TextPx(left + 10, y + row * (boxSize + 6) + 1, 7, $"#{row + 1}", 30, "Helvetica");
            var rx = rowsNeeded > 1 ? startX + 30 : startX;
            for (var i = 0; i < boxCount; i++)
            {
                var bx = rx + i * (boxSize + boxGap);
                if (bx + boxSize > left + tableWidth(cfg) - 10)
                    break;
                canvas.Rect(bx, y + row * (boxSize + 6), boxSize, boxSize, 0.3);
            }
        }
    }

    private static int tableWidth(PnjPdfLayoutConfig cfg) => cfg.MarginRight - cfg.MarginLeft;

    private static int VCenter(int cellHeight, int fontSize) =>
        (int)(cellHeight / 2.0 + 0.51 * fontSize);

    private static (double r, double g, double b) HexToRgb(string hex)
    {
        hex = hex.TrimStart('#');
        if (hex.Length != 6)
            return (0, 0, 0);
        return (
            int.Parse(hex[..2], NumberStyles.HexNumber) / 255.0,
            int.Parse(hex[2..4], NumberStyles.HexNumber) / 255.0,
            int.Parse(hex[4..6], NumberStyles.HexNumber) / 255.0);
    }

    private static PdfCanvas NewPage(string title, int pageNumber, PdfTheme theme)
    {
        var canvas = new PdfCanvas();
        canvas.TemplateImage(theme.LightText);
        if (theme.DrawHeader)
        {
            canvas.TextCenteredPx(620, 118, 24, "ATELIER MJ", "Times-Roman", isBold: true);
            canvas.TextCenteredPx(620, 160, 13, title, "Helvetica");
        }
        canvas.TextPx(1040, 1450, 9, $"Page {pageNumber}", 120, "Helvetica");
        return canvas;
    }

    private static void EnsureSpace(ref PdfCanvas canvas, List<string> pages, string title, PdfTheme theme, ref int y, int needed)
    {
        if (y + needed <= 1410)
            return;

        pages.Add(canvas.Content);
        canvas = NewPage(title, pages.Count + 1, theme);
        y = theme.ContentStartY;
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

    private sealed record PdfTheme(string TemplateFileName, bool LightText, bool DrawHeader, int ContentStartY)
    {
        public static PdfTheme For(string type)
        {
            var normalized = Slug(type);
            return normalized is "pnj" or "groupe-pnj" or "butin"
                ? new PdfTheme("wfrp4-mj-page-background.jpg", LightText: false, DrawHeader: false, ContentStartY: 315)
                : new PdfTheme("wfrp4-character-sheet-1.jpg", LightText: false, DrawHeader: true, ContentStartY: 250);
        }
    }

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

        public void TemplateImage(bool lightText)
        {
            _content.Append("q ")
                .Append(PageWidth.ToString(CultureInfo.InvariantCulture)).Append(" 0 0 ")
                .Append(PageHeight.ToString(CultureInfo.InvariantCulture)).Append(" 0 0 cm /Bg Do Q\n");
            _content.Append(lightText ? "0.92 0.9 0.82 rg\n" : "0 0 0 rg\n");
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

        public void SetTextColor(double r, double g, double b)
        {
            _content.Append(r.ToString("0.##", CultureInfo.InvariantCulture)).Append(' ')
                .Append(g.ToString("0.##", CultureInfo.InvariantCulture)).Append(' ')
                .Append(b.ToString("0.##", CultureInfo.InvariantCulture)).Append(" rg\n");
        }

        public void Line(int x1, int y1, int x2, int y2, double lineWidth = 0.5)
        {
            _content.Append(lineWidth.ToString("0.##", CultureInfo.InvariantCulture)).Append(" w ")
                .Append(PxX(x1).ToString("0.##", CultureInfo.InvariantCulture)).Append(' ')
                .Append(ToPdfY(y1).ToString("0.##", CultureInfo.InvariantCulture)).Append(" m ")
                .Append(PxX(x2).ToString("0.##", CultureInfo.InvariantCulture)).Append(' ')
                .Append(ToPdfY(y2).ToString("0.##", CultureInfo.InvariantCulture)).Append(" l S\n");
        }

        public void Rect(int x, int y, int w, int h, double lineWidth = 0.5)
        {
            _content.Append(lineWidth.ToString("0.##", CultureInfo.InvariantCulture)).Append(" w ")
                .Append(PxX(x).ToString("0.##", CultureInfo.InvariantCulture)).Append(' ')
                .Append(ToPdfY(y + h).ToString("0.##", CultureInfo.InvariantCulture)).Append(' ')
                .Append((PxX(x + w) - PxX(x)).ToString("0.##", CultureInfo.InvariantCulture)).Append(' ')
                .Append((ToPdfY(y) - ToPdfY(y + h)).ToString("0.##", CultureInfo.InvariantCulture)).Append(" re S\n");
        }

        public void FillRect(int x, int y, int w, int h, double r, double g, double b)
        {
            _content.Append("q ")
                .Append(r.ToString("0.##", CultureInfo.InvariantCulture)).Append(' ')
                .Append(g.ToString("0.##", CultureInfo.InvariantCulture)).Append(' ')
                .Append(b.ToString("0.##", CultureInfo.InvariantCulture)).Append(" rg ")
                .Append(PxX(x).ToString("0.##", CultureInfo.InvariantCulture)).Append(' ')
                .Append(ToPdfY(y + h).ToString("0.##", CultureInfo.InvariantCulture)).Append(' ')
                .Append((PxX(x + w) - PxX(x)).ToString("0.##", CultureInfo.InvariantCulture)).Append(' ')
                .Append((ToPdfY(y) - ToPdfY(y + h)).ToString("0.##", CultureInfo.InvariantCulture)).Append(" re f Q\n");
            _content.Append("0 0 0 rg\n");
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
