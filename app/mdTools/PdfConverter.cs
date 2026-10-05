using System.Text;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace MdTools;

public static class PdfConverter
{
    public static async Task<string> ConvertAsync(string pdfPath, IProgress<(int page, int total)>? progress = null)
        => await Task.Run(() => Convert(pdfPath, progress));

    private static string Convert(string pdfPath, IProgress<(int, int)>? progress)
    {
        var sb = new StringBuilder();
        using var doc = PdfDocument.Open(pdfPath);
        int total = doc.NumberOfPages;
        double bodyFs = DetectBodyFontSize(doc);

        for (int pg = 1; pg <= total; pg++)
        {
            progress?.Report((pg, total));
            var page = doc.GetPage(pg);
            var words = page.GetWords().ToList();

            sb.AppendLine($"<!-- Page {pg} -->");

            if (words.Count == 0) { sb.AppendLine(); continue; }

            var lines = GroupIntoLines(words);
            string? lastFmt = null;

            foreach (var line in lines)
            {
                if (line.Count == 0) continue;

                double fs = line.SelectMany(w => w.Letters)
                                .Where(l => l.FontSize > 0)
                                .Select(l => l.FontSize)
                                .DefaultIfEmpty(bodyFs)
                                .Average();

                bool isBold = line.SelectMany(w => w.Letters)
                                  .Any(l => l.Font?.IsBold == true);

                string text = string.Join(" ", line.Select(w => w.Text)).Trim();
                if (string.IsNullOrWhiteSpace(text)) continue;

                string fmt;
                if      (fs >= bodyFs * 1.9)  fmt = $"# {text}";
                else if (fs >= bodyFs * 1.45) fmt = $"## {text}";
                else if (fs >= bodyFs * 1.15) fmt = $"### {text}";
                else if (isBold)              fmt = $"**{text}**";
                else                          fmt = text;

                if (lastFmt is not null && ShouldAddBlankLine(lastFmt, fmt))
                    sb.AppendLine();

                sb.AppendLine(fmt);
                lastFmt = fmt;
            }

            sb.AppendLine();
        }

        return sb.ToString();
    }

    private static double DetectBodyFontSize(PdfDocument doc)
    {
        var sizes = new Dictionary<double, int>();
        int sample = Math.Min(doc.NumberOfPages, 10);

        for (int pg = 1; pg <= sample; pg++)
            foreach (var word in doc.GetPage(pg).GetWords())
            {
                double fs = Math.Round(
                    word.Letters.Where(l => l.FontSize > 0)
                                .Select(l => l.FontSize)
                                .DefaultIfEmpty(12).Average(), 1);
                if (fs is > 1 and < 100)
                    sizes[fs] = sizes.GetValueOrDefault(fs) + 1;
            }

        return sizes.Count > 0
            ? sizes.OrderByDescending(kv => kv.Value).First().Key
            : 10.0;
    }

    private static List<List<Word>> GroupIntoLines(List<Word> words)
    {
        var sorted = words
            .OrderByDescending(w => w.BoundingBox.Bottom)
            .ThenBy(w => w.BoundingBox.Left)
            .ToList();

        var lines = new List<List<Word>>();
        List<Word>? current = null;
        double lastY = double.MaxValue;

        foreach (var word in sorted)
        {
            double y = word.BoundingBox.Bottom;
            if (current is null || Math.Abs(y - lastY) > 3.0)
            {
                current = [];
                lines.Add(current);
                lastY = y;
            }
            current.Add(word);
        }

        return lines;
    }

    private static bool ShouldAddBlankLine(string prev, string curr)
        => prev.StartsWith('#') || curr.StartsWith('#')
        || prev.StartsWith("**") || curr.StartsWith("**");
}
