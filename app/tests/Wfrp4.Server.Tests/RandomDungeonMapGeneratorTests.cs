using Wfrp4.Shared.Maps;

namespace Wfrp4.Server.Tests;

public class RandomDungeonMapGeneratorTests
{
    [Theory]
    [InlineData(RandomMapKind.Dungeon)]
    [InlineData(RandomMapKind.Cave)]
    [InlineData(RandomMapKind.Inn)]
    [InlineData(RandomMapKind.Village)]
    public void Generation_est_deterministe_et_respecte_le_nombre_de_zones(RandomMapKind kind)
    {
        var first = RandomDungeonMapGenerator.Generate(kind, 18, 2, 1789);
        var second = RandomDungeonMapGenerator.Generate(kind, 18, 2, 1789);

        Assert.Equal(18, first.Areas.Count);
        Assert.Equal(first.Areas.Select(area => (area.X, area.Y, area.Width, area.Height, area.Rotation, area.ShapeSeed)),
            second.Areas.Select(area => (area.X, area.Y, area.Width, area.Height, area.Rotation, area.ShapeSeed)));
        Assert.Equal(first.Passages.Select(passage => (passage.FromAreaId, passage.ToAreaId, passage.Secret, passage.Routing)),
            second.Passages.Select(passage => (passage.FromAreaId, passage.ToAreaId, passage.Secret, passage.Routing)));
        Assert.Equal(first.Features.Select(feature => (feature.AreaId, feature.X, feature.Y, feature.Kind)),
            second.Features.Select(feature => (feature.AreaId, feature.X, feature.Y, feature.Kind)));
    }

    [Theory]
    [InlineData(RandomMapKind.Dungeon)]
    [InlineData(RandomMapKind.Cave)]
    [InlineData(RandomMapKind.Inn)]
    [InlineData(RandomMapKind.Village)]
    public void Toutes_les_zones_sont_dans_la_carte_et_ne_se_chevauchent_pas(RandomMapKind kind)
    {
        var map = RandomDungeonMapGenerator.Generate(kind, 24, 3, 42);

        Assert.All(map.Areas, area =>
        {
            Assert.InRange(area.X, 0, RandomDungeonMap.CanvasWidth - area.Width);
            Assert.InRange(area.Y, 0, RandomDungeonMap.CanvasHeight - area.Height);
        });

        foreach (var (area, index) in map.Areas.Select((area, index) => (area, index)))
        foreach (var other in map.Areas.Skip(index + 1))
            Assert.False(Intersects(area, other));
    }

    [Theory]
    [InlineData(RandomMapKind.Dungeon)]
    [InlineData(RandomMapKind.Cave)]
    [InlineData(RandomMapKind.Inn)]
    [InlineData(RandomMapKind.Village)]
    public void Tous_les_espaces_sont_accessibles_depuis_l_entree(RandomMapKind kind)
    {
        var map = RandomDungeonMapGenerator.Generate(kind, 20, 4, 2026);
        var reached = new HashSet<int> { map.Areas[0].Id };

        while (true)
        {
            var neighbours = map.Passages
                .Where(passage => reached.Contains(passage.FromAreaId) || reached.Contains(passage.ToAreaId))
                .SelectMany(passage => new[] { passage.FromAreaId, passage.ToAreaId })
                .ToHashSet();
            var previousCount = reached.Count;
            reached.UnionWith(neighbours);
            if (reached.Count == previousCount) break;
        }

        Assert.Equal(map.Areas.Count, reached.Count);
        Assert.Contains(map.Areas, area => area.Purpose == RandomMapAreaPurpose.Entrance);
        Assert.Contains(map.Areas, area => area.Purpose == RandomMapAreaPurpose.Exit);
    }

