using System.Xml.Linq;
using Wfrp4.Shared.Maps;

namespace Wfrp4.Server.Tests;

public class SettlementAtlasTests
{
    [Theory]
    [InlineData(42, RandomMapSettlementLandscape.River)]
    [InlineData(908875339, RandomMapSettlementLandscape.Port)]
    [InlineData(1789, RandomMapSettlementLandscape.Countryside)]
    public void Buildings_are_separate_bounded_and_clear_of_water(int seed, RandomMapSettlementLandscape landscape)
    {
        var map = new RandomDungeonMap { Seed = seed, SettlementType = "Ville", SettlementLandscape = landscape };
        var plan = SettlementAtlas.Generate(map);
        Assert.True(plan.Buildings.Count > 100);
        foreach (var building in plan.Buildings)
        foreach (var p in building.Outline)
        {
            Assert.InRange(p.X, 0, 960);
            Assert.InRange(p.Y, 0, 640);
            if (landscape == RandomMapSettlementLandscape.River)
                Assert.True(Math.Abs(p.X - SettlementAtlas.RiverX(p.Y, plan.Phase)) >= 14);
            if (landscape == RandomMapSettlementLandscape.Port) Assert.True(p.X <= 770);
        }
        for (var i = 0; i < plan.Buildings.Count; i++)
        for (var j = i + 1; j < plan.Buildings.Count; j++)
            Assert.False(SettlementAtlas.Overlap(plan.Buildings[i].Outline, plan.Buildings[j].Outline));
    }

    [Fact]
    public void Svg_is_reproducible_and_encodes_user_text()
    {
        var map = new RandomDungeonMap { Seed = 1789, SettlementType = "Ville" };
        const string name = "<script>alert('x')</script> & cité";
        var first = SettlementAtlas.Render(SettlementAtlas.Generate(map), name, true);
        var second = SettlementAtlas.Render(SettlementAtlas.Generate(map), name, true);
        Assert.Equal(first, second);
        var xml = XDocument.Parse(first);
        Assert.Empty(xml.Descendants("script"));
        Assert.Contains(xml.Descendants("text"), e => e.Value == name);
    }

    [Fact]
    public void City_is_denser_than_village_and_hidden_labels_do_not_remove_buildings()
    {
        var map = new RandomDungeonMap { Seed = 908875339, SettlementType = "Village" };
        var village = SettlementAtlas.Generate(map);
        map.SettlementType = "Cité";
        var city = SettlementAtlas.Generate(map);
        Assert.True(city.Buildings.Count > village.Buildings.Count * 1.5);
        Assert.DoesNotContain("VIEUX BOURG", SettlementAtlas.Render(city, "Test", false));
        Assert.Contains("VIEUX BOURG", SettlementAtlas.Render(city, "Test", true));
    }

    [Fact]
    public void City_size_changes_urban_footprint_and_density()
    {
        var map = new RandomDungeonMap { Seed = 42, SettlementType = "Ville", SettlementFootprintPercent = 70 };
        var small = SettlementAtlas.Generate(map);
        map.SettlementFootprintPercent = 150;
        var large = SettlementAtlas.Generate(map);
        Assert.True(large.Buildings.Count > small.Buildings.Count);
        Assert.True(large.Buildings.SelectMany(b => b.Outline).Max(p => p.X) > small.Buildings.SelectMany(b => b.Outline).Max(p => p.X));
    }

    [Fact]
    public void Density_form_and_street_width_are_independent_parameters()
    {
        var map = new RandomDungeonMap { Seed = 1789, SettlementType = "Ville", SettlementHasRiver = false };
        map.SettlementDensityPercent = 65;
        var sparse = SettlementAtlas.Generate(map);
        map.SettlementDensityPercent = 140;
        map.SettlementStreetWidthPercent = 140;
        var dense = SettlementAtlas.Generate(map);
        map.SettlementForm = RandomMapSettlementForm.Elongated;
        var elongated = SettlementAtlas.Generate(map);
        Assert.True(dense.Buildings.Count > sparse.Buildings.Count);
        Assert.Equal(1.4, dense.StreetWidth, 2);
        Assert.True(elongated.Buildings.SelectMany(b => b.Outline).Max(p => p.Y) - elongated.Buildings.SelectMany(b => b.Outline).Min(p => p.Y) >
                    dense.Buildings.SelectMany(b => b.Outline).Max(p => p.Y) - dense.Buildings.SelectMany(b => b.Outline).Min(p => p.Y));
    }

