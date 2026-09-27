using Wfrp4.Shared.Adventures;

namespace Wfrp4.Server.Tests;

public class AdventureNarrativeComposerTests
{
    private static readonly AdventureNarrativeInput Input = new(
        BaseSynopsis: "Des bateliers disparaissent le long du Reik.",
        Region: "Reikland",
        Location: "Auerswald, au bord des marais",
        Atmosphere: "Une brume lourde étouffe les berges et assourdit le courant.",
        Hook: "La barque d'un marchand revient vide au petit matin.",
        Threat: "Secte — Un culte prépare un rituel sous les eaux.",
        Twist: "Le prévôt qui engage les personnages protège secrètement le culte.",
        Climax: "Le rituel atteint son apogée tandis que le Reik sort de son lit.",
        Danger: "Moyen",
        Duration: "Court",
        Acts:
        [
            new("Les disparus", "Les personnages interrogent les familles des bateliers.", "Auerswald"),
            new("Sous les eaux", "Les indices conduisent à un sanctuaire noyé.", "Les marais d'Auerswald"),
            new("Le rituel", "Les personnages doivent libérer les prisonniers avant la crue.", "Le sanctuaire noyé")
        ],
        CampaignLink: null);

    [Fact]
    public void Compose_produit_un_synopsis_mj_narratif_et_coherent()
    {
        var result = AdventureNarrativeComposer.Compose(Input);

        Assert.Contains(Input.Region, result.GameMasterSynopsis);
        Assert.Contains(Input.Location, result.GameMasterSynopsis);
        Assert.Contains(Input.BaseSynopsis, result.GameMasterSynopsis);
        Assert.Contains(Input.Hook, result.GameMasterSynopsis);
        Assert.Contains(Input.Threat, result.GameMasterSynopsis);
        Assert.Contains(Input.Twist, result.GameMasterSynopsis);
        Assert.Contains(Input.Climax, result.GameMasterSynopsis);
        Assert.Contains("Auerswald → Les marais d'Auerswald → Le sanctuaire noyé", result.GameMasterSynopsis);
        Assert.True(result.GameMasterSynopsis.Split(Environment.NewLine + Environment.NewLine).Length >= 4);
    }

    [Fact]
    public void Compose_ne_divulgue_pas_les_secrets_dans_le_synopsis_joueurs()
    {
        var result = AdventureNarrativeComposer.Compose(Input);

        Assert.Contains(Input.Region, result.PlayerSynopsis);
        Assert.Contains(Input.BaseSynopsis, result.PlayerSynopsis);
        Assert.Contains(Input.Hook, result.PlayerSynopsis);
        Assert.DoesNotContain(Input.Threat, result.PlayerSynopsis);
        Assert.DoesNotContain("Un culte prépare un rituel sous les eaux.", result.PlayerSynopsis);
        Assert.DoesNotContain(Input.Twist, result.PlayerSynopsis);
        Assert.DoesNotContain(Input.Climax, result.PlayerSynopsis);
        Assert.True(result.PlayerSynopsis.Split(Environment.NewLine + Environment.NewLine).Length >= 3);
    }

    [Theory]
    [InlineData("Intrigue", "intérêts cachés")]
    [InlineData("Exploration", "chemins sûrs")]
    [InlineData("Combat", "rassemble déjà ses forces")]
    [InlineData("Horreur", "défient la raison")]
    [InlineData("Politique", "factions locales")]
    public void ComposePitch_adapte_la_narration_au_theme(string theme, string expectedFrame)
    {
        var pitch = AdventureNarrativeComposer.ComposePitch(
            theme,
            "Auerswald, au bord des marais");

        Assert.Contains("Auerswald", pitch);
        Assert.Contains(expectedFrame, pitch);
    }
}
