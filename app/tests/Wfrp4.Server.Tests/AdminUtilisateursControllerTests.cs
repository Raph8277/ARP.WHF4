using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Wfrp4.Server.Controllers;
using Wfrp4.Server.Services;
using Wfrp4.Shared.DTOs;

namespace Wfrp4.Server.Tests;

/// <summary>Contrôleur appelé directement avec un service Keycloak simulé : prouve les règles, pas le pipeline HTTP.</summary>
public class AdminUtilisateursControllerTests
{
    private const string AdminId = "33333333-3333-3333-3333-333333333333";
    private const string AutreAdminId = "99999999-9999-9999-9999-999999999999";
    private const string JoueurId = "11111111-1111-1111-1111-111111111111";

    private sealed class FauxKeycloak : IKeycloakUtilisateursAdmin
    {
        public Dictionary<string, UtilisateurAdminDto> Comptes { get; } = new();
        public List<string> Appels { get; } = new();
        public bool Panne { get; set; }

        public Task<PageUtilisateursAdminDto> ListerAsync(string? recherche, int page, int taille, CancellationToken ct) =>
            Task.FromResult(new PageUtilisateursAdminDto { Utilisateurs = Comptes.Values.ToList(), Page = page, Taille = taille });

        public Task<UtilisateurAdminDto?> ObtenirAsync(string id, CancellationToken ct)
        {
            if (Panne) throw new KeycloakIndisponibleException("indisponible");
            return Task.FromResult(Comptes.GetValueOrDefault(id));
        }

        public Task DefinirRolesAsync(string id, IReadOnlyCollection<string> roles, CancellationToken ct)
        {
            Appels.Add($"roles:{id}:{string.Join(",", roles.OrderBy(r => r))}");
            Comptes[id].Roles = roles.ToList();
            return Task.CompletedTask;
        }

        public Task DefinirActivationAsync(string id, bool actif, CancellationToken ct)
        {
            Appels.Add($"actif:{id}:{actif}");
            Comptes[id].Actif = actif;
            return Task.CompletedTask;
        }

        public Task EnvoyerReinitialisationMotDePasseAsync(string id, CancellationToken ct)
        {
            Appels.Add($"mdp:{id}");
            return Task.CompletedTask;
        }

        public Task<int> CompterAdminsActifsAsync(CancellationToken ct) =>
            Task.FromResult(Comptes.Values.Count(c => c.Actif && c.Roles.Contains(RolesApplicatifs.Admin)));
    }

