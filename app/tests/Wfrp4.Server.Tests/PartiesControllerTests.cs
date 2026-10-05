using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Wfrp4.Infrastructure.Data;
using Wfrp4.Infrastructure.Entities;
using Wfrp4.Server.Controllers;
using Wfrp4.Server.Filters;
using Wfrp4.Server.Services;
using Wfrp4.Shared.DTOs;
using Wfrp4.Shared.Models;

namespace Wfrp4.Server.Tests;

/// <summary>
/// Contrôleurs appelés directement (EF InMemory, Keycloak simulé) : prouvent les règles de propriété,
/// de validation et le cycle de vie du partage, pas les politiques HTTP ni les contraintes PostgreSQL.
/// </summary>
public class PartiesControllerTests
{
    private const string Mj = "mj-1";
    private const string AutreMj = "mj-2";
    private const string Admin = "admin-1";
    private const string Joueur1 = "joueur-1";
    private const string Joueur2 = "joueur-2";
    private const string SansRole = "sans-role";
    private const string Inactif = "inactif";

    private const int PersoJoueur1 = 1;
    private const int PersoJoueur2 = 2;
    private const int PersoInactifJoueur1 = 3;
    private const int AutrePersoJoueur1 = 4;

    private sealed record Contexte(Wfrp4DbContext Db, FauxKeycloak Keycloak);

    private static Contexte Creer()
    {
        var db = TestDb.Nouvelle();
        db.Especes.Add(new Espece { Id = 1, Code = "HUM", Nom = "Humain" });
        db.Personnages.AddRange(
            new Personnage { Id = PersoJoueur1, Nom = "Aldric", KeycloakId = Joueur1, EspeceId = 1 },
            new Personnage { Id = PersoJoueur2, Nom = "Brunhilde", KeycloakId = Joueur2, EspeceId = 1 },
            new Personnage { Id = PersoInactifJoueur1, Nom = "Retraité", KeycloakId = Joueur1, EspeceId = 1, EstActif = false },
            new Personnage { Id = AutrePersoJoueur1, Nom = "Corwin", KeycloakId = Joueur1, EspeceId = 1 });
        db.SaveChanges();

        var kc = new FauxKeycloak();
        kc.Comptes[Mj] = FauxKeycloak.Compte(Mj, "master", RolesApplicatifs.Joueur, RolesApplicatifs.MaitreJeu);
        kc.Comptes[Joueur1] = FauxKeycloak.Compte(Joueur1, "user", RolesApplicatifs.Joueur);
        kc.Comptes[Joueur2] = FauxKeycloak.Compte(Joueur2, "oscar", RolesApplicatifs.Joueur);
        kc.Comptes[SansRole] = FauxKeycloak.Compte(SansRole, "fantome");
        kc.Comptes[Inactif] = FauxKeycloak.Compte(Inactif, "parti", RolesApplicatifs.Joueur);
        kc.Comptes[Inactif].Actif = false;
        return new Contexte(db, kc);
    }

    private static ClaimsPrincipal Utilisateur(string id, params string[] roles) =>
        new(new ClaimsIdentity(
            roles.Select(r => new Claim(ClaimTypes.Role, r)).Append(new Claim(ClaimTypes.NameIdentifier, id)),
            "test"));

