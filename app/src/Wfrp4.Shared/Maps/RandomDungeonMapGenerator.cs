namespace Wfrp4.Shared.Maps;

public enum RandomMapKind
{
    Dungeon,
    Cave,
    Inn,
    Village
}

public enum RandomMapAreaPurpose
{
    Entrance,
    Chamber,
    Hazard,
    Treasure,
    Shrine,
    Exit
}

public enum RandomMapFeatureKind
{
    Chest,
    Npc,
    Light,
    Well,
    Trap,
    Altar,
    Stairs,
    Campfire,
    Table,
    Bed,
    Barrel,
    Forge,
    Tree,
    Wagon,
    Brazier,
    Sarcophagus,
    Rubble,
    Statue
}

public enum RandomMapPassageRouting
{
    Direct,
    HorizontalFirst,
    VerticalFirst
}

public enum RandomMapSettlementLayout
{
    Linear,
    Clustered,
    Dispersed,
    Fortified
}

public enum RandomMapSettlementLandscape
{
    Countryside,
    River,
    Port
}

public sealed record RandomMapSettlement(string Name, string Type, string Population, string Region);

public sealed class RandomMapArea
{
    public int Id { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
    public double Rotation { get; set; }
    public int ShapeSeed { get; set; }
    public string Name { get; set; } = string.Empty;
    public RandomMapAreaPurpose Purpose { get; set; }

    public double CenterX => X + Width / 2;
    public double CenterY => Y + Height / 2;

    public RandomMapArea Clone() => new()
    {
        Id = Id,
        X = X,
        Y = Y,
        Width = Width,
        Height = Height,
        Rotation = Rotation,
        ShapeSeed = ShapeSeed,
        Name = Name,
        Purpose = Purpose
    };
}

public sealed class RandomMapPassage
{
    public int FromAreaId { get; set; }
    public int ToAreaId { get; set; }
    public bool Secret { get; set; }
    public RandomMapPassageRouting Routing { get; set; }

    public RandomMapPassage Clone() => new()
    {
        FromAreaId = FromAreaId,
        ToAreaId = ToAreaId,
        Secret = Secret,
        Routing = Routing
    };
}

public sealed class RandomMapFeature
{
    public int Id { get; set; }
    public int AreaId { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public RandomMapFeatureKind Kind { get; set; }

    public RandomMapFeature Clone() => new()
    {
        Id = Id,
        AreaId = AreaId,
        X = X,
        Y = Y,
        Kind = Kind
    };
}

public sealed class RandomDungeonMap
{
    public const int CanvasWidth = 960;
    public const int CanvasHeight = 640;

    public RandomMapKind Kind { get; set; }
    public int Seed { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? SettlementType { get; set; }
    public string? SettlementPopulation { get; set; }
    public string? SettlementRegion { get; set; }
    public RandomMapSettlementLayout? SettlementLayout { get; set; }
    public RandomMapSettlementLandscape? SettlementLandscape { get; set; }
    public List<RandomMapArea> Areas { get; set; } = [];
    public List<RandomMapPassage> Passages { get; set; } = [];
    public List<RandomMapFeature> Features { get; set; } = [];

    public RandomDungeonMap Clone() => new()
    {
        Kind = Kind,
        Seed = Seed,
        Name = Name,
        SettlementType = SettlementType,
        SettlementPopulation = SettlementPopulation,
        SettlementRegion = SettlementRegion,
        SettlementLayout = SettlementLayout,
        SettlementLandscape = SettlementLandscape,
        Areas = Areas.Select(area => area.Clone()).ToList(),
        Passages = Passages.Select(passage => passage.Clone()).ToList(),
        Features = Features.Select(feature => feature.Clone()).ToList()
    };
}

public static class RandomDungeonMapGenerator
{
    private const int Margin = 36;

