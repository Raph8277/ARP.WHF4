using Wfrp4.Infrastructure.Entities;
using Wfrp4.Shared.DTOs;
using Wfrp4.Shared.Models;

namespace Wfrp4.Server.Services;

public class XPService
{
    // Coûts XP selon le Livre de Base p. 47
    private static readonly (int MaxAvances, int CoutCarac, int CoutCompetence)[] CoutsXP =
    [
        (5, 25, 10),
        (10, 30, 15),
        (15, 40, 20),
        (20, 50, 30),
        (25, 70, 40),
        (30, 90, 60),
        (35, 120, 80),
        (40, 150, 110),
        (45, 190, 140),
        (50, 230, 180),
    ];

    public int CalculerCoutCaracteristique(int avancesActuelles)
    {
        return CoutsXP.First(c => avancesActuelles < c.MaxAvances).CoutCarac;
    }

    public int CalculerCoutCompetence(int avancesActuelles)
    {
        return CoutsXP.First(c => avancesActuelles < c.MaxAvances).CoutCompetence;
    }

    public int CalculerCoutCompetenceTotal(int avances)
    {
        var total = 0;
        for (var i = 0; i < avances; i++)
            total += CalculerCoutCompetence(i);
        return total;
    }

    public int CalculerCoutTalent(int foisActuelles)
    {
        return 100 + (foisActuelles * 100);
    }

    public int CalculerCoutChangementCarriere(
        NiveauCarriere actuel,
        NiveauCarriere cible,
        bool carriereComplete)
    {
        var cout = carriereComplete ? 100 : 200;
        if (actuel.Carriere.ClasseId != cible.Carriere.ClasseId)
            cout += 100;
        return cout;
    }

    public RevenusDto CalculerRevenus(StatutTier tier, int rang)
    {
        return tier switch
        {
            StatutTier.Bronze => new RevenusDto
            {
                Tier = tier,
                Rang = rang,
                Monnaie = "Sous de cuivre",
                Formule = $"2d10 × {rang}",
                RevenuMin = 2 * rang,
                RevenuMax = 20 * rang,
            },
            StatutTier.Argent => new RevenusDto
            {
                Tier = tier,
                Rang = rang,
                Monnaie = "Pistoles d'argent",
                Formule = $"1d10 × {rang}",
                RevenuMin = 1 * rang,
                RevenuMax = 10 * rang,
            },
            StatutTier.Or => new RevenusDto
            {
                Tier = tier,
                Rang = rang,
                Monnaie = "Couronnes d'or",
                Formule = $"1 × {rang}",
                RevenuMin = rang,
                RevenuMax = rang,
            },
            _ => throw new ArgumentOutOfRangeException(nameof(tier)),
        };
    }
}
