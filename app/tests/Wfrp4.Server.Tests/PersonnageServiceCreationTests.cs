using Microsoft.EntityFrameworkCore;
using Wfrp4.Infrastructure.Data;
using Wfrp4.Infrastructure.Entities;
using Wfrp4.Server.Services;
using Wfrp4.Shared.DTOs;
using Wfrp4.Shared.Models;

namespace Wfrp4.Server.Tests;

public class PersonnageServiceCreationTests
{
    [Fact]
    public async Task CreerPersonnage_ne_facture_pas_les_avances_et_talent_gratuits()
    {
        await using var db = CreateDb();
        var niveau = await SeedCareerAsync(db);
        var request = CreateValidRequest(niveau.Id);
        var service = new PersonnageService(db, new XPService());

        var personnage = await service.CreerPersonnage("user-1", request);

        Assert.Equal(50, personnage.XpTotal);
        Assert.Equal(0, personnage.XpDepense);
        Assert.Equal(0, personnage.AvancesCompetenceGratuitesRestantes);
        Assert.Equal(0, personnage.TalentsGratuitsRestants);
        Assert.Equal(40, personnage.Competences.Sum(c => c.Avances));
        Assert.Single(personnage.Talents);
    }

    [Fact]
    public async Task CreerPersonnage_accepte_les_listes_de_carriere_separees_par_des_virgules()
    {
        await using var db = CreateDb();
        var niveau = await SeedCareerAsync(db, ',');
        var request = CreateValidRequest(niveau.Id);
        var service = new PersonnageService(db, new XPService());

        var personnage = await service.CreerPersonnage("user-1", request);

        Assert.Equal(8, personnage.Competences.Count(c => c.Avances > 0));
        Assert.Single(personnage.Talents);
    }

    [Fact]
    public async Task CreerPersonnage_avec_magie_mineure_exige_le_bonus_fm_en_sorts()
    {
        await using var db = CreateDb();
        var niveau = await SeedCareerAsync(db);
        db.Talents.Add(new Talent { Id = 3, Code = "MAGIE_MINEUR", Nom = "Magie Mineure" });
        for (var id = 1; id <= 3; id++)
        {
            db.SortsReference.Add(new SortReference
            {
                Id = id,
                Code = $"MINEUR:{id}",
                Nom = $"Sort {id}",
                Categorie = "Mineur",
            });
        }
        niveau.TalentsRevenu = "MAGIE_MINEUR|TAL1";
        await db.SaveChangesAsync();
        var request = CreateValidRequest(niveau.Id);
        request.TalentsInitiaux = [3];
        request.SortsInitiaux = [1, 2, 3];
        var service = new PersonnageService(db, new XPService());

        var personnage = await service.CreerPersonnage("user-1", request);

        Assert.Equal(3, personnage.Sorts.Count);
    }

    [Fact]
    public async Task CreerPersonnage_avec_magie_mineure_refuse_un_nombre_de_sorts_incorrect()
    {
        await using var db = CreateDb();
        var niveau = await SeedCareerAsync(db);
        db.Talents.Add(new Talent { Id = 3, Code = "MAGIE_MINEUR", Nom = "Magie Mineure" });
        db.SortsReference.Add(new SortReference { Id = 1, Code = "MINEUR:1", Nom = "Sort 1", Categorie = "Mineur" });
        niveau.TalentsRevenu = "MAGIE_MINEUR|TAL1";
        await db.SaveChangesAsync();
        var request = CreateValidRequest(niveau.Id);
        request.TalentsInitiaux = [3];
        request.SortsInitiaux = [1];
        var service = new PersonnageService(db, new XPService());

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreerPersonnage("user-1", request));