    private static readonly string[] DungeonNames =
    [
        "Vestibule effondré", "Salle des chaînes", "Galerie des crânes", "Ancienne caserne",
        "Crypte scellée", "Forge froide", "Chapelle profanée", "Réserve oubliée",
        "Puits noir", "Geôle humide", "Salle des gardes", "Bibliothèque interdite",
        "Sanctuaire intérieur", "Trésorerie", "Escalier brisé", "Antre du maître"
    ];

    private static readonly string[] CaveNames =
    [
        "Gouffre des échos", "Boyau noyé", "Caverne des spores", "Faille sifflante",
        "Lac souterrain", "Nid de chauves-souris", "Cheminée naturelle", "Forêt de cristaux",
        "Vasque noire", "Galerie des racines", "Antre fétide", "Plage de cendre",
        "Abîme silencieux", "Dôme minéral", "Source chaude", "Cœur de la grotte"
    ];

    private static readonly string[] InnNames =
    [
        "Salle commune", "Cuisine enfumée", "Réserve de bière", "Chambre du voyageur",
        "Chambre des marchands", "Écurie", "Bureau de l’aubergiste", "Cellier",
        "Dortoir", "Salon privé", "Garde-manger", "Cour intérieure",
        "Chambre verrouillée", "Buanderie", "Cave à tonneaux", "Grenier"
    ];

    private static readonly string[] VillageNames =
    [
        "Auberge du Sanglier Noir", "Forge", "Temple de Sigmar", "Maison du bourgmestre",
        "Moulin", "Ferme isolée", "Échoppe", "Grange", "Corps de garde",
        "Marché", "Maison du guérisseur", "Puits communal", "Cimetière",
        "Relais de diligence", "Brasserie", "Ruine envahie"
    ];

    private static readonly string[] TownNames =
    [
        "Place du marché", "Maison du prévôt", "Auberge principale", "Temple",
        "Forge et maréchalerie", "Halle aux grains", "Corps de garde", "Brasserie",
        "Quartier des artisans", "Relais de diligence", "Greniers communaux", "Hospice",
        "Cour des marchands", "Moulin", "Écuries", "Faubourg"
    ];

    private static readonly string[] CityNames =
    [
        "Grand marché", "Hôtel de ville", "Temple principal", "Caserne",
        "Quartier des artisans", "Quartier marchand", "Entrepôts", "Hospice",
        "Porte fortifiée", "Place des diligences", "Greniers", "Auberge renommée",
        "Faubourg populaire", "Maison de guilde", "Cour de justice", "Cimetière",
        "Quartier noble", "Marché aux bestiaux", "Tour de guet", "Ancienne enceinte"
    ];

    public static RandomDungeonMap Generate(
        RandomMapKind kind,
        int areaCount,
        int loops,
        int seed,
        RandomMapSettlement? settlement = null)
    {
        areaCount = Math.Clamp(areaCount, 4, 30);
        loops = Math.Clamp(loops, 0, 6);
        var random = new Random(seed);
        settlement = kind == RandomMapKind.Village ? settlement : null;
        RandomMapSettlementLayout? settlementLayout = settlement is null ? null : SettlementLayoutFor(settlement);
        var areas = GenerateAreas(kind, areaCount, random, settlement, settlementLayout);
        var passages = ConnectAreas(areas, loops, random);
        var features = GenerateFeatures(kind, areas, random);

        return new RandomDungeonMap
        {
            Kind = kind,
            Seed = seed,
            Name = settlement?.Name ?? kind switch
            {
                RandomMapKind.Dungeon => "Donjon sans nom",
                RandomMapKind.Cave => "Grotte sans nom",
                RandomMapKind.Inn => "Auberge sans nom",
                RandomMapKind.Village => "Village sans nom",
                _ => "Lieu sans nom"
            },
            SettlementType = settlement?.Type,
            SettlementPopulation = settlement?.Population,
            SettlementRegion = settlement?.Region,
            SettlementLayout = settlementLayout,
            SettlementLandscape = kind == RandomMapKind.Village ? RandomMapSettlementLandscape.River : null,
            Areas = areas,
            Passages = passages,
            Features = features
        };
    }