    [Theory]
    [InlineData(RandomMapKind.Dungeon)]
    [InlineData(RandomMapKind.Cave)]
    [InlineData(RandomMapKind.Inn)]
    [InlineData(RandomMapKind.Village)]
    public void Les_details_sont_rattaches_et_places_dans_leur_zone(RandomMapKind kind)
    {
        var map = RandomDungeonMapGenerator.Generate(kind, 16, 3, 1604);

        Assert.True(map.Features.Count >= map.Areas.Count);
        Assert.All(map.Features, feature =>
        {
            var area = Assert.Single(map.Areas, area => area.Id == feature.AreaId);
            Assert.InRange(feature.X, area.X, area.X + area.Width);
            Assert.InRange(feature.Y, area.Y, area.Y + area.Height);
        });
        Assert.Contains(map.Features, feature => feature.Kind == RandomMapFeatureKind.Light);
        Assert.Contains(map.Features, feature => feature.Kind == RandomMapFeatureKind.Stairs);
    }

    [Theory]
    [InlineData(RandomMapKind.Inn, "Auberge sans nom", "Salle commune")]
    [InlineData(RandomMapKind.Village, "Village sans nom", "Auberge du Sanglier Noir")]
    public void Les_nouveaux_lieux_recoivent_une_identite_et_des_details_contextuels(
        RandomMapKind kind,
        string expectedMapName,
        string expectedAreaName)
    {
        var map = RandomDungeonMapGenerator.Generate(kind, 18, 2, 1789);

        Assert.Equal(expectedMapName, map.Name);
        Assert.Contains(map.Areas, area => area.Name == expectedAreaName);
        if (kind == RandomMapKind.Inn)
            Assert.Contains(map.Features, feature => feature.Kind is RandomMapFeatureKind.Table or RandomMapFeatureKind.Bed or RandomMapFeatureKind.Barrel);
        else
            Assert.Contains(map.Features, feature => feature.Kind is RandomMapFeatureKind.Forge or RandomMapFeatureKind.Tree or RandomMapFeatureKind.Wagon or RandomMapFeatureKind.Well);
    }

    [Fact]
    public void Le_village_brise_les_alignements_par_des_batiments_orientes()
    {
        var village = RandomDungeonMapGenerator.Generate(RandomMapKind.Village, 24, 3, 1789);

        Assert.Contains(village.Areas, area => Math.Abs(area.Rotation) >= 3);
        Assert.True(village.Areas.Select(area => Math.Round(area.Rotation)).Distinct().Count() >= 8);
        Assert.True(village.Areas.Select(area => Math.Round(area.CenterY / 10)).Distinct().Count() >= 10);
    }

    [Fact]
    public void L_auberge_forme_un_batiment_compact_compose_de_pieces_jointives()
    {
        var inn = RandomDungeonMapGenerator.Generate(RandomMapKind.Inn, 24, 2, 1789);

        Assert.Equal(24, inn.Areas.Count);
        Assert.All(inn.Areas, area =>
        {
            Assert.InRange(area.X, 80, 880);
            Assert.InRange(area.Y, 50, 590);
        });
        Assert.All(inn.Areas, area =>
            Assert.Contains(inn.Areas, other => other.Id != area.Id && DistanceBetweenEdges(area, other) <= 14));
    }

    [Fact]
    public void Le_donjon_se_deploie_en_grappes_irregulieres_et_varie_ses_salles()
    {
        var dungeon = RandomDungeonMapGenerator.Generate(RandomMapKind.Dungeon, 24, 3, 1789);

        Assert.True(dungeon.Areas.Count(area => Math.Abs(area.X % 10) > 0.2) >= 16);
        Assert.True(dungeon.Areas.Select(area => Math.Round(area.Width / 5)).Distinct().Count() >= 10);
        Assert.Contains(dungeon.Features, feature => feature.Kind is
            RandomMapFeatureKind.Brazier or RandomMapFeatureKind.Sarcophagus or
            RandomMapFeatureKind.Rubble or RandomMapFeatureKind.Statue);
    }

    [Fact]
    public void La_population_et_le_type_determinent_la_densite_du_plan_de_localite()
    {
        Assert.Equal(12, RandomDungeonMapGenerator.RecommendedAreaCount(
            new RandomMapSettlement("Bissendorf", "Village", "94 hab.", "Empire, Ostermark")));
        Assert.Equal(21, RandomDungeonMapGenerator.RecommendedAreaCount(
            new RandomMapSettlement("Auerswald", "Bourg", "2 500 hab.", "Empire, Reikland")));
        Assert.Equal(21, RandomDungeonMapGenerator.RecommendedAreaCount(
            new RandomMapSettlement("Bögenhafen", "Ville", "4 500 hab.", "Empire, Reikland")));
        Assert.Equal(30, RandomDungeonMapGenerator.RecommendedAreaCount(
            new RandomMapSettlement("Altdorf", "Cité", "15 000 hab.", "Empire, Reikland")));
    }

