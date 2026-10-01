using System.Globalization;
using System.Net;
using System.Text;

namespace Wfrp4.Shared.Maps;

/// <summary>A deterministic, top-down settlement atlas. No external assets or scripts.</summary>
public static class SettlementAtlas
{
    public readonly record struct Point(double X, double Y)
    {
        public static Point operator +(Point a, Point b) => new(a.X + b.X, a.Y + b.Y);
        public static Point operator -(Point a, Point b) => new(a.X - b.X, a.Y - b.Y);
        public static Point operator *(Point a, double n) => new(a.X * n, a.Y * n);
    }
    public sealed record Building(Point[] Outline, int Ward);
    public sealed record Street(Point A, Point B);
    public sealed record Plan(List<Building> Buildings, List<Street> Streets, List<Point> Trees,
        List<Point[]> Gardens, Point Market, Point Castle, double Phase, RandomMapSettlementLandscape Landscape,
        bool HasRiver, bool HasCoast, bool HasForest, double Scale, double FormX, double FormY,
        double StreetWidth);

    private static double Dot(Point a, Point b) => a.X * b.X + a.Y * b.Y;
    private static double Length(Point p) => Math.Sqrt(Dot(p, p));
    public static double RiverX(double y, double phase) => 645 + 36 * Math.Sin(y / 115 + phase);