    public static int RecommendedAreaCount(RandomMapSettlement settlement)
    {
        var population = ParsePopulation(settlement.Population);
        return settlement.Type switch
        {
            "Cité" => population is null ? 28 : Math.Clamp(24 + population.Value / 2500, 24, 30),
            "Ville" => population is null ? 23 : Math.Clamp(18 + population.Value / 1200, 18, 28),
            "Bourg" => population is null ? 16 : Math.Clamp(11 + population.Value / 220, 11, 21),
            _ => population is null ? 11 : Math.Clamp(7 + population.Value / 18, 7, 15)
        };
    }

    public static int SettlementSeed(RandomMapSettlement settlement)
    {
        unchecked
        {
            var hash = 2166136261u;
            foreach (var character in $"{settlement.Name}|{settlement.Type}|{settlement.Region}")
                hash = (hash ^ character) * 16777619u;
            return (int)(hash % (int.MaxValue - 1)) + 1;
        }
    }

    public static RandomMapSettlementLayout SettlementLayoutFor(RandomMapSettlement settlement)
    {
        if (settlement.Type is "Ville" or "Cité") return RandomMapSettlementLayout.Fortified;
        if (settlement.Type == "Bourg") return RandomMapSettlementLayout.Clustered;

        return (SettlementSeed(settlement) % 3) switch
        {
            0 => RandomMapSettlementLayout.Linear,
            1 => RandomMapSettlementLayout.Clustered,
            _ => RandomMapSettlementLayout.Dispersed
        };
    }

    private static List<RandomMapArea> GenerateAreas(
        RandomMapKind kind,
        int count,
        Random random,
        RandomMapSettlement? settlement,
        RandomMapSettlementLayout? settlementLayout)
    {
        return kind switch
        {
            RandomMapKind.Dungeon => GenerateDungeonAreas(count, random),
            RandomMapKind.Inn => GenerateInnAreas(count, random),
            RandomMapKind.Village => GenerateVillageAreas(count, random, settlement, settlementLayout ?? RandomMapSettlementLayout.Linear),
            _ => GenerateGridAreas(kind, count, random)
        };
    }

    private static List<RandomMapArea> GenerateGridAreas(RandomMapKind kind, int count, Random random)
    {
        var aspectRatio = RandomDungeonMap.CanvasWidth / (double)RandomDungeonMap.CanvasHeight;
        var columns = Math.Max(2, (int)Math.Ceiling(Math.Sqrt(count * aspectRatio)));
        var rows = (int)Math.Ceiling(count / (double)columns);
        var cellWidth = (RandomDungeonMap.CanvasWidth - Margin * 2) / (double)columns;
        var cellHeight = (RandomDungeonMap.CanvasHeight - Margin * 2) / (double)rows;
        var cells = Enumerable.Range(0, columns * rows).OrderBy(_ => random.Next()).Take(count).ToArray();
        var areas = new List<RandomMapArea>(count);

        for (var index = 0; index < cells.Length; index++)
        {
            var cell = cells[index];
            var column = cell % columns;
            var row = cell / columns;
            var widthFactor = kind switch
            {
                RandomMapKind.Cave => 0.62 + random.NextDouble() * 0.22,
                RandomMapKind.Village => 0.46 + random.NextDouble() * 0.20,
                _ => 0.58 + random.NextDouble() * 0.18
            };
            var heightFactor = kind switch
            {
                RandomMapKind.Cave => 0.60 + random.NextDouble() * 0.24,
                RandomMapKind.Village => 0.42 + random.NextDouble() * 0.20,
                _ => 0.52 + random.NextDouble() * 0.22
            };
            var width = Math.Clamp(cellWidth * widthFactor, 58, 150);
            var height = Math.Clamp(cellHeight * heightFactor, 48, 120);
            var x = Margin + column * cellWidth + (cellWidth - width) / 2 + Jitter(random, cellWidth * 0.08);
            var y = Margin + row * cellHeight + (cellHeight - height) / 2 + Jitter(random, cellHeight * 0.08);

            if (kind != RandomMapKind.Cave)
            {
                x = Snap(x, 10);
                y = Snap(y, 10);
                width = Snap(width, 10);
                height = Snap(height, 10);
            }

            areas.Add(new RandomMapArea
            {
                Id = index + 1,
                X = Math.Clamp(x, Margin, RandomDungeonMap.CanvasWidth - Margin - width),
                Y = Math.Clamp(y, Margin, RandomDungeonMap.CanvasHeight - Margin - height),
                Width = width,
                Height = height,
                ShapeSeed = random.Next(1, int.MaxValue),
                Name = AreaName(kind, index, count),
                Purpose = AreaPurpose(index, count)
            });
        }

        return areas;
    }