        Assert.Contains("exactement 3 sorts mineurs", exception.Message);
    }

    [Fact]
    public async Task CreerPersonnage_refuse_un_total_d_avances_different_de_40()
    {
        await using var db = CreateDb();
        var niveau = await SeedCareerAsync(db);
        var request = CreateValidRequest(niveau.Id);
        request.CompetencesInitiales[1] = 4;
        var service = new PersonnageService(db, new XPService());

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreerPersonnage("user-1", request));
    }

    [Fact]
    public async Task CreerPersonnage_refuse_plusieurs_talents_de_carriere()
    {
        await using var db = CreateDb();
        var niveau = await SeedCareerAsync(db);
        var request = CreateValidRequest(niveau.Id);
        request.TalentsInitiaux = [1, 2];
        var service = new PersonnageService(db, new XPService());

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreerPersonnage("user-1", request));
    }

    [Fact]
    public async Task CreerPersonnage_valorise_les_competences_de_base_absentes_a_5()
    {
        await using var db = CreateDb();
        var niveau = await SeedCareerAsync(db);
        db.Competences.Add(new Competence
        {
            Id = 9,
            Code = "BASE9",
            Nom = "Competence de base hors carriere",
            Caracteristique = "CC",
            EstAvancee = false,
        });
        await db.SaveChangesAsync();
        var request = CreateValidRequest(niveau.Id);
        var service = new PersonnageService(db, new XPService());

        var personnage = await service.CreerPersonnage("user-1", request);

        Assert.Equal(5, personnage.Competences.Single(c => c.CompetenceId == 9).Avances);
        Assert.Equal(0, personnage.XpDepense);
        Assert.Equal(0, personnage.AvancesCompetenceGratuitesRestantes);
    }

    [Fact]
    public async Task AvancerCompetence_de_base_absente_demarre_a_5_sans_cout_de_socle()
    {
        await using var db = CreateDb();
        await SeedCareerAsync(db);
        db.Personnages.Add(new Personnage
        {
            Id = 1,
            KeycloakId = "user-1",
            Nom = "Test",
            EspeceId = 1,
            XpTotal = 100,
            XpDepense = 0,
            AvancesCompetenceGratuitesRestantes = 0,
        });
        await db.SaveChangesAsync();
        var service = new PersonnageService(db, new XPService());

        var cout = await service.AvancerCompetence(1, 1, 1);

        var competence = await db.PersonnageCompetences.SingleAsync(c => c.PersonnageId == 1 && c.CompetenceId == 1);
        Assert.Equal(6, competence.Avances);
        Assert.Equal(15, cout);
        Assert.Equal(15, (await db.Personnages.FindAsync(1))!.XpDepense);
    }

    [Fact]
    public async Task ReduireCompetence_gratuite_ne_rend_pas_d_xp_inexistant()
    {
        await using var db = CreateDb();
        await SeedCareerAsync(db);
        db.Personnages.Add(new Personnage
        {
            Id = 1,
            KeycloakId = "user-1",
            Nom = "Test",
            EspeceId = 1,
            XpTotal = 50,
            XpDepense = 0,
            AvancesCompetenceGratuitesRestantes = 35,
            Competences =
            [
                new PersonnageCompetence
                {
                    CompetenceId = 1,
                    Avances = 7,
                }
            ],
        });
        await db.SaveChangesAsync();
        var service = new PersonnageService(db, new XPService());

        var cout = await service.AvancerCompetence(1, 1, -2);

        var personnage = await db.Personnages.Include(p => p.Competences).SingleAsync(p => p.Id == 1);
        Assert.Equal(0, cout);
        Assert.Equal(0, personnage.XpDepense);
        Assert.Equal(50, personnage.XpTotal - personnage.XpDepense);
        Assert.Equal(5, personnage.Competences.Single(c => c.CompetenceId == 1).Avances);
    }

    [Fact]
    public async Task AvancerCarriere_consomme_100_xp_et_active_le_niveau_suivant()
    {
        await using var db = CreateDb();
        var niveau = await SeedCareerAsync(db);
        var niveauSuivant = new NiveauCarriere
        {
            Id = 2,
            CarriereId = 1,
            Niveau = 2,
            Intitule = "Niveau 2",
            Statut = StatutTier.Argent,
            StatutNumerique = 1,
        };
        db.NiveauCarrieres.Add(niveauSuivant);
        await db.SaveChangesAsync();
        var request = CreateValidRequest(niveau.Id);
        request.XpBonus = 200;
        var service = new PersonnageService(db, new XPService());
        var personnage = await service.CreerPersonnage("user-1", request);

        var cout = await service.AvancerCarriere(personnage.Id, niveauSuivant.Id);

        var actualise = await db.Personnages.Include(p => p.Carrieres).SingleAsync(p => p.Id == personnage.Id);
        Assert.Equal(100, cout);
        Assert.Equal(100, actualise.XpDepense);
        Assert.Equal(niveauSuivant.Id, actualise.CarriereCouranteId);
        Assert.Single(actualise.Carrieres, c => c.EstCourante && c.NiveauCarriereId == niveauSuivant.Id);
        Assert.Single(await db.HistoriqueXPs.Where(h => h.Type == TypeXP.Carriere).ToListAsync());
    }

    [Fact]
    public async Task AvancerCarriere_refuse_si_les_competences_sont_insuffisantes()
    {
        await using var db = CreateDb();
        var niveau = await SeedCareerAsync(db);
        var niveauSuivant = new NiveauCarriere
        {
            Id = 2,
            CarriereId = 1,
            Niveau = 2,
            Intitule = "Niveau 2",
            Statut = StatutTier.Argent,
            StatutNumerique = 1,
        };
        db.NiveauCarrieres.Add(niveauSuivant);
        await db.SaveChangesAsync();
        var request = CreateValidRequest(niveau.Id);
        request.XpBonus = 200;
        var service = new PersonnageService(db, new XPService());
        var personnage = await service.CreerPersonnage("user-1", request);
        personnage.Competences.Single(c => c.CompetenceId == 1).Avances = 4;
        await db.SaveChangesAsync();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.AvancerCarriere(personnage.Id, niveauSuivant.Id));

        Assert.Contains("7/8 compétences", exception.Message);
        Assert.Equal(niveau.Id, personnage.CarriereCouranteId);
    }

    [Fact]
    public async Task AnnulerDernierPassageCarriere_reactive_le_niveau_precedent_et_rembourse_100_xp()
    {
        await using var db = CreateDb();
        var niveau = await SeedCareerAsync(db);
        var niveauSuivant = new NiveauCarriere
        {
            Id = 2, CarriereId = 1, Niveau = 2, Intitule = "Niveau 2",
            Statut = StatutTier.Argent, StatutNumerique = 1,
        };
        db.NiveauCarrieres.Add(niveauSuivant);
        await db.SaveChangesAsync();
        var request = CreateValidRequest(niveau.Id);
        request.XpBonus = 200;
        var service = new PersonnageService(db, new XPService());
        var personnage = await service.CreerPersonnage("user-1", request);
        await service.AvancerCarriere(personnage.Id, niveauSuivant.Id);

        var remboursement = await service.AnnulerDernierPassageCarriere(personnage.Id);

        var actualise = await db.Personnages.Include(p => p.Carrieres).SingleAsync(p => p.Id == personnage.Id);
        Assert.Equal(100, remboursement);
        Assert.Equal(0, actualise.XpDepense);
        Assert.Equal(niveau.Id, actualise.CarriereCouranteId);
        Assert.Single(actualise.Carrieres);
        Assert.True(actualise.Carrieres.Single().EstCourante);
    }

    [Fact]
    public async Task ChangerCarriere_dans_la_meme_classe_coute_100_xp_si_niveau_complete()
    {
        await using var db = CreateDb();
        var niveau = await SeedCareerAsync(db);
        db.Carrieres.Add(new Carriere { Id = 2, Code = "CAR2", Nom = "Carriere 2", ClasseId = 1 });
        var cible = new NiveauCarriere
        {
            Id = 2, CarriereId = 2, Niveau = 1, Intitule = "Nouveau métier",
            Statut = StatutTier.Bronze, StatutNumerique = 2,
        };
        db.NiveauCarrieres.Add(cible);
        await db.SaveChangesAsync();
        var request = CreateValidRequest(niveau.Id);
        request.XpBonus = 200;
        var service = new PersonnageService(db, new XPService());
        var personnage = await service.CreerPersonnage("user-1", request);

        var cout = await service.AvancerCarriere(personnage.Id, cible.Id);

        Assert.Equal(100, cout);
        Assert.Equal(cible.Id, personnage.CarriereCouranteId);
        Assert.Equal(100, personnage.XpDepense);
    }

    private static Wfrp4DbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<Wfrp4DbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new Wfrp4DbContext(options);
    }

    private static async Task<NiveauCarriere> SeedCareerAsync(Wfrp4DbContext db, char separator = '|')
    {
        db.Especes.Add(new Espece
        {
            Id = 1,
            Code = "HUMAIN",
            Nom = "Humain",
            MouvementBase = 4,
        });
        db.Classes.Add(new Classe { Id = 1, Code = "CL", Nom = "Classe" });
        db.Carrieres.Add(new Carriere { Id = 1, Code = "CAR", Nom = "Carriere", ClasseId = 1 });

        for (var i = 1; i <= 8; i++)
        {
            db.Competences.Add(new Competence
            {
                Id = i,
                Code = $"COMP{i}",
                Nom = $"Competence {i}",
                Caracteristique = "CC",
            });
        }

        db.Talents.Add(new Talent { Id = 1, Code = "TAL1", Nom = "Talent 1" });
        db.Talents.Add(new Talent { Id = 2, Code = "TAL2", Nom = "Talent 2" });

        var niveau = new NiveauCarriere
        {
            Id = 1,
            CarriereId = 1,
            Niveau = 1,
            Intitule = "Niveau 1",
            Statut = StatutTier.Bronze,
            StatutNumerique = 1,
            CompetenceRevenu = string.Join(separator, Enumerable.Range(1, 8).Select(i => $"COMP{i}")),
            TalentsRevenu = string.Join(separator, "TAL1", "TAL2"),
        };
        db.NiveauCarrieres.Add(niveau);
        await db.SaveChangesAsync();
        return niveau;
    }

    private static CreatePersonnageRequest CreateValidRequest(int niveauCarriereId) => new()
    {
        Nom = "Test",
        EspeceId = 1,
        NiveauCarriereId = niveauCarriereId,
        XpBonus = 50,
        CaracteristiquesInitiales = new Dictionary<string, int>
        {
            ["CC"] = 30,
            ["CT"] = 30,
            ["F"] = 30,
            ["E"] = 30,
            ["I"] = 30,
            ["Ag"] = 30,
            ["Dex"] = 30,
            ["Int"] = 30,
            ["FM"] = 30,
            ["Soc"] = 30,
        },
        CompetencesInitiales = Enumerable.Range(1, 8).ToDictionary(i => i, _ => 5),
        TalentsInitiaux = [1],
    };
}
