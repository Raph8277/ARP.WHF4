using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Wfrp4.Server.Services;
using Wfrp4.Shared.DTOs;

namespace Wfrp4.Server.Tests;

public class MjPdfValidationTests
{
    [Fact]
    public async Task Export_annule_avant_acces_au_disque()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => new MjPdfExportService(new TestEnvironment())
            .GenerateAsync(new(), cancellation.Token));
    }

    [Fact]
    public async Task Export_ordinaire_produit_un_pdf()
    {
        var environment = new TestEnvironment { ContentRootPath = Path.GetFullPath("../../../../../src/Wfrp4.Server", AppContext.BaseDirectory) };
        var request = new MjPdfExportRequest { Title = "Test de régression", Sections = [new() {
            Title = "Garde", Stats = [new() { Name = "Garde", Wounds = 12 }],
        }] };
        var result = await new MjPdfExportService(environment).GenerateAsync(request, CancellationToken.None);
        Assert.StartsWith("%PDF-1.4", System.Text.Encoding.ASCII.GetString(result.Content, 0, 8));
        Assert.EndsWith(".pdf", result.FileName);
        Assert.InRange(result.Content.Length, 100, 8 * 1024 * 1024);
    }

    [Fact]
    public async Task Aventure_longue_est_paginee_et_conserve_les_accents()
    {
        var environment = new TestEnvironment { ContentRootPath = Path.GetFullPath("../../../../../src/Wfrp4.Server", AppContext.BaseDirectory) };
        var request = new MjPdfExportRequest
        {
            Type = "aventure",
            Title = "La forêt des damnés",
            Adventure = new MjPdfAdventureDto
            {
                Title = "La forêt des damnés",
                Theme = "Mystère",
                Region = "Reikland",
                Danger = "Élevé",
                Duration = "Trois séances",
                Synopsis = string.Join(' ', Enumerable.Repeat("Une étrange lueur traverse la forêt et révèle un ancien sanctuaire.", 40)),
                Acts = Enumerable.Range(1, 18)
                    .Select(i => new MjPdfAdventureActDto
                    {
                        Title = $"Étape {i}",
                        Description = string.Join(' ', Enumerable.Repeat($"Description détaillée de l'étape {i}.", 8)),
                        Challenge = $"Défi {i}"
                    })
                    .ToList()
            }
        };

        var result = await new MjPdfExportService(environment).GenerateAsync(request, CancellationToken.None);
        var pdfText = System.Text.Encoding.Latin1.GetString(result.Content);
        WriteQaPdf("aventure-reference.pdf", result.Content);

        Assert.Contains("forêt", pdfText);
        Assert.Contains("Étape 18", pdfText);
        Assert.Contains("Page 2", pdfText);
    }

    [Fact]
    public async Task Edition_joueurs_masque_les_secrets_pnj()
    {
        var environment = new TestEnvironment { ContentRootPath = Path.GetFullPath("../../../../../src/Wfrp4.Server", AppContext.BaseDirectory) };
        var request = new MjPdfExportRequest
        {
            Type = "pnj",
            Title = "Témoin",
            Options = new() { Audience = "Players" },
            Pnjs =
            [
                new()
                {
                    Name = "Éléonore",
                    Role = "Témoin",
                    Secret = "SECRET-NE-DOIT-PAS-FUITER",
                    Stats = new() { Wounds = 11, Move = 4 }
                }
            ]
        };

        var result = await new MjPdfExportService(environment).GenerateAsync(request, CancellationToken.None);
        var pdfText = System.Text.Encoding.Latin1.GetString(result.Content);
        WriteQaPdf("pnj-joueurs-reference.pdf", result.Content);

        Assert.Contains("Éléonore", pdfText);
        Assert.DoesNotContain("SECRET-NE-DOIT-PAS-FUITER", pdfText);
    }

    [Fact]
    public async Task Edition_joueurs_utilise_le_synopsis_public_de_l_aventure()
    {
        var environment = new TestEnvironment { ContentRootPath = Path.GetFullPath("../../../../../src/Wfrp4.Server", AppContext.BaseDirectory) };
        var request = new MjPdfExportRequest
        {
            Type = "aventure",
            Title = "Les ombres du Reik",
            Options = new() { Audience = "Players" },
            Adventure = new MjPdfAdventureDto
            {
                Title = "Les ombres du Reik",
                Theme = "Intrigue",
                Region = "Reikland",
                Danger = "Moyen",
                Duration = "Court",
                Synopsis = "Des bateliers disparaissent.",
                PlayerSynopsis = "Les familles des bateliers cherchent des aventuriers assez courageux pour remonter le fleuve.",
                NarrativeSynopsis = "SECRET-MJ : le prévôt dirige la secte responsable des disparitions."
            }
        };

        var result = await new MjPdfExportService(environment).GenerateAsync(request, CancellationToken.None);
        var pdfText = System.Text.Encoding.Latin1.GetString(result.Content);

        Assert.Contains("Les familles des bateliers", pdfText);
        Assert.DoesNotContain("SECRET-MJ", pdfText);
    }

    [Fact]
    public async Task Synopsis_multiligne_conserve_un_interligne_lisible()
    {
        var environment = new TestEnvironment { ContentRootPath = Path.GetFullPath("../../../../../src/Wfrp4.Server", AppContext.BaseDirectory) };
        var request = new MjPdfExportRequest
        {
            Type = "aventure",
            Title = "Test d'interligne",
            Adventure = new MjPdfAdventureDto
            {
                Title = "Test d'interligne",
                Theme = "Intrigue",
                Region = "Reikland",
                Danger = "Moyen",
                Duration = "One-shot",
                Synopsis = "Une piste conduit les aventuriers vers un complot.",
                NarrativeSynopsis = string.Join(' ', Enumerable.Repeat("ligne-regression", 12))
            }
        };

        var result = await new MjPdfExportService(environment).GenerateAsync(request, CancellationToken.None);
        var pdfText = System.Text.Encoding.Latin1.GetString(result.Content);
        var baselines = System.Text.RegularExpressions.Regex.Matches(
                pdfText,
                @"BT /F1 10 Tf [0-9.]+ (?<y>[0-9.]+) Td \((?<text>.*?)\) Tj ET")
            .Select(match => new
            {
                Y = double.Parse(match.Groups["y"].Value, System.Globalization.CultureInfo.InvariantCulture),
                Text = match.Groups["text"].Value
            })
            .Where(line => line.Text.Contains("ligne-regression", StringComparison.Ordinal))
            .Select(line => line.Y)
            .ToList();

        Assert.True(baselines.Count >= 2);
        Assert.InRange(baselines[0] - baselines[1], 12, 14);
    }

    [Fact]
    public async Task Synopsis_narratif_conserve_les_paragraphes_sans_caracteres_inconnus()
    {
        var environment = new TestEnvironment { ContentRootPath = Path.GetFullPath("../../../../../src/Wfrp4.Server", AppContext.BaseDirectory) };
        var request = new MjPdfExportRequest
        {
            Type = "aventure",
            Title = "Test de paragraphes",
            Adventure = new MjPdfAdventureDto
            {
                Title = "Test de paragraphes",
                Theme = "Intrigue",
                Region = "Reikland",
                Danger = "Moyen",
                Duration = "One-shot",
                Synopsis = "Une piste conduit les aventuriers vers un complot.",
                NarrativeSynopsis = "Premier-paragraphe.\r\n\r\nDeuxieme-paragraphe."
            }
        };

        var result = await new MjPdfExportService(environment).GenerateAsync(request, CancellationToken.None);
        var pdfText = System.Text.Encoding.Latin1.GetString(result.Content);
        var paragraphLines = System.Text.RegularExpressions.Regex.Matches(
                pdfText,
                @"BT /F1 10 Tf [0-9.]+ (?<y>[0-9.]+) Td \((?<text>.*?)\) Tj ET")
            .Select(match => new
            {
                Y = double.Parse(match.Groups["y"].Value, System.Globalization.CultureInfo.InvariantCulture),
                Text = match.Groups["text"].Value
            })
            .Where(line => line.Text.Contains("-paragraphe", StringComparison.Ordinal))
            .ToList();

        Assert.Equal(2, paragraphLines.Count);
        Assert.InRange(paragraphLines[0].Y - paragraphLines[1].Y, 24.5, 26.5);
        Assert.DoesNotContain("Premier-paragraphe.??Deuxieme-paragraphe.", pdfText);
    }

    [Fact]
    public async Task Titre_d_aventure_ne_chevauche_pas_le_libelle_du_dossier()
    {
        var environment = new TestEnvironment { ContentRootPath = Path.GetFullPath("../../../../../src/Wfrp4.Server", AppContext.BaseDirectory) };
        var request = new MjPdfExportRequest
        {
            Type = "aventure",
            Title = "Le cri de Karak Azgaraz",
            Adventure = new MjPdfAdventureDto
            {
                Title = "Le cri de Karak Azgaraz",
                Theme = "Intrigue",
                Region = "Reikland",
                Danger = "Moyen",
                Duration = "One-shot",
                Synopsis = "Une piste conduit les aventuriers vers un complot."
            }
        };

        var result = await new MjPdfExportService(environment).GenerateAsync(request, CancellationToken.None);
        var pdfText = System.Text.Encoding.Latin1.GetString(result.Content);
        var subtitle = System.Text.RegularExpressions.Regex.Match(
            pdfText,
            @"BT /F1B 10 Tf [0-9.]+ (?<y>[0-9.]+) Td \(DOSSIER D'AVENTURE\) Tj ET");
        var title = System.Text.RegularExpressions.Regex.Match(
            pdfText,
            @"BT /F2 24 Tf [0-9.]+ (?<y>[0-9.]+) Td \(Le cri de Karak Azgaraz\) Tj ET");

        Assert.True(subtitle.Success);
        Assert.True(title.Success);
        var subtitleBaseline = double.Parse(subtitle.Groups["y"].Value, System.Globalization.CultureInfo.InvariantCulture);
        var titleBaseline = double.Parse(title.Groups["y"].Value, System.Globalization.CultureInfo.InvariantCulture);
        Assert.InRange(subtitleBaseline - titleBaseline, 28, 30);
    }

    [Fact]
    public async Task Tresor_utilise_une_mise_en_page_semantique()
    {
        var environment = new TestEnvironment { ContentRootPath = Path.GetFullPath("../../../../../src/Wfrp4.Server", AppContext.BaseDirectory) };
        var request = new MjPdfExportRequest
        {
            Type = "butin",
            Title = "Butin du bac",
            Loot = new MjPdfLootDto
            {
                Context = "Épave fluviale",
                Coins = "12 pistoles",
                Items = ["Bague gravée", "Carte détrempée"],
                MagicItem = "Miroir d'ambre",
                MagicRisk = "Attire les démons"
            }
        };

        var result = await new MjPdfExportService(environment).GenerateAsync(request, CancellationToken.None);
        var pdfText = System.Text.Encoding.Latin1.GetString(result.Content);
        WriteQaPdf("tresor-reference.pdf", result.Content);

        Assert.Contains("INVENTAIRE", pdfText);
        Assert.Contains("Bague gravée", pdfText);
        Assert.Contains("RISQUE MAGIQUE", pdfText);
    }

    [Theory]
    [InlineData("null-section")]
    [InlineData("null-line")]
    [InlineData("long-text")]
    [InlineData("color")]
    [InlineData("geometry")]
    [InlineData("lines")]
    [InlineData("profiles")]
    public void Export_refuse_les_contenus_incoherents(string variant)
    {
        var request = new MjPdfExportRequest { Sections = [new()], Layout = new() };
        switch (variant)
        {
            case "null-section": request.Sections[0] = null!; break;
            case "null-line": request.Sections[0].Lines.Add(null!); break;
            case "long-text": request.Title = new string('x', 257); break;
            case "color": request.Layout.HeaderBarColorHex = "#GGGGGG"; break;
            case "geometry": request.Layout.MarginLeft = 1000; request.Layout.MarginRight = 900; break;
            case "lines": request.Sections[0].Lines = Enumerable.Range(0, 513).Select(_ => new MjPdfLineDto()).ToList(); break;
            case "profiles": request.Sections[0].Stats = Enumerable.Range(0, 129).Select(_ => new MjPdfStatsDto()).ToList(); break;
        }
        Assert.Throws<ValidationException>(() => MjPdfRequestValidator.Validate(request));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(16)]
    [InlineData(int.MaxValue)]
    public async Task Notes_hors_limite_refusees_avant_lecture_du_template(int count)
    {
        var service = new MjPdfExportService(new TestEnvironment());
        var request = new MjPdfExportRequest { Layout = new() { NotesLineCount = count } };
        await Assert.ThrowsAsync<ValidationException>(() => service.GenerateAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task Volume_total_refuse_avant_lecture_du_template()
    {
        var request = new MjPdfExportRequest {
            Sections = Enumerable.Range(0, 65).Select(_ => new MjPdfSectionDto()).ToList(),
        };
        await Assert.ThrowsAsync<ValidationException>(() => new MjPdfExportService(new TestEnvironment())
            .GenerateAsync(request, CancellationToken.None));
    }

    private sealed class TestEnvironment : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = "Tests";
        public string EnvironmentName { get; set; } = "Testing";
        public string ContentRootPath { get; set; } = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
        public string WebRootPath { get; set; } = "";
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
    }

    private static void WriteQaPdf(string fileName, byte[] content)
    {
        var outputDirectory = Environment.GetEnvironmentVariable("WFRP4_PDF_QA_DIR");
        if (string.IsNullOrWhiteSpace(outputDirectory))
            return;
        Directory.CreateDirectory(outputDirectory);
        File.WriteAllBytes(Path.Combine(outputDirectory, fileName), content);
    }
}