    private static List<RandomMapArea> GenerateDungeonAreas(int count, Random random)
    {
        var areas = new List<RandomMapArea>(count);
        var entranceWidth = 76 + random.NextDouble() * 28;
        var entranceHeight = 58 + random.NextDouble() * 26;
        areas.Add(new RandomMapArea
        {
            Id = 1,
            X = 48 + random.NextDouble() * 55,
            Y = RandomDungeonMap.CanvasHeight / 2d - entranceHeight / 2 + Jitter(random, 85),
            Width = entranceWidth,
            Height = entranceHeight,
            ShapeSeed = random.Next(1, int.MaxValue),
            Name = AreaName(RandomMapKind.Dungeon, 0, count),
            Purpose = RandomMapAreaPurpose.Entrance
        });

        for (var index = 1; index < count; index++)
        {
            var width = index % 8 == 0 ? 130 + random.NextDouble() * 42 : 68 + random.NextDouble() * 70;
            var height = index % 8 == 0 ? 92 + random.NextDouble() * 30 : 52 + random.NextDouble() * 60;
            var placed = false;
            double x = 0;
            double y = 0;

            for (var attempt = 0; attempt < 520; attempt++)
            {
                var anchor = areas[random.Next(areas.Count)];
                var direction = random.Next(4);
                var gap = 30 + random.NextDouble() * 72;
                switch (direction)
                {
                    case 0:
                        x = anchor.X + anchor.Width + gap;
                        y = anchor.CenterY - height / 2 + Jitter(random, 45);
                        break;
                    case 1:
                        x = anchor.X - width - gap;
                        y = anchor.CenterY - height / 2 + Jitter(random, 45);
                        break;
                    case 2:
                        x = anchor.CenterX - width / 2 + Jitter(random, 55);
                        y = anchor.Y + anchor.Height + gap;
                        break;
                    default:
                        x = anchor.CenterX - width / 2 + Jitter(random, 55);
                        y = anchor.Y - height - gap;
                        break;
                }

                if (x < Margin || y < Margin || x + width > RandomDungeonMap.CanvasWidth - Margin ||
                    y + height > RandomDungeonMap.CanvasHeight - Margin)
                    continue;
                if (areas.All(area => !Intersects(area, x, y, width, height, 18)))
                {
                    placed = true;
                    break;
                }
            }

            for (var attempt = 0; attempt < 700 && !placed; attempt++)
            {
                x = Margin + random.NextDouble() * (RandomDungeonMap.CanvasWidth - Margin * 2 - width);
                y = Margin + random.NextDouble() * (RandomDungeonMap.CanvasHeight - Margin * 2 - height);
                if (areas.All(area => !Intersects(area, x, y, width, height, 13)))
                    placed = true;
            }

            if (!placed)
            {
                width = 58;
                height = 44;
                for (var scanY = Margin; scanY <= RandomDungeonMap.CanvasHeight - Margin - height && !placed; scanY += 14)
                for (var scanX = Margin; scanX <= RandomDungeonMap.CanvasWidth - Margin - width; scanX += 14)
                {
                    if (!areas.All(area => !Intersects(area, scanX, scanY, width, height, 8))) continue;
                    x = scanX;
                    y = scanY;
                    placed = true;
                    break;
                }
            }

            areas.Add(new RandomMapArea
            {
                Id = index + 1,
                X = x,
                Y = y,
                Width = width,
                Height = height,
                ShapeSeed = random.Next(1, int.MaxValue),
                Name = AreaName(RandomMapKind.Dungeon, index, count),
                Purpose = AreaPurpose(index, count)
            });
        }

        return areas;
    }

