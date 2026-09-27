namespace Wfrp4.Shared.Adventures;

public sealed record AdventureNarrativeAct(string Title, string Description, string Location);

public sealed record AdventureNarrativeInput(
    string BaseSynopsis,
    string Region,
    string Location,
    string Atmosphere,
    string Hook,
    string Threat,
    string Twist,
    string Climax,
    string Danger,
    string Duration,
    IReadOnlyList<AdventureNarrativeAct> Acts,
    string? CampaignLink);

public sealed record AdventureNarrative(string PlayerSynopsis, string GameMasterSynopsis);

public static class AdventureNarrativeComposer
{
    public static string ComposePitch(string theme, string location)
    {
        var frame = theme switch
        {
            "Intrigue" => "Des alliances fragiles, des témoignages contradictoires et des intérêts cachés empêchent encore d'identifier le véritable responsable.",
            "Exploration" => "Pour atteindre la source du danger, les personnages devront quitter les chemins sûrs et suivre des traces que d'autres préféreraient voir disparaître.",
            "Combat" => "La menace rassemble déjà ses forces, et chaque heure perdue rendra l'affrontement plus coûteux pour les habitants.",
            "Horreur" => "Les premiers signes défient la raison, tandis que la peur et les superstitions isolent ceux qui pourraient encore témoigner.",
            "Politique" => "Les autorités et les factions locales cherchent moins à résoudre la crise qu'à en tirer avantage ou à désigner un coupable commode.",
            _ => "Les apparences dissimulent une crise plus profonde dont les personnages devront découvrir l'origine."
        };

        return JoinSentences($"Le point de départ est {location}", frame);
    }

    public static AdventureNarrative Compose(AdventureNarrativeInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var route = BuildRoute(input.Acts);
        var introduction = JoinSentences(
            input.Atmosphere,
            input.BaseSynopsis,
            $"Le cadre initial se situe à {input.Location}; la situation concerne {input.Region}.");

        var playerIncitingIncident = JoinSentences(
            input.Hook,
            "Ce qui paraît d'abord être un incident isolé révèle peu à peu une crise capable de bouleverser durablement la région.");
        var playerProgression = JoinSentences(
            route.Length == 0 ? null : $"L'enquête conduira les personnages le long de l'itinéraire suivant : {route}.",
            PlayerStakes(input.Danger, input.Region));

        var gameMasterThreat = JoinSentences(
            input.Hook,
            $"Derrière les premiers signes se cache la véritable menace : {input.Threat}");
        var gameMasterProgression = JoinSentences(
            route.Length == 0 ? null : $"La progression dramatique mène les personnages de {route}.",
            BuildActSummary(input.Acts));
        var gameMasterResolution = JoinSentences(
            input.Twist,
            input.Climax,
            FailureConsequence(input.Threat, input.Danger, input.Region),
            input.CampaignLink);

        return new AdventureNarrative(
            PlayerSynopsis: JoinParagraphs(introduction, playerIncitingIncident, playerProgression),
            GameMasterSynopsis: JoinParagraphs(introduction, gameMasterThreat, gameMasterProgression, gameMasterResolution));
    }

    private static string BuildRoute(IReadOnlyList<AdventureNarrativeAct>? acts) =>
        acts is null
            ? string.Empty
            : string.Join(" → ", acts
                .Select(act => act.Location?.Trim())
                .Where(location => !string.IsNullOrWhiteSpace(location))
                .Distinct(StringComparer.OrdinalIgnoreCase));

    private static string BuildActSummary(IReadOnlyList<AdventureNarrativeAct>? acts)
    {
        if (acts is null || acts.Count == 0)
            return string.Empty;

        return "Le déroulement proposé est le suivant : " + string.Join(' ', acts.Select((act, index) =>
            $"Acte {index + 1}, « {act.Title} » : {act.Description}"));
    }

    private static string PlayerStakes(string danger, string region) => danger switch
    {
        "Faible" => $"Même contenue, cette affaire laissera des traces parmi les habitants de {region}.",
        "Eleve" => $"Si personne n'intervient, les conséquences dépasseront rapidement {region} et menaceront les provinces voisines.",
        _ => $"Les choix des personnages décideront si {region} retrouve un fragile répit ou s'enfonce dans une crise durable."
    };

    private static string FailureConsequence(string threat, string danger, string region)
    {
        var escalation = danger == "Eleve" ? "sans rencontrer de résistance" : "avant que les autorités ne comprennent le danger";
        var consequence = threat switch
        {
            var value when value.StartsWith("Chaos", StringComparison.OrdinalIgnoreCase) => "la corruption gagnera les communautés voisines",
            var value when value.StartsWith("Morts-vivants", StringComparison.OrdinalIgnoreCase) => "les morts se multiplieront le long des routes et dans les cimetières",
            var value when value.StartsWith("Peaux-vertes", StringComparison.OrdinalIgnoreCase) => "les raids ouvriront la voie à une offensive plus vaste",
            var value when value.StartsWith("Skavens", StringComparison.OrdinalIgnoreCase) => "les tunnels et les réserves contaminées étendront la crise sous les cités",
            var value when value.StartsWith("Criminels", StringComparison.OrdinalIgnoreCase) => "le réseau criminel imposera durablement sa loi",
            var value when value.StartsWith("Politique", StringComparison.OrdinalIgnoreCase) => "les factions rivales entraîneront la province dans une lutte ouverte",
            var value when value.StartsWith("Betes sauvages", StringComparison.OrdinalIgnoreCase) => "les routes et les fermes isolées deviendront impraticables",
            var value when value.StartsWith("Secte", StringComparison.OrdinalIgnoreCase) => "la secte recrutera de nouveaux fidèles et préparera un rituel plus ambitieux",
            var value when value.StartsWith("Sorcellerie", StringComparison.OrdinalIgnoreCase) => "les phénomènes magiques contamineront les lieux et ceux qui les habitent",
            _ => "la menace étendra son influence"
        };

        return $"En cas d'échec, {consequence} dans {region}, {escalation}.";
    }

    private static string JoinParagraphs(params string?[] paragraphs) =>
        string.Join(Environment.NewLine + Environment.NewLine,
            paragraphs.Where(paragraph => !string.IsNullOrWhiteSpace(paragraph)).Select(paragraph => paragraph!.Trim()));

    private static string JoinSentences(params string?[] sentences) =>
        string.Join(' ', sentences
            .Where(sentence => !string.IsNullOrWhiteSpace(sentence))
            .Select(sentence => EnsureTerminalPunctuation(sentence!)));

    private static string EnsureTerminalPunctuation(string sentence)
    {
        var value = sentence.Trim();
        return value.EndsWith('.') || value.EndsWith('!') || value.EndsWith('?') ? value : $"{value}.";
    }
}