    private static (AdminUtilisateursController Controller, FauxKeycloak Keycloak) Creer(int nombreAdmins = 2)
    {
        var kc = new FauxKeycloak();
        kc.Comptes[AdminId] = Compte(AdminId, "admin", RolesApplicatifs.Joueur, RolesApplicatifs.MaitreJeu, RolesApplicatifs.Admin);
        if (nombreAdmins > 1)
            kc.Comptes[AutreAdminId] = Compte(AutreAdminId, "admin2", RolesApplicatifs.Joueur, RolesApplicatifs.Admin);
        kc.Comptes[JoueurId] = Compte(JoueurId, "joueur1", RolesApplicatifs.Joueur);

        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, AdminId),
            new Claim(ClaimTypes.Role, RolesApplicatifs.Admin),
        }, "test");
        var controller = new AdminUtilisateursController(kc, NullLogger<AdminUtilisateursController>.Instance)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) },
            },
        };
        return (controller, kc);
    }

    private static UtilisateurAdminDto Compte(string id, string nom, params string[] roles) => new()
    {
        Id = id, NomUtilisateur = nom, Email = nom + "@wfrp4.local", Actif = true, Roles = roles.ToList(),
    };

    [Fact]
    public void Le_controleur_est_reserve_a_la_politique_Admin()
    {
        var attributs = typeof(AdminUtilisateursController).GetCustomAttributes<AuthorizeAttribute>().ToList();
        Assert.Contains(attributs, a => a.Policy == "Admin");

        // Aucune action ne rouvre l'accès.
        foreach (var methode in typeof(AdminUtilisateursController).GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            Assert.Empty(methode.GetCustomAttributes<AllowAnonymousAttribute>());
    }

    [Fact]
    public async Task Promouvoir_MJ_ajoute_le_role_et_conserve_joueur()
    {
        var (controller, kc) = Creer();

        var result = await controller.ModifierRoles(JoueurId, new ModifierRolesRequest { Roles = { RolesApplicatifs.MaitreJeu } });

        Assert.IsType<NoContentResult>(result);
        Assert.Equal(new[] { RolesApplicatifs.Joueur, RolesApplicatifs.MaitreJeu }, kc.Comptes[JoueurId].Roles.OrderBy(r => r));
    }

    [Theory]
    [InlineData("realm-management")]
    [InlineData("offline_access")]
    [InlineData("wfrp4-superadmin")]
    public async Task Role_hors_liste_blanche_est_refuse_sans_modification(string role)
    {
        var (controller, kc) = Creer();

        var result = await controller.ModifierRoles(JoueurId, new ModifierRolesRequest { Roles = { RolesApplicatifs.Joueur, role } });

        Assert.IsType<BadRequestObjectResult>(result);
        Assert.Empty(kc.Appels);
    }

    [Fact]
    public async Task Liste_de_roles_vide_est_refusee()
    {
        var (controller, kc) = Creer();

        var result = await controller.ModifierRoles(JoueurId, new ModifierRolesRequest());

        Assert.IsType<BadRequestObjectResult>(result);
        Assert.Empty(kc.Appels);
    }

    [Fact]
    public async Task Admin_ne_peut_pas_se_retrograder_lui_meme()
    {
        var (controller, kc) = Creer();

        var result = await controller.ModifierRoles(AdminId, new ModifierRolesRequest { Roles = { RolesApplicatifs.Joueur } });

        Assert.IsType<ConflictObjectResult>(result);
        Assert.Empty(kc.Appels);
    }

    [Fact]
    public async Task Le_dernier_admin_actif_ne_peut_pas_etre_retrograde()
    {
        var (controller, kc) = Creer(nombreAdmins: 1);
        // Un seul admin actif : l'appelant (un admin déjà déconnecté du compte cible) vise ce compte.
        controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "autre-appelant"),
            new Claim(ClaimTypes.Role, RolesApplicatifs.Admin),
        }, "test"));

        var result = await controller.ModifierRoles(AdminId, new ModifierRolesRequest { Roles = { RolesApplicatifs.Joueur } });

        Assert.IsType<ConflictObjectResult>(result);
        Assert.Empty(kc.Appels);
    }

    [Fact]
    public async Task Retrograder_un_autre_admin_reste_possible_s_il_en_reste_un()
    {
        var (controller, kc) = Creer();

        var result = await controller.ModifierRoles(AutreAdminId, new ModifierRolesRequest { Roles = { RolesApplicatifs.Joueur } });

        Assert.IsType<NoContentResult>(result);
        Assert.DoesNotContain(RolesApplicatifs.Admin, kc.Comptes[AutreAdminId].Roles);
    }

    [Fact]
    public async Task Admin_ne_peut_pas_desactiver_son_propre_compte()
    {
        var (controller, kc) = Creer();

        var result = await controller.ModifierActivation(AdminId, new ModifierActivationRequest { Actif = false });

        Assert.IsType<ConflictObjectResult>(result);
        Assert.Empty(kc.Appels);
    }

    [Fact]
    public async Task Desactiver_un_joueur_est_possible()
    {
        var (controller, kc) = Creer();

        var result = await controller.ModifierActivation(JoueurId, new ModifierActivationRequest { Actif = false });

        Assert.IsType<NoContentResult>(result);
        Assert.False(kc.Comptes[JoueurId].Actif);
    }

    [Fact]
    public async Task Compte_inconnu_ou_compte_de_service_renvoie_404()
    {
        var (controller, kc) = Creer();

        Assert.IsType<NotFoundResult>(await controller.Obtenir("inconnu"));
        Assert.IsType<NotFoundResult>(await controller.ModifierRoles("inconnu", new ModifierRolesRequest { Roles = { RolesApplicatifs.Joueur } }));
        Assert.IsType<NotFoundResult>(await controller.ReinitialiserMotDePasse("inconnu"));
        Assert.Empty(kc.Appels);
    }

    [Fact]
    public async Task Reinitialisation_declenche_l_envoi_sans_manipuler_de_mot_de_passe()
    {
        var (controller, kc) = Creer();

        var result = await controller.ReinitialiserMotDePasse(JoueurId);

        Assert.IsType<NoContentResult>(result);
        Assert.Equal(new[] { $"mdp:{JoueurId}" }, kc.Appels);
    }

    [Fact]
    public async Task Reinitialisation_refusee_sans_adresse_email()
    {
        var (controller, kc) = Creer();
        kc.Comptes[JoueurId].Email = null;

        var result = await controller.ReinitialiserMotDePasse(JoueurId);

        Assert.IsType<BadRequestObjectResult>(result);
        Assert.Empty(kc.Appels);
    }

    [Fact]
    public async Task Panne_Keycloak_donne_503_sans_fuite_de_detail()
    {
        var (controller, kc) = Creer();
        kc.Panne = true;

        var result = await controller.Obtenir(JoueurId);

        var objet = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status503ServiceUnavailable, objet.StatusCode);
    }
}
