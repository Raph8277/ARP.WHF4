using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Wfrp4.Shared.Maps;

public sealed class RandomDungeonMapProject
{
    public const int CurrentVersion = 1;

    public int Version { get; set; } = CurrentVersion;
    public RandomDungeonMap Map { get; set; } = new();
    public int AreaCount { get; set; } = 14;
    public int Loops { get; set; } = 2;
    public bool ShowGrid { get; set; }
    public bool ShowLabels { get; set; } = true;
    public bool ShowFeatures { get; set; } = true;
}

public static class RandomDungeonMapProjectSerializer
{
    public const int MaxProjectBytes = 1_048_576;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public static string Serialize(RandomDungeonMapProject project)
    {
        ArgumentNullException.ThrowIfNull(project);
        return JsonSerializer.Serialize(project, JsonOptions);
    }

    public static bool TryDeserialize(
        string json,
        out RandomDungeonMapProject? project,
        out string error)
    {
        project = null;
        error = string.Empty;

        if (string.IsNullOrWhiteSpace(json))
        {
            error = "Le fichier de projet est vide.";
            return false;
        }

        if (Encoding.UTF8.GetByteCount(json) > MaxProjectBytes)
        {
            error = "Le fichier de projet dépasse la taille maximale de 1 Mio.";
            return false;
        }

        try
        {
            project = JsonSerializer.Deserialize<RandomDungeonMapProject>(json, JsonOptions);
        }
        catch (JsonException)
        {
            error = "Le fichier n’est pas un projet de carte JSON valide.";
            return false;
        }

        if (project is null)
        {
            error = "Le fichier ne contient aucun projet de carte.";
            return false;
        }

        if (!TryValidate(project, out error))
        {
            project = null;
            return false;
        }

        return true;
    }

    private static bool TryValidate(RandomDungeonMapProject project, out string error)
    {
        error = string.Empty;
        if (project.Version != RandomDungeonMapProject.CurrentVersion)
            return Fail("Cette version de projet de carte n’est pas prise en charge.", out error);

        if (project.Map is null)
            return Fail("Le projet ne contient pas de carte.", out error);

        if (!Enum.IsDefined(project.Map.Kind) || project.Map.Seed < 1)
            return Fail("Le type ou la graine de la carte est invalide.", out error);

        if (!Enum.IsDefined(project.Map.SettlementSize) ||
            project.Map.SettlementFootprintPercent is < 65 or > 160 ||
            project.Map.SettlementDensityPercent is < 65 or > 140 ||
            project.Map.SettlementStreetWidthPercent is < 60 or > 150 ||
            project.Map.SettlementForm is { } settlementForm && !Enum.IsDefined(settlementForm))
            return Fail("Les paramètres de ville sont hors limites.", out error);

        if (string.IsNullOrWhiteSpace(project.Map.Name) || project.Map.Name.Length > 120)
            return Fail("Le nom de la carte est invalide.", out error);

        if (project.AreaCount is < 4 or > 30 || project.Loops is < 0 or > 6)
            return Fail("Les paramètres de génération sont hors limites.", out error);

        if (project.Map.Areas is null || project.Map.Areas.Count is < 1 or > 30)
            return Fail("Le projet doit contenir entre 1 et 30 zones.", out error);

        if (project.Map.Passages is null || project.Map.Features is null ||
            project.Map.SettlementBoundary is null || project.Map.Districts is null)
            return Fail("La structure de la carte est incomplète.", out error);

        if (project.Map.SettlementBoundary.Count > 0 &&
            (project.Map.SettlementBoundary.Count < 3 || project.Map.SettlementBoundary.Any(point => !ValidPoint(point))))
            return Fail("L’enceinte de la localité est invalide.", out error);

        var districtIds = new HashSet<int>();
        foreach (var district in project.Map.Districts)
        {
            if (district.Id < 1 || !districtIds.Add(district.Id) || string.IsNullOrWhiteSpace(district.Name) ||
                district.Name.Length > 120 || !Enum.IsDefined(district.Kind) || district.Boundary is null ||
                district.Boundary.Count < 3 || district.Boundary.Any(point => !ValidPoint(point)))
                return Fail("Un quartier de la localité est invalide.", out error);
        }

        var areaIds = new HashSet<int>();
        foreach (var area in project.Map.Areas)
        {
            if (area.Id < 1 || !areaIds.Add(area.Id) || string.IsNullOrWhiteSpace(area.Name) || area.Name.Length > 120 ||
                !Enum.IsDefined(area.Purpose) || !AreFinite(area.X, area.Y, area.Width, area.Height, area.Rotation) ||
                area.Width <= 0 || area.Height <= 0 || area.X < 0 || area.Y < 0 ||
                area.X + area.Width > RandomDungeonMap.CanvasWidth || area.Y + area.Height > RandomDungeonMap.CanvasHeight)
                return Fail("Une zone du projet est invalide ou sort de la carte.", out error);
        }

        var passageKeys = new HashSet<(int, int)>();
        foreach (var passage in project.Map.Passages)
        {
            var key = (Math.Min(passage.FromAreaId, passage.ToAreaId), Math.Max(passage.FromAreaId, passage.ToAreaId));
            if (passage.FromAreaId == passage.ToAreaId || !areaIds.Contains(passage.FromAreaId) ||
                !areaIds.Contains(passage.ToAreaId) || !Enum.IsDefined(passage.Routing) || !passageKeys.Add(key))
                return Fail("Un passage du projet est invalide.", out error);
        }

        var featureIds = new HashSet<int>();
        foreach (var feature in project.Map.Features)
        {
            var area = project.Map.Areas.FirstOrDefault(candidate => candidate.Id == feature.AreaId);
            if (feature.Id < 1 || !featureIds.Add(feature.Id) || area is null || !Enum.IsDefined(feature.Kind) ||
                !AreFinite(feature.X, feature.Y) || feature.X < area.X || feature.X > area.X + area.Width ||
                feature.Y < area.Y || feature.Y > area.Y + area.Height)
                return Fail("Un détail du projet est invalide ou placé hors de sa zone.", out error);
        }

        return true;
    }

    private static bool AreFinite(params double[] values) => values.All(double.IsFinite);

    private static bool ValidPoint(RandomMapPoint point) =>
        point is not null && AreFinite(point.X, point.Y) &&
        point.X is >= 0 and <= RandomDungeonMap.CanvasWidth &&
        point.Y is >= 0 and <= RandomDungeonMap.CanvasHeight;

    private static bool Fail(string message, out string error)
    {
        error = message;
        return false;
    }
}
