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
        Assert.Equal(first.Districts.SelectMany(district => district.Boundary.Select(point => (district.Id, point.X, point.Y))),
            second.Districts.SelectMany(district => district.Boundary.Select(point => (district.Id, point.X, point.Y))));
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
        Assert.Equal(map.Districts.Count, map.Clone().Districts.Count);
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

    [Fact]
    public void Une_cite_est_decoupee_en_quartiers_organiques_autour_d_un_marche()
    {
        var city = new RandomMapSettlement("Altdorf", "Cité", "15 000 hab.", "Empire, Reikland");

        var map = RandomDungeonMapGenerator.Generate(RandomMapKind.Village, 30, 6, 1789, city);

        Assert.InRange(map.Districts.Count, 12, 16);
        Assert.Equal(RandomMapDistrictKind.Market, map.Districts[0].Kind);
        Assert.Contains(map.Districts, district => district.Kind == RandomMapDistrictKind.Craftsmen);
        Assert.Contains(map.Districts, district => district.Kind == RandomMapDistrictKind.Merchant);
        Assert.Contains(map.Districts, district => district.Kind == RandomMapDistrictKind.Military);
        Assert.All(map.Districts, district =>
        {
            Assert.True(district.Boundary.Count >= 3);
            Assert.All(district.Boundary, point =>
            {
                Assert.InRange(point.X, 0, RandomDungeonMap.CanvasWidth);
                Assert.InRange(point.Y, 0, RandomDungeonMap.CanvasHeight);
            });
        });
    }

    [Theory]
    [InlineData(3, RandomMapSettlementLayout.Linear)]
    [InlineData(4, RandomMapSettlementLayout.Clustered)]
    [InlineData(5, RandomMapSettlementLayout.Dispersed)]
    public void Un_village_aleatoire_recoit_une_implantation_et_des_quartiers_reproductibles(
        int seed,
        RandomMapSettlementLayout expectedLayout)
    {
        var first = RandomDungeonMapGenerator.Generate(RandomMapKind.Village, 14, 2, seed);
        var second = RandomDungeonMapGenerator.Generate(RandomMapKind.Village, 14, 2, seed);

        Assert.Equal(expectedLayout, first.SettlementLayout);
        Assert.InRange(first.Districts.Count, 4, 6);
        Assert.All(first.Districts, district => Assert.True(district.Boundary.Count >= 3));
        Assert.Equal(first.SettlementBoundary.Select(point => (point.X, point.Y)),
            second.SettlementBoundary.Select(point => (point.X, point.Y)));
        Assert.Equal(first.Districts.Select(district => (district.Name, district.Kind, district.CenterX, district.CenterY)),
            second.Districts.Select(district => (district.Name, district.Kind, district.CenterX, district.CenterY)));
    }

    [Fact]
    public void La_route_principale_traverse_entierement_la_carte()
    {
        var first = RandomSettlementMapGeometry.MainRoadPoints(1789);
        var second = RandomSettlementMapGeometry.MainRoadPoints(1789);

        Assert.True(first[0].X < 0);
        Assert.True(first[^1].X > RandomDungeonMap.CanvasWidth);
        Assert.All(first, point => Assert.InRange(point.Y, 0, RandomDungeonMap.CanvasHeight));
        Assert.Equal(first.Select(point => (point.X, point.Y)), second.Select(point => (point.X, point.Y)));
    }

    [Fact]
    public void Le_reseau_de_rues_raccorde_tous_les_quartiers_au_marche()
    {
        var city = new RandomMapSettlement("Altdorf", "Cité", "15 000 hab.", "Empire, Reikland");
        var map = RandomDungeonMapGenerator.Generate(RandomMapKind.Village, 30, 6, 1789, city);
        var connections = RandomSettlementMapGeometry.DistrictConnections(map);
        var market = Assert.Single(map.Districts, district => district.Kind == RandomMapDistrictKind.Market);
        var reached = new HashSet<int> { market.Id };

        Assert.Equal(map.Districts.Count - 1, connections.Count);
        while (true)
        {
            var previousCount = reached.Count;
            foreach (var connection in connections)
            {
                if (reached.Contains(connection.FromDistrictId)) reached.Add(connection.ToDistrictId);
                if (reached.Contains(connection.ToDistrictId)) reached.Add(connection.FromDistrictId);
            }
            if (reached.Count == previousCount) break;
        }

        Assert.Equal(map.Districts.Select(district => district.Id).Order(), reached.Order());
    }

    [Fact]
    public void Une_cite_fortifiee_possede_une_porte_sur_chaque_face_de_l_enceinte()
    {
        var city = new RandomMapSettlement("Altdorf", "Cité", "15 000 hab.", "Empire, Reikland");
        var map = RandomDungeonMapGenerator.Generate(RandomMapKind.Village, 30, 6, 1789, city);

        var gates = RandomSettlementMapGeometry.SettlementGates(map);

        Assert.Equal(4, gates.Count);
        Assert.Equal(4, gates.Select(gate => gate.Edge).Distinct().Count());
        Assert.Equal(map.SettlementBoundary.Min(point => point.Y), Assert.Single(gates, gate => gate.Edge == RandomMapEdge.North).Y);
        Assert.Equal(map.SettlementBoundary.Max(point => point.X), Assert.Single(gates, gate => gate.Edge == RandomMapEdge.East).X);
        Assert.Equal(map.SettlementBoundary.Max(point => point.Y), Assert.Single(gates, gate => gate.Edge == RandomMapEdge.South).Y);
        Assert.Equal(map.SettlementBoundary.Min(point => point.X), Assert.Single(gates, gate => gate.Edge == RandomMapEdge.West).X);
    }

    [Fact]
    public void Un_projet_de_carte_sauvegarde_restitue_la_carte_editable_et_ses_options()
    {
        var map = RandomDungeonMapGenerator.Generate(RandomMapKind.Village, 14, 4, 1789);
        map.Name = "Crypte du Corbeau";
        map.Areas[1].Name = "Salle remaniée";
        map.Passages[0].Secret = true;
        map.SettlementFootprintPercent = 135;
        map.SettlementDensityPercent = 115;
        map.SettlementStreetWidthPercent = 90;
        map.SettlementForm = RandomMapSettlementForm.Sprawling;
        var source = new RandomDungeonMapProject
        {
            Map = map,
            AreaCount = 14,
            Loops = 4,
            ShowGrid = true,
            ShowLabels = false,
            ShowFeatures = false
        };

        var json = RandomDungeonMapProjectSerializer.Serialize(source);
        var success = RandomDungeonMapProjectSerializer.TryDeserialize(json, out var restored, out var error);

        Assert.True(success, error);
        Assert.NotNull(restored);
        Assert.Equal("Crypte du Corbeau", restored.Map.Name);
        Assert.Equal("Salle remaniée", restored.Map.Areas[1].Name);
        Assert.True(restored.Map.Passages[0].Secret);
        Assert.Equal(4, restored.Loops);
        Assert.True(restored.ShowGrid);
        Assert.False(restored.ShowLabels);
        Assert.False(restored.ShowFeatures);
        Assert.Equal(map.Districts.Count, restored.Map.Districts.Count);
        Assert.Equal(135, restored.Map.SettlementFootprintPercent);
        Assert.Equal(115, restored.Map.SettlementDensityPercent);
        Assert.Equal(90, restored.Map.SettlementStreetWidthPercent);
        Assert.Equal(RandomMapSettlementForm.Sprawling, restored.Map.SettlementForm);
    }

    [Fact]
    public void Un_projet_importe_refuse_un_detail_rattache_a_une_zone_inconnue()
    {
        var map = RandomDungeonMapGenerator.Generate(RandomMapKind.Inn, 10, 2, 42);
        map.Features[0].AreaId = 999;
        var json = RandomDungeonMapProjectSerializer.Serialize(new RandomDungeonMapProject
        {
            Map = map,
            AreaCount = 10,
            Loops = 2
        });

        var success = RandomDungeonMapProjectSerializer.TryDeserialize(json, out var restored, out var error);

        Assert.False(success);
        Assert.Null(restored);
        Assert.Contains("détail", error);
    }

    [Fact]
    public void Un_projet_refuse_des_parametres_de_ville_hors_limites()
    {
        var map = RandomDungeonMapGenerator.Generate(RandomMapKind.Village, 10, 2, 42);
        map.SettlementDensityPercent = 500;
        var json = RandomDungeonMapProjectSerializer.Serialize(new RandomDungeonMapProject
        {
            Map = map,
            AreaCount = 10,
            Loops = 2
        });

        var success = RandomDungeonMapProjectSerializer.TryDeserialize(json, out var restored, out var error);

        Assert.False(success);
        Assert.Null(restored);
        Assert.Contains("paramètres de ville", error);
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