    private static List<RandomMapArea> GenerateInnAreas(int count, Random random)
    {
        const double gap = 8;
        var partitions = new List<MapPartition> { new(88, 58, 784, 524) };
        while (partitions.Count < count)
        {
            var candidate = partitions
                .Where(partition => partition.Width >= 126 || partition.Height >= 106)
                .OrderByDescending(partition => partition.Width * partition.Height)
                .FirstOrDefault();
            if (candidate is null) break;

            partitions.Remove(candidate);
            var splitVertically = candidate.Width / candidate.Height > 1.18 ||
                                  candidate.Height / candidate.Width < 1.18 && random.Next(2) == 0;
            var ratio = 0.42 + random.NextDouble() * 0.16;
            if (splitVertically && candidate.Width >= 126)
            {
                var leftWidth = candidate.Width * ratio - gap / 2;
                partitions.Add(new MapPartition(candidate.X, candidate.Y, leftWidth, candidate.Height));
                partitions.Add(new MapPartition(candidate.X + leftWidth + gap, candidate.Y,
                    candidate.Width - leftWidth - gap, candidate.Height));
            }
            else
            {
                var topHeight = candidate.Height * ratio - gap / 2;
                partitions.Add(new MapPartition(candidate.X, candidate.Y, candidate.Width, topHeight));
                partitions.Add(new MapPartition(candidate.X, candidate.Y + topHeight + gap,
                    candidate.Width, candidate.Height - topHeight - gap));
            }
        }

        return partitions
            .OrderBy(partition => partition.Y)
            .ThenBy(partition => partition.X)
            .Take(count)
            .Select((partition, index) => new RandomMapArea
            {
                Id = index + 1,
                X = partition.X + Jitter(random, 1.8),
                Y = partition.Y + Jitter(random, 1.8),
                Width = Math.Max(54, partition.Width),
                Height = Math.Max(44, partition.Height),
                Rotation = 0,
                ShapeSeed = random.Next(1, int.MaxValue),
                Name = AreaName(RandomMapKind.Inn, index, count),
                Purpose = AreaPurpose(index, count)
            })
            .ToList();
    }

    private static List<RandomMapArea> GenerateVillageAreas(
        int count,
        Random random,
        RandomMapSettlement? settlement,
        RandomMapSettlementLayout layout)
    {
        var areas = new List<RandomMapArea>(count);
        var phase = random.NextDouble() * Math.PI * 2;

        for (var index = 0; index < count; index++)
        {
            var scale = layout == RandomMapSettlementLayout.Fortified ? 0.62 : 1;
            var width = (66 + random.NextDouble() * 58) * scale;
            var height = (48 + random.NextDouble() * 38) * scale;
            var placed = false;
            double x = 0;
            double y = 0;

            for (var attempt = 0; attempt < 900; attempt++)
            {
                (x, y) = VillageCandidate(layout, index, attempt, width, height, phase, random);
                y = Math.Clamp(y, Margin, RandomDungeonMap.CanvasHeight - Margin - height);
                x = Math.Clamp(x, Margin, RandomDungeonMap.CanvasWidth - Margin - width);

                var padding = layout == RandomMapSettlementLayout.Fortified ? 8 : 16;
                if (areas.All(area => !Intersects(area, x, y, width, height, padding)))
                {
                    placed = true;
                    break;
                }
            }

            if (!placed)
            {
                width = 62;
                height = 46;
                var scanMinX = layout == RandomMapSettlementLayout.Fortified ? 120 : Margin;
                var scanMinY = layout == RandomMapSettlementLayout.Fortified ? 120 : Margin;
                var scanMaxX = layout == RandomMapSettlementLayout.Fortified ? 840 - width : RandomDungeonMap.CanvasWidth - Margin - width;
                var scanMaxY = layout == RandomMapSettlementLayout.Fortified ? 520 - height : RandomDungeonMap.CanvasHeight - Margin - height;
                for (var scanY = scanMinY; scanY <= scanMaxY && !placed; scanY += 16)
                for (var scanX = scanMinX; scanX <= scanMaxX; scanX += 16)
                {
                    if (areas.All(area => !Intersects(area, scanX, scanY, width, height, 8)))
                    {
                        x = scanX;
                        y = scanY;
                        placed = true;
                        break;
                    }
                }
            }

            areas.Add(new RandomMapArea
            {
                Id = index + 1,
                X = x,
                Y = y,
                Width = width,
                Height = height,
                Rotation = Jitter(random, 13),
                ShapeSeed = random.Next(1, int.MaxValue),
                Name = VillageAreaName(settlement, index, count),
                Purpose = AreaPurpose(index, count)
            });
        }

        return areas;
    }

