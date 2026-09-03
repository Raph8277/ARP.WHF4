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

    private static Wfrp4DbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<Wfrp4DbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new Wfrp4DbContext(options);
    }

    private static async Task<NiveauCarriere> SeedCareerAsync(Wfrp4DbContext db)
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
            CompetenceRevenu = string.Join('|', Enumerable.Range(1, 8).Select(i => $"COMP{i}")),
            TalentsRevenu = "TAL1|TAL2",
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
