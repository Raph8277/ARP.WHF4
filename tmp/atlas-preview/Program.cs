using Wfrp4.Shared.Maps;
var directory = Path.GetFullPath("tmp/atlas-preview");
foreach (var type in new[] { "Village", "Ville", "Cité" })
{
    var map = RandomDungeonMapGenerator.Generate(RandomMapKind.Village, 20, 2, 908875339,
        new RandomMapSettlement("Doverley — " + type, type, "2500", "Reikland"));
    var plan = SettlementAtlas.Generate(map);
    File.WriteAllText(Path.Combine(directory, type + ".svg"),
        "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"1440\" height=\"960\" viewBox=\"0 0 960 640\">" +
        SettlementAtlas.Render(plan, map.Name, true) + "</svg>");
    Console.WriteLine($"{type}: {plan.Buildings.Count} bâtiments, {plan.Streets.Count} rues");
}
