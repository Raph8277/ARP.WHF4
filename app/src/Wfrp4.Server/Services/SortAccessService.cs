using Microsoft.EntityFrameworkCore;
using Wfrp4.Infrastructure.Data;
using Wfrp4.Infrastructure.Entities;

namespace Wfrp4.Server.Services;

public class SortAccessService
{
    private readonly Wfrp4DbContext _db;

    public SortAccessService(Wfrp4DbContext db) => _db = db;

    public static bool EstAccessible(SortReference sort, IEnumerable<string> talentCodes)
    {
        var talents = talentCodes.ToHashSet(StringComparer.OrdinalIgnoreCase);
        return (sort.Categorie == "Mineur" && talents.Contains("MAGIE_MINEUR"))
            || (sort.Categorie == "Arcanique" && talents.Contains("MAGIE_ARCANE"));
    }

    public async Task<bool> EstAccessibleAuPersonnage(int personnageId, SortReference sort)
    {
        var talentCodes = await _db.PersonnageTalents
            .Where(t => t.PersonnageId == personnageId)
            .Select(t => t.Talent.Code)
            .ToListAsync();

        return EstAccessible(sort, talentCodes);
    }
}