    private static (double X, double Y) VillageCandidate(
        RandomMapSettlementLayout layout,
        int index,
        int attempt,
        double width,
        double height,
        double phase,
        Random random)
    {
        if (layout == RandomMapSettlementLayout.Dispersed)
            return (
                Margin + random.NextDouble() * (RandomDungeonMap.CanvasWidth - Margin * 2 - width),
                Margin + random.NextDouble() * (RandomDungeonMap.CanvasHeight - Margin * 2 - height));

        if (layout == RandomMapSettlementLayout.Clustered)
        {
            var angle = random.NextDouble() * Math.PI * 2;
            var radius = 45 + Math.Sqrt(random.NextDouble()) * 245;
            return (
                RandomDungeonMap.CanvasWidth / 2d + Math.Cos(angle) * radius - width / 2,
                RandomDungeonMap.CanvasHeight / 2d + Math.Sin(angle) * radius * 0.72 - height / 2);
        }

        if (layout == RandomMapSettlementLayout.Fortified)
        {
            var angle = random.NextDouble() * Math.PI * 2;
            var radius = 36 + Math.Sqrt(random.NextDouble()) * 260;
            return (
                RandomDungeonMap.CanvasWidth / 2d + Math.Cos(angle) * radius - width / 2,
                RandomDungeonMap.CanvasHeight / 2d + Math.Sin(angle) * radius * 0.68 - height / 2);
        }

        var x = Margin + random.NextDouble() * (RandomDungeonMap.CanvasWidth - Margin * 2 - width);
        var roadY = VillageRoadY(x, phase);
        var side = (index + attempt / 7) % 2 == 0 ? -1 : 1;
        var distance = 48 + random.NextDouble() * 125;
        return (x, roadY + side * distance - height / 2 + Jitter(random, 24));
    }

    private static double VillageRoadY(double x, double phase) =>
        318 + Math.Sin(x / 145 + phase) * 82 + Math.Sin(x / 62 + phase * 0.7) * 18;

    private static bool Intersects(RandomMapArea area, double x, double y, double width, double height, double padding) =>
        area.X - padding < x + width && area.X + area.Width + padding > x &&
        area.Y - padding < y + height && area.Y + area.Height + padding > y;