    public static Plan Generate(RandomDungeonMap map)
    {
        var random = new Random(map.Seed ^ 0x517bc2);
        var phase = random.NextDouble() * 6.28;
        var landscape = map.SettlementLandscape ?? RandomMapSettlementLandscape.River;
        var hasRiver = map.SettlementHasRiver ?? landscape == RandomMapSettlementLandscape.River;
        var hasCoast = map.SettlementHasCoast ?? landscape == RandomMapSettlementLandscape.Port;
        var hasForest = map.SettlementHasForest ?? false;
        var city = map.SettlementType is "Cité" or "Ville";
        var baseScale = map.SettlementType == "Cité" ? 1.23 : city ? 1.12 : map.SettlementType == "Bourg" ? 1 : 0.8;
        var sizeFactor = map.SettlementFootprintPercent is { } footprint
            ? Math.Clamp(footprint, 65, 160) / 100d
            : map.SettlementSize switch { RandomMapSettlementSize.Small => .78, RandomMapSettlementSize.Large => 1.3, _ => 1d };
        var scale = baseScale * sizeFactor;
        var density = Math.Clamp(map.SettlementDensityPercent ?? 100, 65, 140) / 100d;
        var spacing = (map.SettlementType == "Cité" ? 40d : city ? 45d : 58d) / density;
        var form = map.SettlementForm ?? map.SettlementLayout switch
        {
            RandomMapSettlementLayout.Linear => RandomMapSettlementForm.Elongated,
            RandomMapSettlementLayout.Dispersed => RandomMapSettlementForm.Sprawling,
            _ => RandomMapSettlementForm.Compact
        };
        var (formX, formY) = form switch
        {
            RandomMapSettlementForm.Elongated => (.82, 1.2),
            RandomMapSettlementForm.Sprawling => (1.22, 1.05),
            _ => (.94, .94)
        };
        var streetWidth = Math.Clamp(map.SettlementStreetWidthPercent ?? 100, 60, 150) / 100d;
        var sites = new List<Point>();
        for (var y = -35d; y < 720; y += spacing)
        for (var x = -35d; x < 1040; x += spacing)
            sites.Add(new Point(x + random.NextDouble() * spacing * .7,
                y + random.NextDouble() * spacing * .7));
        var buildings = new List<Building>();
        var streets = new List<Street>();
        var gardens = new List<Point[]>();
        var roadKeys = new HashSet<string>();
        var occupied = new List<Point[]>();
        var buildingBuckets = new Dictionary<(int X, int Y), List<int>>();
        var nearbyBuildings = new HashSet<int>();
        var market = sites.MinBy(p => Length(p - new Point(443, 293)));
        var castle = sites.MinBy(p => Length(p - new Point(355, 242)));
        foreach (var site in sites)
        {
            var dx = (site.X - 475) / (238 * scale * formX);
            var dy = (site.Y - 298) / (153 * scale * formY);
            var core = dx * dx + dy * dy < 1 + .14 * Math.Sin(site.X / 41 + phase);
            var tail = Math.Pow((site.X - 590) / (75 * scale * formX), 2) + Math.Pow((site.Y - 467) / (132 * scale * formY), 2) < 1;
            if (!core && !tail) continue;
            if (site.Y < 95 || site.Y > 575 || site.X < 140 || site.X > 822) continue;
            if (hasCoast && site.X > 695) continue;
            var cell = new List<Point> { new(55, 85), new(890, 85), new(890, 595), new(55, 595) };
            foreach (var other in sites)
            {
                if (site == other || Length(site - other) > spacing * 3) continue;
                var normal = other - site;
                cell = Clip(cell, normal, (Dot(other, other) - Dot(site, site)) / 2);
                if (cell.Count < 3) break;
            }
            if (cell.Count < 3) continue;
            occupied.Add(cell.ToArray());
            for (var i = 0; i < cell.Count; i++)
            {
                var a = cell[i]; var b = cell[(i + 1) % cell.Count];
                var ka = $"{Math.Round(a.X, 1)},{Math.Round(a.Y, 1)}";
                var kb = $"{Math.Round(b.X, 1)},{Math.Round(b.Y, 1)}";
                var key = string.CompareOrdinal(ka, kb) < 0 ? ka + ":" + kb : kb + ":" + ka;
                if (roadKeys.Add(key)) streets.Add(new(a, b));
            }
            if (site == market || site == castle) continue;
            var gardenChance = Math.Clamp(.09 - (density - 1) * .1, .015, .16);
            if (random.NextDouble() < gardenChance)
            {
                gardens.Add(cell.Select(p => site + (p - site) * .8).ToArray());
                continue;
            }
            var ward = site.Y > 423 ? 3 : site.X > 575 ? 2 : site.Y < 265 ? 0 : 1;
            for (var edge = 0; edge < cell.Count; edge++)
            {
                var a = cell[edge]; var b = cell[(edge + 1) % cell.Count];
                var length = Length(b - a);
                if (length < 10) continue;
                var tangent = (b - a) * (1 / length);
                var inward = new Point(-tangent.Y, tangent.X);
                if (Dot(inward, site - a) < 0) inward = inward * -1;
                for (var distance = 5d; distance < length - 9;)
                {
                    var width = (6 + random.NextDouble() * 5) / Math.Sqrt(density);
                    var depth = (8 + random.NextDouble() * 7) / Math.Sqrt(density);
                    var start = a + tangent * distance + inward * 3.5;
                    var outline = new[] { start, start + tangent * width,
                        start + tangent * width + inward * depth, start + inward * depth };
                    distance += width + Math.Max(1, 1.5 / density);
                    if (tail && !core && random.NextDouble() < .34) continue;
                    if (outline.Any(p => !Contains(cell, p, 2))) continue;
                    if (outline.Any(p => IsWater(p, hasRiver, hasCoast, phase))) continue;
                    if (OverlapsNearbyBuilding(outline, buildings, buildingBuckets, nearbyBuildings)) continue;
                    var buildingIndex = buildings.Count;
                    buildings.Add(new(outline, ward));
                    foreach (var cellKey in BucketKeys(outline))
                    {
                        if (!buildingBuckets.TryGetValue(cellKey, out var bucket))
                            buildingBuckets[cellKey] = bucket = [];
                        bucket.Add(buildingIndex);
                    }
                }
            }
        }
        // Approach roads end at actual street vertices, keeping the network connected.
        if (streets.Count > 0)
        {
            var vertices = streets.SelectMany(s => new[] { s.A, s.B }).ToList();
            foreach (var entry in new[] { new Point(-10, 340), new Point(970, 315), new Point(325, 650) })
            {
                var target = vertices.MinBy(p => Length(p - entry));
                streets.Add(new(entry, target));
            }
        }
        var trees = new List<Point>();
        for (var i = 0; i < 1300 && trees.Count < (city ? 145 : 100); i++)
        {
            var p = new Point(175 + random.NextDouble() * 635, 115 + random.NextDouble() * 465);
            if (IsWater(p, hasRiver, hasCoast, phase) || Length(p - market) < 26 || Length(p - castle) < 35) continue;
            if (buildings.Any(b => Contains(b.Outline, p, -4))) continue;
            if (streets.Any(s => Distance(p, s.A, s.B) < 7)) continue;
            if (trees.Any(t => Length(t - p) < 8)) continue;
            if (!occupied.Any(c => Contains(c, p, -15))) continue;
            trees.Add(p);
        }
        if (hasForest)
        {
            var clusters = new[] { new Point(185, 255), new Point(735, 175), new Point(760, 535), new Point(260, 515) };
            foreach (var center in clusters)
            for (var i = 0; i < 34; i++)
            {
                var angle = random.NextDouble() * Math.PI * 2;
                var radius = 24 + random.NextDouble() * 60;
                var p = center + new Point(Math.Cos(angle) * radius, Math.Sin(angle) * radius);
                if (p.X is < 65 or > 885 || p.Y is < 85 or > 595 || IsWater(p, hasRiver, hasCoast, phase)) continue;
                trees.Add(p);
            }
        }
        return new(buildings, streets, trees, gardens, market, castle, phase, landscape, hasRiver, hasCoast, hasForest, scale, formX, formY, streetWidth);
    }