    private static PartiesController Parties(Contexte c, string id, params string[] roles) =>
        new(c.Db, new PartieService(c.Db), c.Keycloak, NullLogger<PartiesController>.Instance)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = Utilisateur(id, roles) } },
        };

    private static PartiesController ParMj(Contexte c) => Parties(c, Mj, RolesApplicatifs.Joueur, RolesApplicatifs.MaitreJeu);

    private static ParticipationsController Participations(Contexte c, string id) =>
        new(c.Db, new PartieService(c.Db), NullLogger<ParticipationsController>.Instance)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = Utilisateur(id, RolesApplicatifs.Joueur) } },
        };

    private static async Task<int> CreerPartie(PartiesController controller, string nom = "Les Ennemis Intérieurs")
    {
        var result = await controller.Creer(new EnregistrerPartieRequest { Nom = nom, Type = TypePartie.Campagne });
        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        return Assert.IsType<PartieDetailDto>(created.Value).Id;
    }

    private static async Task<int> AjouterMembre(PartiesController controller, int partieId, string joueur)
    {
        var result = await controller.AjouterMembre(partieId, new AjouterMembreRequest { JoueurKeycloakId = joueur }, default);
        return Assert.IsType<PartieDetailDto>(result.Value).Membres.Single(m => m.JoueurKeycloakId == joueur).Id;
    }

    [Fact]
    public void Politiques_MJ_pour_les_parties_et_Joueur_pour_les_participations()
    {
        Assert.Contains(typeof(PartiesController).GetCustomAttributes<AuthorizeAttribute>(), a => a.Policy == "MaitreJeu");
        Assert.Contains(typeof(ParticipationsController).GetCustomAttributes<AuthorizeAttribute>(), a => a.Policy == "Joueur");

        foreach (var type in new[] { typeof(PartiesController), typeof(ParticipationsController) })
        foreach (var methode in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            Assert.Empty(methode.GetCustomAttributes<AllowAnonymousAttribute>());
    }

    [Fact]
    public async Task Un_autre_MJ_ne_gere_pas_la_partie_mais_l_admin_si()
    {
        var c = Creer();
        var partieId = await CreerPartie(ParMj(c));
        var membreId = await AjouterMembre(ParMj(c), partieId, Joueur1);
        var autre = Parties(c, AutreMj, RolesApplicatifs.Joueur, RolesApplicatifs.MaitreJeu);

        Assert.IsType<ForbidResult>((await autre.Obtenir(partieId)).Result);
        Assert.IsType<ForbidResult>((await autre.Modifier(partieId, new EnregistrerPartieRequest { Nom = "Volée" })).Result);
        Assert.IsType<ForbidResult>((await autre.AjouterMembre(partieId, new AjouterMembreRequest { JoueurKeycloakId = Joueur2 }, default)).Result);
        Assert.IsType<ForbidResult>((await autre.ChoisirPersonnage(partieId, membreId, new ChoisirPersonnageRequest { PersonnageId = PersoJoueur1 })).Result);
        Assert.IsType<ForbidResult>((await autre.PersonnagesDuMembre(partieId, membreId)).Result);
        Assert.IsType<ForbidResult>(await autre.Supprimer(partieId));
        Assert.Empty((await autre.Lister()).Value!);

        var admin = Parties(c, Admin, RolesApplicatifs.Joueur, RolesApplicatifs.MaitreJeu, RolesApplicatifs.Admin);
        Assert.IsType<PartieDetailDto>((await admin.Obtenir(partieId)).Value);
        Assert.Single((await admin.Lister()).Value!);
    }

    [Theory]
    [InlineData("inconnu")]
    [InlineData(SansRole)]
    [InlineData(Inactif)]
    [InlineData(Mj)]
    public async Task Ajouter_un_compte_non_joueur_actif_est_refuse(string joueur)
    {
        var c = Creer();
        var mj = ParMj(c);
        var partieId = await CreerPartie(mj);

        var result = await mj.AjouterMembre(partieId, new AjouterMembreRequest { JoueurKeycloakId = joueur }, default);

        Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Empty(c.Db.PartieMembres);
    }

    [Fact]
    public async Task Ajouter_deux_fois_le_meme_joueur_donne_409()
    {
        var c = Creer();
        var mj = ParMj(c);
        var partieId = await CreerPartie(mj);
        await AjouterMembre(mj, partieId, Joueur1);

        var result = await mj.AjouterMembre(partieId, new AjouterMembreRequest { JoueurKeycloakId = Joueur1 }, default);

        Assert.IsType<ConflictObjectResult>(result.Result);
        Assert.Single(c.Db.PartieMembres);
    }

    [Fact]
    public async Task Le_MJ_ne_voit_que_les_personnages_actifs_du_membre()
    {
        var c = Creer();
        var mj = ParMj(c);
        var partieId = await CreerPartie(mj);
        var membreId = await AjouterMembre(mj, partieId, Joueur1);

        var personnages = (await mj.PersonnagesDuMembre(partieId, membreId)).Value!;

        Assert.Equal(new[] { "Aldric", "Corwin" }, personnages.Select(p => p.Nom));
    }

    [Theory]
    [InlineData(PersoJoueur2)]
    [InlineData(PersoInactifJoueur1)]
    [InlineData(999)]
    public async Task Choisir_un_personnage_qui_n_est_pas_au_membre_est_refuse(int personnageId)
    {
        var c = Creer();
        var mj = ParMj(c);
        var partieId = await CreerPartie(mj);
        var membreId = await AjouterMembre(mj, partieId, Joueur1);

        var result = await mj.ChoisirPersonnage(partieId, membreId, new ChoisirPersonnageRequest { PersonnageId = personnageId });

        Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Empty(c.Db.PersonnagePartages);
        Assert.Null(c.Db.PartieMembres.Single().PersonnageId);
    }

    [Fact]
    public async Task Choisir_un_personnage_cree_un_partage_XP_sans_droit_de_modification()
    {
        var c = Creer();
        var mj = ParMj(c);
        var partieId = await CreerPartie(mj);
        var membreId = await AjouterMembre(mj, partieId, Joueur1);

        var result = await mj.ChoisirPersonnage(partieId, membreId, new ChoisirPersonnageRequest { PersonnageId = PersoJoueur1 });

        Assert.Equal("Aldric", Assert.IsType<PartieDetailDto>(result.Value).Membres.Single().PersonnageNom);
        var partage = c.Db.PersonnagePartages.Single();
        Assert.Equal((PersoJoueur1, Mj, PermissionPartage.XP, (int?)partieId), (partage.PersonnageId, partage.MjKeycloakId, partage.Permission, partage.PartieId));

        // Invariant AGENTS.md : un partage XP ne donne pas le droit de modifier l'équipement.
        c.Db.PersonnagePossessions.Add(new PersonnagePossession { Id = 10, PersonnageId = PersoJoueur1, Nom = "Épée" });
        await c.Db.SaveChangesAsync();
        Assert.IsType<ForbidResult>(await SupprimerPossessionCommeMj(c.Db, PersoJoueur1, 10));
        Assert.True(await c.Db.PersonnagePossessions.AnyAsync(p => p.Id == 10));
    }

    [Fact]
    public async Task Changer_de_personnage_deplace_le_partage()
    {
        var c = Creer();
        var mj = ParMj(c);
        var partieId = await CreerPartie(mj);
        var membreId = await AjouterMembre(mj, partieId, Joueur1);
        await mj.ChoisirPersonnage(partieId, membreId, new ChoisirPersonnageRequest { PersonnageId = PersoJoueur1 });

        await mj.ChoisirPersonnage(partieId, membreId, new ChoisirPersonnageRequest { PersonnageId = AutrePersoJoueur1 });

        Assert.Equal(AutrePersoJoueur1, c.Db.PersonnagePartages.Single().PersonnageId);
    }

    [Fact]
    public async Task Retirer_le_membre_supprime_le_partage_de_la_partie_mais_garde_un_partage_manuel()
    {
        var c = Creer();
        var mj = ParMj(c);
        var partieId = await CreerPartie(mj);
        var membre1 = await AjouterMembre(mj, partieId, Joueur1);
        var membre2 = await AjouterMembre(mj, partieId, Joueur2);
        // Joueur 2 avait déjà partagé son personnage en lecture avec ce MJ.
        c.Db.PersonnagePartages.Add(new PersonnagePartage { PersonnageId = PersoJoueur2, MjKeycloakId = Mj, Permission = PermissionPartage.Lecture });
        await c.Db.SaveChangesAsync();
        await mj.ChoisirPersonnage(partieId, membre1, new ChoisirPersonnageRequest { PersonnageId = PersoJoueur1 });
        await mj.ChoisirPersonnage(partieId, membre2, new ChoisirPersonnageRequest { PersonnageId = PersoJoueur2 });

        Assert.IsType<NoContentResult>(await mj.RetirerMembre(partieId, membre1));
        Assert.IsType<NoContentResult>(await mj.RetirerMembre(partieId, membre2));

        var restant = Assert.Single(c.Db.PersonnagePartages);
        Assert.Equal((PersoJoueur2, PermissionPartage.Lecture, (int?)null), (restant.PersonnageId, restant.Permission, restant.PartieId));
        Assert.Empty(c.Db.PartieMembres);
    }

    [Fact]
    public async Task Un_partage_commun_a_deux_parties_du_meme_MJ_est_rattache_a_la_partie_restante()
    {
        var c = Creer();
        var mj = ParMj(c);
        var partie1 = await CreerPartie(mj, "Aventure 1");
        var partie2 = await CreerPartie(mj, "Aventure 2");
        var m1 = await AjouterMembre(mj, partie1, Joueur1);
        var m2 = await AjouterMembre(mj, partie2, Joueur1);
        await mj.ChoisirPersonnage(partie1, m1, new ChoisirPersonnageRequest { PersonnageId = PersoJoueur1 });
        await mj.ChoisirPersonnage(partie2, m2, new ChoisirPersonnageRequest { PersonnageId = PersoJoueur1 });

        Assert.IsType<NoContentResult>(await mj.Supprimer(partie1));

        Assert.Equal(partie2, Assert.Single(c.Db.PersonnagePartages).PartieId);

        await mj.ChoisirPersonnage(partie2, m2, new ChoisirPersonnageRequest { PersonnageId = null });
        Assert.Empty(c.Db.PersonnagePartages);
    }

    [Fact]
    public async Task Le_joueur_voit_ses_participations_et_peut_quitter()
    {
        var c = Creer();
        var mj = ParMj(c);
        var partieId = await CreerPartie(mj);
        var membreId = await AjouterMembre(mj, partieId, Joueur1);
        await mj.ChoisirPersonnage(partieId, membreId, new ChoisirPersonnageRequest { PersonnageId = PersoJoueur1 });

        var participation = Assert.Single((await Participations(c, Joueur1).Lister()).Value!);
        Assert.Equal(("Les Ennemis Intérieurs", "Aldric"), (participation.PartieNom, participation.PersonnageNom));
        Assert.Empty((await Participations(c, Joueur2).Lister()).Value!);

        Assert.IsType<NotFoundResult>(await Participations(c, Joueur2).Quitter(partieId));
        Assert.IsType<NoContentResult>(await Participations(c, Joueur1).Quitter(partieId));
        Assert.Empty(c.Db.PartieMembres);
        Assert.Empty(c.Db.PersonnagePartages);
    }

    [Fact]
    public async Task Le_joueur_propose_son_personnage_et_le_MJ_peut_le_changer()
    {
        var c = Creer();
        var mj = ParMj(c);
        var partieId = await CreerPartie(mj);
        var membreId = await AjouterMembre(mj, partieId, Joueur1);

        var result = await Participations(c, Joueur1).ProposerPersonnage(partieId, new ChoisirPersonnageRequest { PersonnageId = PersoJoueur1 });

        Assert.Equal("Aldric", result.Value!.PersonnageNom);
        var partage = Assert.Single(c.Db.PersonnagePartages);
        Assert.Equal((PersoJoueur1, PermissionPartage.XP, (int?)partieId), (partage.PersonnageId, partage.Permission, partage.PartieId));

        await mj.ChoisirPersonnage(partieId, membreId, new ChoisirPersonnageRequest { PersonnageId = AutrePersoJoueur1 });
        Assert.Equal(AutrePersoJoueur1, Assert.Single(c.Db.PersonnagePartages).PersonnageId);

        await Participations(c, Joueur1).ProposerPersonnage(partieId, new ChoisirPersonnageRequest { PersonnageId = null });
        Assert.Empty(c.Db.PersonnagePartages);
        Assert.Null(c.Db.PartieMembres.Single().PersonnageId);
    }

    [Theory]
    [InlineData(PersoJoueur2)]
    [InlineData(PersoInactifJoueur1)]
    public async Task Le_joueur_ne_propose_que_ses_personnages_actifs(int personnageId)
    {
        var c = Creer();
        var mj = ParMj(c);
        var partieId = await CreerPartie(mj);
        await AjouterMembre(mj, partieId, Joueur1);

        var result = await Participations(c, Joueur1).ProposerPersonnage(partieId, new ChoisirPersonnageRequest { PersonnageId = personnageId });

        Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Empty(c.Db.PersonnagePartages);
    }

    [Fact]
    public async Task Un_non_membre_ne_propose_rien()
    {
        var c = Creer();
        var partieId = await CreerPartie(ParMj(c));

        var result = await Participations(c, Joueur2).ProposerPersonnage(partieId, new ChoisirPersonnageRequest { PersonnageId = PersoJoueur2 });

        Assert.IsType<NotFoundResult>(result.Result);
        Assert.Empty(c.Db.PersonnagePartages);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" a ")]
    public async Task Nom_de_partie_invalide_est_refuse(string nom)
    {
        var c = Creer();

        var result = await ParMj(c).Creer(new EnregistrerPartieRequest { Nom = nom });

        Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Empty(c.Db.Parties);
    }

    [Fact]
    public async Task Recherche_vide_liste_tous_les_joueurs_actifs_sauf_le_MJ()
    {
        var c = Creer();
        var mj = ParMj(c);

        // Le MJ porte aussi le rôle joueur mais ne s'ajoute pas à sa propre partie ; inactif et sans rôle exclus.
        foreach (var vide in new string?[] { null, "", "  " })
            Assert.Equal(new[] { "oscar", "user" }, (await mj.RechercherJoueurs(vide, default)).Value!.Select(j => j.NomUtilisateur));

        var filtres = (await mj.RechercherJoueurs("er", default)).Value!;
        Assert.Equal(new[] { Joueur1 }, filtres.Select(j => j.Id));
        Assert.Equal("user@wfrp4.local", filtres.Single().Email);

        Assert.IsType<BadRequestObjectResult>((await mj.RechercherJoueurs(new string('x', 101), default)).Result);
    }

    [Fact]
    public async Task L_activite_admin_resume_personnages_et_parties()
    {
        var c = Creer();
        var mj = ParMj(c);
        var partieId = await CreerPartie(mj);
        await AjouterMembre(mj, partieId, Joueur1);
        var admin = new AdminUtilisateursController(c.Keycloak, c.Db, NullLogger<AdminUtilisateursController>.Instance);

        var joueur = (await admin.Activite(Joueur1)).Value!;
        var meneur = (await admin.Activite(Mj)).Value!;

        Assert.Equal((3, 0, 1), (joueur.NombrePersonnages, joueur.PartiesMenees.Count, joueur.Participations.Count));
        Assert.Equal((0, 1, 0), (meneur.NombrePersonnages, meneur.PartiesMenees.Count, meneur.Participations.Count));
    }

    /// <summary>Passe par PersonnageOwnerFilter puis l'action, comme PersonnageAuthorizationTests.</summary>
    private static async Task<IActionResult?> SupprimerPossessionCommeMj(Wfrp4DbContext db, int personnageId, int possessionId)
    {
        var http = new DefaultHttpContext { User = Utilisateur(Mj, RolesApplicatifs.Joueur, RolesApplicatifs.MaitreJeu) };
        var controller = new PersonnagesController(db, new PersonnageService(db, new XPService()), new SortAccessService(db))
        {
            ControllerContext = new ControllerContext { HttpContext = http },
        };
        var action = new ActionContext(http, new RouteData(), new ActionDescriptor());
        var context = new ActionExecutingContext(action, new List<IFilterMetadata>(),
            new Dictionary<string, object?> { ["id"] = personnageId }, controller);
        IActionResult? result = null;
        await new PersonnageOwnerFilter(db).OnActionExecutionAsync(context, async () =>
        {
            result = await controller.SupprimerPossession(personnageId, possessionId);
            return new ActionExecutedContext(action, new List<IFilterMetadata>(), controller) { Result = result };
        });
        return result ?? context.Result;
    }
}