    private static List<RandomMapPassage> ConnectAreas(IReadOnlyList<RandomMapArea> areas, int loops, Random random)
    {
        var passages = new List<RandomMapPassage>();
        var connected = new HashSet<int> { areas[0].Id };

        while (connected.Count < areas.Count)
        {
            var nearest = areas
                .Where(target => !connected.Contains(target.Id))
                .SelectMany(target => areas
                    .Where(source => connected.Contains(source.Id))
                    .Select(source => new { Source = source, Target = target, Distance = Distance(source, target) }))
                .OrderBy(candidate => candidate.Distance)
                .ThenBy(candidate => candidate.Target.Id)
                .First();

            passages.Add(NewPassage(nearest.Source.Id, nearest.Target.Id, random));
            connected.Add(nearest.Target.Id);
        }

        var desiredExtras = Math.Min(areas.Count * (areas.Count - 1) / 2 - passages.Count, loops * Math.Max(1, areas.Count / 8));
        var candidates = areas
            .SelectMany((left, index) => areas.Skip(index + 1).Select(right => (LeftId: left.Id, RightId: right.Id, Distance: Distance(left, right))))
            .Where(pair => !Contains(passages, pair.LeftId, pair.RightId))
            .OrderBy(pair => pair.Distance + random.NextDouble() * 180)
            .Take(desiredExtras);

        foreach (var candidate in candidates)
            passages.Add(NewPassage(candidate.LeftId, candidate.RightId, random));

        return passages;
    }

    private static RandomMapPassage NewPassage(int from, int to, Random random) => new()
    {
        FromAreaId = Math.Min(from, to),
        ToAreaId = Math.Max(from, to),
        Secret = random.NextDouble() < 0.14,
        Routing = (RandomMapPassageRouting)random.Next(0, 3)
    };

    private static List<RandomMapFeature> GenerateFeatures(RandomMapKind mapKind, IReadOnlyList<RandomMapArea> areas, Random random)
    {
        var features = new List<RandomMapFeature>();
        var nextId = 1;
        foreach (var area in areas)
        {
            AddFeature(features, ref nextId, area, PrimaryFeature(mapKind, area.Purpose, random), random);
            if (area.Purpose == RandomMapAreaPurpose.Chamber && random.NextDouble() < 0.4)
                AddFeature(features, ref nextId, area, AmbientFeature(mapKind, random), random);
        }
        return features;
    }

    private static void AddFeature(
        ICollection<RandomMapFeature> features,
        ref int nextId,
        RandomMapArea area,
        RandomMapFeatureKind kind,
        Random random)
    {
        const double padding = 16;
        var usableWidth = Math.Max(1, area.Width - padding * 2);
        var usableHeight = Math.Max(1, area.Height - padding * 2);
        features.Add(new RandomMapFeature
        {
            Id = nextId++,
            AreaId = area.Id,
            X = area.X + padding + random.NextDouble() * usableWidth,
            Y = area.Y + padding + random.NextDouble() * usableHeight,
            Kind = kind
        });
    }

    private static RandomMapFeatureKind PrimaryFeature(RandomMapKind mapKind, RandomMapAreaPurpose purpose, Random random) => purpose switch
    {
        RandomMapAreaPurpose.Entrance => RandomMapFeatureKind.Light,
        RandomMapAreaPurpose.Hazard => RandomMapFeatureKind.Trap,
        RandomMapAreaPurpose.Treasure => RandomMapFeatureKind.Chest,
        RandomMapAreaPurpose.Shrine => RandomMapFeatureKind.Altar,
        RandomMapAreaPurpose.Exit => RandomMapFeatureKind.Stairs,
        _ => AmbientFeature(mapKind, random)
    };

    private static RandomMapFeatureKind AmbientFeature(RandomMapKind mapKind, Random random)
    {
        RandomMapFeatureKind[] choices = mapKind switch
        {
            RandomMapKind.Inn =>
            [
                RandomMapFeatureKind.Npc, RandomMapFeatureKind.Table, RandomMapFeatureKind.Bed,
                RandomMapFeatureKind.Barrel, RandomMapFeatureKind.Light, RandomMapFeatureKind.Chest
            ],
            RandomMapKind.Village =>
            [
                RandomMapFeatureKind.Npc, RandomMapFeatureKind.Well, RandomMapFeatureKind.Forge,
                RandomMapFeatureKind.Tree, RandomMapFeatureKind.Wagon, RandomMapFeatureKind.Campfire
            ],
            RandomMapKind.Dungeon =>
            [
                RandomMapFeatureKind.Npc, RandomMapFeatureKind.Brazier, RandomMapFeatureKind.Sarcophagus,
                RandomMapFeatureKind.Rubble, RandomMapFeatureKind.Statue, RandomMapFeatureKind.Chest
            ],
            _ =>
            [
                RandomMapFeatureKind.Npc, RandomMapFeatureKind.Light, RandomMapFeatureKind.Well,
                RandomMapFeatureKind.Campfire, RandomMapFeatureKind.Chest
            ]
        };
        return choices[random.Next(choices.Length)];
    }