    /// <summary>Moves editable place markers onto the settlement's inner land area.</summary>
    public static Plan? ConstrainPlaces(RandomDungeonMap map)
    {
        if (map.Kind != RandomMapKind.Village || map.Areas.Count == 0) return null;
        var plan = Generate(map);
        ConstrainPlaces(map, plan);
        return plan;
    }

    public static void ConstrainPlaces(RandomDungeonMap map, Plan plan)
    {
        if (map.Kind != RandomMapKind.Village || map.Areas.Count == 0) return;
        var candidates = new List<Point>();
        for (var y = 150d; y <= 470; y += 6)
        for (var x = 195d; x <= 720; x += 6)
        {
            var dx = (x - 475) / (238 * plan.Scale * plan.FormX);
            var dy = (y - 298) / (153 * plan.Scale * plan.FormY);
            var core = dx * dx + dy * dy < .48 + .08 * Math.Sin(x / 41 + plan.Phase);
            var tail = Math.Pow((x - 590) / (75 * plan.Scale * plan.FormX), 2) + Math.Pow((y - 467) / (132 * plan.Scale * plan.FormY), 2) < .48;
            var point = new Point(x, y);
            if ((core || tail) && !IsWater(point, plan.HasRiver, plan.HasCoast, plan.Phase)) candidates.Add(point);
        }
        var used = new List<Point>();
        foreach (var area in map.Areas.OrderBy(area => area.Id))
        {
            var old = new Point(area.CenterX, area.CenterY);
            var target = candidates
                .OrderBy(point => Length(point - old) + used.Sum(previous => Math.Max(0, 46 - Length(point - previous)) * 8))
                .FirstOrDefault();
            if (target == default) continue;
            var dx = target.X - area.CenterX;
            var dy = target.Y - area.CenterY;
            area.X += dx;
            area.Y += dy;
            foreach (var feature in map.Features.Where(feature => feature.AreaId == area.Id))
            {
                feature.X += dx;
                feature.Y += dy;
            }
            used.Add(target);
        }
    }

