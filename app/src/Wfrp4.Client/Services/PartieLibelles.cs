using MudBlazor;
using Wfrp4.Shared.Models;

namespace Wfrp4.Client.Services;

public static class PartieLibelles
{
    public static string Type(TypePartie type) => type switch
    {
        TypePartie.Campagne => "Campagne",
        _ => "Aventure",
    };

    public static string Statut(StatutPartie statut) => statut switch
    {
        StatutPartie.Preparation => "En préparation",
        StatutPartie.EnCours => "En cours",
        StatutPartie.Terminee => "Terminée",
        _ => statut.ToString(),
    };

    public static Color CouleurStatut(StatutPartie statut) => statut switch
    {
        StatutPartie.EnCours => Color.Success,
        StatutPartie.Terminee => Color.Default,
        _ => Color.Info,
    };
}
