namespace Wfrp4.Shared.Models;

public static class TitreGenreHelper
{
    private static readonly Dictionary<string, string> Masculins = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Armurière"] = "Armurier",
        ["Arpenteuse"] = "Arpenteur",
        ["Aventurière"] = "Aventurier",
        ["Bottière"] = "Bottier",
        ["Brigande"] = "Brigand",
        ["Briseuse"] = "Briseur",
        ["Charognarde"] = "Charognard",
        ["Combattante"] = "Combattant",
        ["Criminelle"] = "Criminel",
        ["Écorcheuse"] = "Écorcheur",
        ["Envoûteuse"] = "Envoûteur",
        ["Folle furieuse"] = "Fou furieux",
        ["Forgeronne"] = "Forgeron",
        ["Fossoyeuse"] = "Fossoyeur",
        ["Fracasseuse"] = "Fracasseur",
        ["Joaillière"] = "Joaillier",
        ["Mineuse"] = "Mineur",
        ["Moissonneuse"] = "Moissonneur",
        ["Randonneuse"] = "Randonneur",
        ["Ravageuse"] = "Ravageur",
        ["Recycleuse"] = "Recycleur",
        ["Roturière"] = "Roturier",
        ["Souveraine"] = "Souverain",
        ["Tireuse d'élite"] = "Tireur d'élite",
        ["Traqueuse"] = "Traqueur",
        ["Adjuratrice"] = "Adjurateur",
        ["Arnaqueuse"] = "Arnaqueur",
        ["Baronne"] = "Baron",
        ["Batailleuse"] = "Batailleur",
        ["Bergère"] = "Berger",
        ["Bouchère"] = "Boucher",
        ["Brasseuse"] = "Brasseur",
        ["Bricoleuse"] = "Bricoleur",
        ["Brigadière"] = "Brigadier",
        ["Canaille"] = "Canaille",
        ["Candidate"] = "Candidat",
        ["Cavalière"] = "Cavalier",
        ["Chamane"] = "Chaman",
        ["Championne"] = "Champion",
        ["Chasseuse"] = "Chasseur",
        ["Châtelaine"] = "Châtelain",
        ["Cheffe"] = "Chef",
        ["Cogneuse"] = "Cogneur",
        ["Collectionneuse"] = "Collectionneur",
        ["Connaisseuse"] = "Connaisseur",
        ["Conquérante"] = "Conquérant",
        ["Coordinatrice"] = "Coordinateur",
        ["Coupable"] = "Coupable",
        ["Dame"] = "Seigneur",
        ["Danseuse"] = "Danseur",
        ["Démone"] = "Démon",
        ["Destructrice"] = "Destructeur",
        ["Diablesse"] = "Diable",
        ["Dirigeante"] = "Dirigeant",
        ["Druidesse"] = "Druide",
        ["Égorgeuse"] = "Égorgeur",
        ["Élue"] = "Élu",
        ["Enchanteresse"] = "Enchanteur",
        ["Ennemie"] = "Ennemi",
        ["Ensorceleuse"] = "Ensorceleur",
        ["Exploratrice"] = "Explorateur",
        ["Fanfaronne"] = "Fanfaron",
        ["Faucheuse"] = "Faucheur",
        ["Fleuriste"] = "Fleuriste",
        ["Fouilleuse"] = "Fouilleur",
        ["Fripouille"] = "Fripouille",
        ["Fugitive"] = "Fugitif",
        ["Gardienne"] = "Gardien",
        ["Gloutonne"] = "Glouton",
        ["Guerrière"] = "Guerrier",
        ["Harponneuse"] = "Harponneur",
        ["Hérétique"] = "Hérétique",
        ["Héroïne"] = "Héros",
        ["Immortelle"] = "Immortel",
        ["Impostrice"] = "Imposteur",
        ["Incendiaire"] = "Incendiaire",
        ["Inspectrice"] = "Inspecteur",
        ["Lauréate"] = "Lauréat",
        ["Louve"] = "Loup",
        ["Magnate"] = "Magnat",
        ["Maîtresse"] = "Maître",
        ["Maîtresse brasseuse"] = "Maître brasseur",
        ["Mangeuse"] = "Mangeur",
        ["Maraudeuse"] = "Maraudeur",
        ["Marchande d'armes"] = "Marchand d'armes",
        ["Meneuse"] = "Meneur",
        ["Messagère"] = "Messager",
        ["Meurtrière"] = "Meurtrier",
        ["Misérable"] = "Misérable",
        ["Navigatrice"] = "Navigateur",
        ["Nécromancienne"] = "Nécromancien",
        ["Opportuniste"] = "Opportuniste",
        ["Parfumeuse"] = "Parfumeur",
        ["Parieuse"] = "Parieur",
        ["Partisane"] = "Partisan",
        ["Patronne"] = "Patron",
        ["Pêcheresse"] = "Pêcheur",
        ["Perdante"] = "Perdant",
        ["Pisteuse"] = "Pisteur",
        ["Poursuivante"] = "Poursuivant",
        ["Prédatrice"] = "Prédateur",
        ["Protectrice"] = "Protecteur",
        ["Renverseuse"] = "Renverseur",
        ["Saccageuse"] = "Saccageur",
        ["Séductrice"] = "Séducteur",
        ["Sorcière"] = "Sorcier",
        ["Tortionnaire"] = "Tortionnaire",
        ["Triomphatrice"] = "Triomphateur",
        ["Tueuse"] = "Tueur",
        ["Voleuse"] = "Voleur",
        ["Voyageuse"] = "Voyageur",
    };

    public static string AppliquerGenre(string? libelle, GenrePersonnage genre)
    {
        if (string.IsNullOrWhiteSpace(libelle) || genre == GenrePersonnage.Feminin)
            return libelle ?? string.Empty;

        if (Masculins.TryGetValue(libelle, out var masculin))
            return masculin;

        return MasculiniserRegulier(libelle);
    }

    public static string Assembler(string? baseLibelle, string? qualificatifLibelle, GenrePersonnage genre)
    {
        if (string.IsNullOrWhiteSpace(baseLibelle) || string.IsNullOrWhiteSpace(qualificatifLibelle))
            return string.Empty;

        return $"{AppliquerGenre(baseLibelle, genre)} {AppliquerGenre(qualificatifLibelle, genre)}";
    }

    private static string MasculiniserRegulier(string libelle)
    {
        var mots = libelle.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return string.Join(' ', mots.Select(MasculiniserMot));
    }

    private static string MasculiniserMot(string mot)
    {
        if (mot.StartsWith("d'", StringComparison.OrdinalIgnoreCase)
            || mot.StartsWith("D'", StringComparison.Ordinal)
            || mot is "de" or "De" or "la" or "La" or "le" or "Le" or "du" or "Du" or "des" or "Des" or "à" or "À")
            return mot;

        if (mot.EndsWith("euse", StringComparison.OrdinalIgnoreCase))
            return mot[..^4] + "eur";
        if (mot.EndsWith("trice", StringComparison.OrdinalIgnoreCase))
            return mot[..^5] + "teur";
        if (mot.EndsWith("ière", StringComparison.OrdinalIgnoreCase))
            return mot[..^4] + "ier";
        if (mot.EndsWith("onne", StringComparison.OrdinalIgnoreCase))
            return mot[..^4] + "on";
        if (mot.EndsWith("elle", StringComparison.OrdinalIgnoreCase))
            return mot[..^4] + "el";
        if (mot.EndsWith("ive", StringComparison.OrdinalIgnoreCase))
            return mot[..^3] + "if";
        if (mot.EndsWith("que", StringComparison.OrdinalIgnoreCase))
            return mot;
        if (mot.EndsWith("te", StringComparison.OrdinalIgnoreCase))
            return mot[..^1];
        if (mot.EndsWith("ée", StringComparison.OrdinalIgnoreCase))
            return mot[..^1];
        if (mot.EndsWith("e", StringComparison.OrdinalIgnoreCase) && mot.Length > 3)
            return mot[..^1];

        return mot;
    }
}