    public static string Render(Plan plan, string name, bool labels)
    {
        var svg = new StringBuilder("<g class=\"settlement-atlas\" pointer-events=\"none\"><rect width=\"960\" height=\"640\" fill=\"#c5c5b7\"/>");
        if (plan.HasForest)
            svg.Append("<path d=\"M 55 0 H 285 Q 335 100 268 150 Q 180 205 55 160 Z M 55 485 Q 160 444 284 500 L 320 640 H 55 Z M 730 0 H 960 V 158 Q 845 210 746 157 Z M 760 477 Q 850 440 960 485 V 640 H 776 Z\" fill=\"#aab394\" opacity=\".55\"/>");
        foreach (var garden in plan.Gardens)
            svg.Append($"<path d=\"{Path(garden, true)}\" fill=\"#b5bd9d\" stroke=\"#999f88\" stroke-width=\".6\"/>");
        var streetPath = string.Join(" ", plan.Streets.Select(street => Path([street.A, street.B])));
        svg.Append($"<path d=\"{streetPath}\" fill=\"none\" stroke=\"#7c776c\" stroke-width=\"{F(3.4 * plan.StreetWidth)}\" stroke-linejoin=\"round\"/>");
        svg.Append($"<path d=\"{streetPath}\" fill=\"none\" stroke=\"#e9e1cf\" stroke-width=\"{F(2.6 * plan.StreetWidth)}\" stroke-linejoin=\"round\"/>");
        if (plan.HasRiver)
        {
            var river = Enumerable.Range(0, 71).Select(i => new Point(RiverX(i * 10 - 20, plan.Phase), i * 10 - 20)).ToArray();
            svg.Append($"<path d=\"{Path(river)}\" fill=\"none\" stroke=\"#637e80\" stroke-width=\"19\"/><path d=\"{Path(river)}\" fill=\"none\" stroke=\"#94aeaf\" stroke-width=\"15\"/>");
            // Bridge decks are exactly the street portions crossing the water.
            var bridgeStart = default(Point);
            var bridgeEnd = default(Point);
            var bridgeActive = false;
            void FlushBridge()
            {
                if (bridgeActive)
                {
                    var bridge = Path([bridgeStart, bridgeEnd]);
                    svg.Append($"<path d=\"{bridge}\" stroke=\"#665e52\" stroke-width=\"{F(6 * plan.StreetWidth)}\" stroke-linecap=\"round\"/><path d=\"{bridge}\" stroke=\"#e9e1cf\" stroke-width=\"{F(4 * plan.StreetWidth)}\" stroke-linecap=\"round\"/>");
                }
                bridgeActive = false;
            }
            foreach (var street in plan.Streets)
            {
                if (Math.Max(street.A.X, street.B.X) < 597 || Math.Min(street.A.X, street.B.X) > 693)
                    continue;
                var previous = street.A;
                var segments = Math.Clamp((int)Math.Ceiling(Length(street.B - street.A) / 3), 8, 400);
                for (var i = 1; i <= segments; i++)
                {
                    var next = street.A + (street.B - street.A) * (i / (double)segments);
                    var onBridge = Math.Abs(previous.X - RiverX(previous.Y, plan.Phase)) < 12 ||
                                   Math.Abs(next.X - RiverX(next.Y, plan.Phase)) < 12;
                    if (onBridge)
                    {
                        if (!bridgeActive) bridgeStart = previous;
                        bridgeEnd = next;
                        bridgeActive = true;
                    }
                    else if (bridgeActive) FlushBridge();
                    previous = next;
                }
                FlushBridge();
            }
        }
        if (plan.HasCoast)
        {
            svg.Append("<path d=\"M 795 0 Q 755 160 790 300 T 770 640 H 960 V 0 Z\" fill=\"#94aeaf\" stroke=\"#637e80\" stroke-width=\"2\"/>");
            for (var y = 240; y < 480; y += 48)
                svg.Append($"<path d=\"M 695 {y} H 755\" stroke=\"#726a59\" stroke-width=\"6\"/>");
        }
        string[] roofs = ["#717f88", "#7d798e", "#6d898c", "#868399"];
        for (var ward = 0; ward < roofs.Length; ward++)
        {
            var outlines = plan.Buildings.Where(building => building.Ward == ward).Select(building => Path(building.Outline, true));
            svg.Append($"<path d=\"{string.Join(" ", outlines)}\" fill=\"{roofs[ward]}\" stroke=\"#51575a\" stroke-width=\".65\"/>");
        }
        var treePath = string.Join(" ", plan.Trees.Select(tree =>
            $"M {F(tree.X - 3)} {F(tree.Y - 2)} q -3 -5 2 -5 q 5 -3 6 2 q 4 3 0 5 q -3 4 -6 1 Z"));
        if (treePath.Length > 0)
            svg.Append($"<path d=\"{treePath}\" fill=\"#939e7b\" stroke=\"#6d745e\" stroke-width=\".65\"/>");
        svg.Append($"<g transform=\"translate({F(plan.Castle.X)} {F(plan.Castle.Y)}) rotate(-12)\"><rect x=\"-19\" y=\"-23\" width=\"38\" height=\"46\" fill=\"#e9e1cf\" stroke=\"#79786d\" stroke-width=\"3\"/><rect x=\"-10\" y=\"-13\" width=\"20\" height=\"26\" fill=\"#6c8887\" stroke=\"#4f6260\"/></g>");
        svg.Append($"<circle cx=\"{F(plan.Market.X)}\" cy=\"{F(plan.Market.Y)}\" r=\"4\" fill=\"#94aeaf\" stroke=\"#716b5c\" stroke-width=\"2\"/>");
        svg.Append($"<text x=\"48\" y=\"58\" font-family=\"Georgia,serif\" font-size=\"32\" fill=\"#51483f\">{WebUtility.HtmlEncode(name)}</text>");
        svg.Append("<path d=\"M 877 44 V 85 M 865 65 H 889 M 877 44 L 873 53 L 881 53 Z\" fill=\"#51483f\" stroke=\"#51483f\"/><text x=\"877\" y=\"36\" text-anchor=\"middle\" font-family=\"Georgia,serif\" font-size=\"14\" fill=\"#51483f\">N</text>");
        if (labels)
        {
            foreach (var label in new[] { ("LES ARTISANS", 405, 193), ("VIEUX BOURG", 433, 351),
                         ("LES QUAIS", 686, 268), ("LES FORGES", 566, 514) })
                svg.Append($"<text x=\"{label.Item2}\" y=\"{label.Item3}\" text-anchor=\"middle\" font-family=\"Georgia,serif\" font-size=\"18\" letter-spacing=\"3\" fill=\"#554a43\" stroke=\"#d7d6c7\" stroke-width=\"2\" paint-order=\"stroke\">{(label.Item1 == "LES QUAIS" && !plan.HasCoast ? "LES JARDINS" : label.Item1)}</text>");
            svg.Append($"<text x=\"{F(plan.Market.X)}\" y=\"{F(plan.Market.Y - 12)}\" text-anchor=\"middle\" font-family=\"Georgia,serif\" font-size=\"10\" fill=\"#554a43\">MARCHÉ</text>");
        }
        return svg.Append("</g>").ToString();
    }