    [Fact]
    public void River_bridges_are_joined_into_a_bounded_number_of_svg_paths()
    {
        var map = new RandomDungeonMap
        {
            Kind = RandomMapKind.Village,
            Seed = 908875339,
            SettlementType = "Cité",
            SettlementFootprintPercent = 160,
            SettlementDensityPercent = 140,
            SettlementHasCoast = true,
            SettlementHasForest = true
        };
        map.SettlementHasRiver = false;
        var withoutRiver = SettlementAtlas.Render(SettlementAtlas.Generate(map), "Test", true);
        map.SettlementHasRiver = true;
        var withRiver = SettlementAtlas.Render(SettlementAtlas.Generate(map), "Test", true);

        var pathsWithoutRiver = XDocument.Parse($"<svg xmlns=\"http://www.w3.org/2000/svg\">{withoutRiver}</svg>").Descendants().Count(element => element.Name.LocalName == "path");
        var pathsWithRiver = XDocument.Parse($"<svg xmlns=\"http://www.w3.org/2000/svg\">{withRiver}</svg>").Descendants().Count(element => element.Name.LocalName == "path");

        Assert.InRange(pathsWithRiver - pathsWithoutRiver, 1, 250);
        Assert.True(withRiver.Length < withoutRiver.Length * 1.2);
    }

    [Fact]
    public void Places_stay_in_town_and_render_combined_river_coast_and_forest()
    {
        var map = RandomDungeonMapGenerator.Generate(RandomMapKind.Village, 14, 2, 908875339);
        map.SettlementType = "Ville";
        map.SettlementHasRiver = true;
        map.SettlementHasCoast = true;
        map.SettlementHasForest = true;
        var originalAreas = map.Areas.ToDictionary(area => area.Id, area => new SettlementAtlas.Point(area.X, area.Y));
        var originalFeatures = map.Features.ToDictionary(feature => feature.Id, feature => new SettlementAtlas.Point(feature.X, feature.Y));
        SettlementAtlas.ConstrainPlaces(map);
        var plan = SettlementAtlas.Generate(map);
        Assert.All(map.Areas, area =>
        {
            Assert.InRange(area.CenterX, 195, 720);
            Assert.InRange(area.CenterY, 150, 470);
            Assert.True(Math.Abs(area.CenterX - SettlementAtlas.RiverX(area.CenterY, plan.Phase)) >= 14);
            Assert.True(area.CenterX <= 770);
            var dx = (area.CenterX - 475) / (238 * plan.Scale);
            var dy = (area.CenterY - 298) / (153 * plan.Scale);
            var core = dx * dx + dy * dy < .48 + .08 * Math.Sin(area.CenterX / 41 + plan.Phase);
            var tail = Math.Pow((area.CenterX - 590) / (75 * plan.Scale), 2) + Math.Pow((area.CenterY - 467) / 132, 2) < .48;
            Assert.True(core || tail);
        });
        var svg = SettlementAtlas.Render(plan, "Test", true);
        Assert.Contains("M 795 0 Q 755 160", svg);
        Assert.Contains("fill=\"#aab394\"", svg);
        Assert.True(plan.HasRiver && plan.HasCoast && plan.HasForest);
        foreach (var feature in map.Features)
        {
            var area = map.Areas.Single(area => area.Id == feature.AreaId);
            var beforeArea = originalAreas[area.Id];
            var beforeFeature = originalFeatures[feature.Id];
            Assert.Equal(beforeFeature.X + area.X - beforeArea.X, feature.X, 6);
            Assert.Equal(beforeFeature.Y + area.Y - beforeArea.Y, feature.Y, 6);
        }
        var clone = map.Clone();
        Assert.Equal(RandomMapSettlementSize.Medium, clone.SettlementSize);
        Assert.Equal(map.SettlementHasForest, clone.SettlementHasForest);
        Assert.Equal(map.SettlementFootprintPercent, clone.SettlementFootprintPercent);
        Assert.Equal(map.SettlementDensityPercent, clone.SettlementDensityPercent);
        Assert.Equal(map.SettlementForm, clone.SettlementForm);
    }
}