    [Fact]
    public void Le_plan_de_localite_conserve_les_donnees_du_referentiel()
    {
        var settlement = new RandomMapSettlement("Bissendorf", "Village", "94 hab.", "Empire, Ostermark");
        var areaCount = RandomDungeonMapGenerator.RecommendedAreaCount(settlement);
        var seed = RandomDungeonMapGenerator.SettlementSeed(settlement);

        var map = RandomDungeonMapGenerator.Generate(RandomMapKind.Village, areaCount, 2, seed, settlement);

        Assert.Equal("Bissendorf", map.Name);
        Assert.Equal("Village", map.SettlementType);
        Assert.Equal("94 hab.", map.SettlementPopulation);
        Assert.Equal("Empire, Ostermark", map.SettlementRegion);
        Assert.Equal(RandomMapSettlementLandscape.River, map.SettlementLandscape);
        Assert.Equal(map.SettlementLandscape, map.Clone().SettlementLandscape);
        Assert.Equal(areaCount, map.Areas.Count);
        Assert.Contains(map.Areas, area => area.Name == "Entrée de Bissendorf");
        Assert.Contains(map.Areas, area => area.Name == "Route de Ostermark");
    }

    [Fact]
    public void La_disposition_urbaine_suit_le_statut_de_la_localite()
    {
        var city = new RandomMapSettlement("Altdorf", "Cité", "15 000 hab.", "Empire, Reikland");
        var town = new RandomMapSettlement("Auerswald", "Bourg", "2 500 hab.", "Empire, Reikland");
        var village = new RandomMapSettlement("Bissendorf", "Village", "94 hab.", "Empire, Ostermark");

        Assert.Equal(RandomMapSettlementLayout.Fortified, RandomDungeonMapGenerator.SettlementLayoutFor(city));
        Assert.Equal(RandomMapSettlementLayout.Clustered, RandomDungeonMapGenerator.SettlementLayoutFor(town));
        Assert.Contains(RandomDungeonMapGenerator.SettlementLayoutFor(village), new[]
        {
            RandomMapSettlementLayout.Linear,
            RandomMapSettlementLayout.Clustered,
            RandomMapSettlementLayout.Dispersed
        });

        var map = RandomDungeonMapGenerator.Generate(
            RandomMapKind.Village,
            RandomDungeonMapGenerator.RecommendedAreaCount(city),
            6,
            RandomDungeonMapGenerator.SettlementSeed(city),
            city);

        Assert.Equal(RandomMapSettlementLayout.Fortified, map.SettlementLayout);
        Assert.All(map.Areas, area =>
        {
            Assert.InRange(area.X, 0, RandomDungeonMap.CanvasWidth - area.Width);
            Assert.InRange(area.Y, 0, RandomDungeonMap.CanvasHeight - area.Height);
        });
        foreach (var (area, index) in map.Areas.Select((area, index) => (area, index)))
        foreach (var other in map.Areas.Skip(index + 1))
            Assert.False(Intersects(area, other));
    }

    private static bool Intersects(RandomMapArea left, RandomMapArea right) =>
        left.X < right.X + right.Width && left.X + left.Width > right.X &&
        left.Y < right.Y + right.Height && left.Y + left.Height > right.Y;

    private static double DistanceBetweenEdges(RandomMapArea left, RandomMapArea right)
    {
        var horizontal = Math.Max(0, Math.Max(left.X, right.X) - Math.Min(left.X + left.Width, right.X + right.Width));
        var vertical = Math.Max(0, Math.Max(left.Y, right.Y) - Math.Min(left.Y + left.Height, right.Y + right.Height));
        return Math.Sqrt(horizontal * horizontal + vertical * vertical);
    }
}