    private static bool IsWater(Point p, bool hasRiver, bool hasCoast, double phase) =>
        hasRiver && Math.Abs(p.X - RiverX(p.Y, phase)) < 14 || hasCoast && p.X > 770;

    private static bool OverlapsNearbyBuilding(Point[] outline, List<Building> buildings,
        Dictionary<(int X, int Y), List<int>> buckets, HashSet<int> nearby)
    {
        nearby.Clear();
        foreach (var key in BucketKeys(outline))
            if (buckets.TryGetValue(key, out var bucket))
                foreach (var index in bucket) nearby.Add(index);
        foreach (var index in nearby)
            if (Overlap(outline, buildings[index].Outline)) return true;
        return false;
    }

    private static IEnumerable<(int X, int Y)> BucketKeys(Point[] outline)
    {
        const double bucketSize = 32;
        var minX = (int)Math.Floor((outline.Min(p => p.X) - 2) / bucketSize);
        var maxX = (int)Math.Floor((outline.Max(p => p.X) + 2) / bucketSize);
        var minY = (int)Math.Floor((outline.Min(p => p.Y) - 2) / bucketSize);
        var maxY = (int)Math.Floor((outline.Max(p => p.Y) + 2) / bucketSize);
        for (var x = minX; x <= maxX; x++)
        for (var y = minY; y <= maxY; y++)
            yield return (x, y);
    }

    private static List<Point> Clip(List<Point> polygon, Point normal, double limit)
    {
        var result = new List<Point>();
        for (var i = 0; i < polygon.Count; i++)
        {
            var a = polygon[i]; var b = polygon[(i + 1) % polygon.Count];
            var da = Dot(a, normal) - limit; var db = Dot(b, normal) - limit;
            if (da <= 0) result.Add(a);
            if ((da <= 0) != (db <= 0)) result.Add(a + (b - a) * (da / (da - db)));
        }
        return result;
    }
    private static bool Contains(IReadOnlyList<Point> polygon, Point p, double margin)
    {
        for (var i = 0; i < polygon.Count; i++)
        {
            var edge = polygon[(i + 1) % polygon.Count] - polygon[i];
            var offset = p - polygon[i];
            if (edge.X * offset.Y - edge.Y * offset.X < margin * Length(edge)) return false;
        }
        return polygon.Count >= 3;
    }
    public static bool Overlap(Point[] a, Point[] b)
    {
        foreach (var polygon in new[] { a, b })
        for (var i = 0; i < polygon.Length; i++)
        {
            var edge = polygon[(i + 1) % polygon.Length] - polygon[i];
            var normal = new Point(-edge.Y, edge.X);
            if (a.Max(p => Dot(p, normal)) < b.Min(p => Dot(p, normal)) - Length(normal) ||
                b.Max(p => Dot(p, normal)) < a.Min(p => Dot(p, normal)) - Length(normal)) return false;
        }
        return true;
    }
    private static double Distance(Point p, Point a, Point b)
    {
        var edge = b - a;
        var t = Math.Clamp(Dot(p - a, edge) / Math.Max(.001, Dot(edge, edge)), 0, 1);
        return Length(p - (a + edge * t));
    }
    private static string F(double n) => n.ToString("0.##", CultureInfo.InvariantCulture);
    private static string Path(Point[] points, bool close = false) =>
        "M " + string.Join(" L ", points.Select(p => F(p.X) + " " + F(p.Y))) + (close ? " Z" : "");
}
