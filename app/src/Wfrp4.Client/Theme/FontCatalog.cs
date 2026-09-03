namespace Wfrp4.Client.Theme;

public sealed record FontEntry(string Name, string GoogleName, string[] Fallbacks, FontCategory Category);

public enum FontCategory { Sans, Serif, Display }

public static class FontCatalog
{
    public static IReadOnlyList<FontEntry> BodyFonts { get; } =
    [
        new("Inter", "Inter", ["Segoe UI", "Helvetica Neue", "Arial", "sans-serif"], FontCategory.Sans),
        new("Roboto", "Roboto", ["Arial", "sans-serif"], FontCategory.Sans),
        new("Open Sans", "Open+Sans", ["Arial", "sans-serif"], FontCategory.Sans),
        new("Lato", "Lato", ["Helvetica", "sans-serif"], FontCategory.Sans),
        new("Nunito", "Nunito", ["Verdana", "sans-serif"], FontCategory.Sans),
        new("Source Sans 3", "Source+Sans+3", ["Arial", "sans-serif"], FontCategory.Sans),
        new("Merriweather", "Merriweather", ["Georgia", "serif"], FontCategory.Serif),
        new("Lora", "Lora", ["Georgia", "serif"], FontCategory.Serif),
    ];

    public static IReadOnlyList<FontEntry> HeadingFonts { get; } =
    [
        new("Cormorant Garamond", "Cormorant+Garamond", ["Georgia", "serif"], FontCategory.Serif),
        new("Playfair Display", "Playfair+Display", ["Georgia", "serif"], FontCategory.Serif),
        new("Cinzel", "Cinzel", ["Georgia", "serif"], FontCategory.Serif),
        new("EB Garamond", "EB+Garamond", ["Georgia", "serif"], FontCategory.Serif),
        new("Cinzel Decorative", "Cinzel+Decorative", ["Georgia", "serif"], FontCategory.Display),
        new("MedievalSharp", "MedievalSharp", ["Georgia", "serif"], FontCategory.Display),
        new("Uncial Antiqua", "Uncial+Antiqua", ["Georgia", "serif"], FontCategory.Display),
        new("Inter", "Inter", ["Segoe UI", "Helvetica Neue", "Arial", "sans-serif"], FontCategory.Sans),
        new("Montserrat", "Montserrat", ["Arial", "sans-serif"], FontCategory.Sans),
        new("Oswald", "Oswald", ["Arial Narrow", "sans-serif"], FontCategory.Sans),
        new("Raleway", "Raleway", ["Segoe UI", "sans-serif"], FontCategory.Sans),
        new("Poppins", "Poppins", ["Segoe UI", "sans-serif"], FontCategory.Sans),
        new("Bebas Neue", "Bebas+Neue", ["Impact", "sans-serif"], FontCategory.Sans),
    ];

    public static string[] BuildStack(string fontName, IReadOnlyList<FontEntry> catalog)
    {
        var entry = catalog.FirstOrDefault(f => f.Name == fontName);
        if (entry is null)
            return [fontName, "sans-serif"];
        return [entry.Name, .. entry.Fallbacks];
    }

    public static string GoogleFontsUrl(string bodyFont, string headingFont)
    {
        var families = new HashSet<string>();

        var bodyEntry = BodyFonts.FirstOrDefault(f => f.Name == bodyFont);
        if (bodyEntry is not null)
            families.Add(bodyEntry.GoogleName + ":wght@400;500;600;700");

        var headingEntry = HeadingFonts.FirstOrDefault(f => f.Name == headingFont);
        if (headingEntry is not null && headingEntry.GoogleName != bodyEntry?.GoogleName)
            families.Add(headingEntry.GoogleName + ":wght@400;600;700");

        if (families.Count == 0)
            return "";

        return "https://fonts.googleapis.com/css2?display=swap&" +
               string.Join("&", families.Select(f => "family=" + f));
    }
}