    private static bool Contains(IEnumerable<RandomMapPassage> passages, int left, int right) =>
        passages.Any(passage =>
            passage.FromAreaId == Math.Min(left, right) && passage.ToAreaId == Math.Max(left, right));

    private static double Distance(RandomMapArea left, RandomMapArea right) =>
        Math.Sqrt(Math.Pow(left.CenterX - right.CenterX, 2) + Math.Pow(left.CenterY - right.CenterY, 2));

    private static double Jitter(Random random, double amplitude) => (random.NextDouble() * 2 - 1) * amplitude;

    private static double Snap(double value, int step) => Math.Round(value / step) * step;

    private static string AreaName(RandomMapKind kind, int index, int count)
    {
        if (index == 0)
            return kind switch
            {
                RandomMapKind.Dungeon => "Entrée du donjon",
                RandomMapKind.Cave => "Bouche de la grotte",
                RandomMapKind.Inn => "Porche de l’auberge",
                RandomMapKind.Village => "Route du village",
                _ => "Entrée"
            };
        if (index == count - 1)
            return kind switch
            {
                RandomMapKind.Dungeon => "Issue secrète",
                RandomMapKind.Cave => "Faille vers la surface",
                RandomMapKind.Inn => "Escalier vers l’étage",
                RandomMapKind.Village => "Chemin vers les champs",
                _ => "Sortie"
            };
        var names = kind switch
        {
            RandomMapKind.Dungeon => DungeonNames,
            RandomMapKind.Cave => CaveNames,
            RandomMapKind.Inn => InnNames,
            RandomMapKind.Village => VillageNames,
            _ => DungeonNames
        };
        return names[(index - 1) % names.Length];
    }

    private static string VillageAreaName(RandomMapSettlement? settlement, int index, int count)
    {
        if (settlement is null)
            return AreaName(RandomMapKind.Village, index, count);
        if (index == 0)
            return settlement.Type is "Ville" or "Cité"
                ? $"Porte de {settlement.Name}"
                : $"Entrée de {settlement.Name}";
        if (index == count - 1)
            return $"Route de {ShortRegion(settlement.Region)}";

        var names = settlement.Type switch
        {
            "Ville" or "Cité" => CityNames,
            "Bourg" => TownNames,
            _ => VillageNames
        };
        return names[(index - 1) % names.Length];
    }

    private static string ShortRegion(string region)
    {
        var parts = region.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return parts.Length == 0 ? region : parts[^1];
    }

    private static int? ParsePopulation(string population)
    {
        var digits = new string(population.Where(char.IsDigit).ToArray());
        return int.TryParse(digits, out var value) ? value : null;
    }

    private static RandomMapAreaPurpose AreaPurpose(int index, int count)
    {
        if (index == 0) return RandomMapAreaPurpose.Entrance;
        if (index == count - 1) return RandomMapAreaPurpose.Exit;
        if (index % 7 == 0) return RandomMapAreaPurpose.Treasure;
        if (index % 5 == 0) return RandomMapAreaPurpose.Shrine;
        if (index % 3 == 0) return RandomMapAreaPurpose.Hazard;
        return RandomMapAreaPurpose.Chamber;
    }

    private sealed record MapPartition(double X, double Y, double Width, double Height);
}
