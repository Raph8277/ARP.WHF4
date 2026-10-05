using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wfrp4.Infrastructure.Data;
using Wfrp4.Server.Services;
using Wfrp4.Shared.DTOs;

namespace Wfrp4.Server.Controllers;

/// <summary>
/// Administration des comptes Keycloak. Réservée à l'admin : l'autorisation vient du jeton,
/// jamais des DTO envoyés par le client.
/// </summary>
[ApiController]
[Route("api/admin/utilisateurs")]
[Authorize(Policy = "Admin")]
public class AdminUtilisateursController : ControllerBase
{
    private const int TailleMaxPage = 50;

    private const string MessageSuperAdmin =
        "Ce compte est administrateur par configuration (Administration:SuperAdmins) : son profil et son activation ne se modifient pas ici.";

    private readonly IKeycloakUtilisateursAdmin _keycloak;
    private readonly Wfrp4DbContext _db;
    private readonly ILogger<AdminUtilisateursController> _logger;

    public AdminUtilisateursController(IKeycloakUtilisateursAdmin keycloak, Wfrp4DbContext db, ILogger<AdminUtilisateursController> logger)
    {
        _keycloak = keycloak;
        _db = db;
        _logger = logger;
    }

    private string? CallerId => User.FindFirstValue(ClaimTypes.NameIdentifier);

    [HttpGet]
    public Task<IActionResult> Lister([FromQuery] string? recherche, [FromQuery] string? profil, [FromQuery] int page = 0, [FromQuery] int taille = 20, CancellationToken ct = default) =>
        Executer(async () =>
        {
            ProfilUtilisateur? filtre = profil?.Trim().ToLowerInvariant() switch
            {
                null or "" or "tous" => null,
                "joueur" => ProfilUtilisateur.Joueur,
                "mj" => ProfilUtilisateur.MaitreJeu,
                "admin" => ProfilUtilisateur.Admin,
                _ => ProfilUtilisateur.Aucun,
            };
            if (filtre == ProfilUtilisateur.Aucun)
                return BadRequest(new { Error = "Profil inconnu (joueur, mj ou admin)." });

            var resultat = await _keycloak.ListerAsync(recherche, filtre, Math.Max(page, 0), Math.Clamp(taille, 1, TailleMaxPage), ct);
            return Ok(resultat);
        });

    /// <summary>Personnages et parties d'un compte, lus dans la base applicative.</summary>
    [HttpGet("{id}/activite")]
    public async Task<ActionResult<ActiviteUtilisateurDto>> Activite(string id, CancellationToken ct = default)
    {
        return new ActiviteUtilisateurDto
        {
            NombrePersonnages = await _db.Personnages.CountAsync(p => p.KeycloakId == id, ct),
            PartiesMenees = await _db.Parties.AsNoTracking()
                .Where(p => p.MjKeycloakId == id)
                .OrderByDescending(p => p.UpdatedAt)
                .Select(PartieService.VersResume)
                .ToListAsync(ct),
            Participations = await _db.PartieMembres.AsNoTracking()
                .Where(m => m.JoueurKeycloakId == id)
                .OrderByDescending(m => m.Partie.UpdatedAt)
                .Select(PartieService.VersParticipation)
                .ToListAsync(ct),
        };
    }

    [HttpGet("{id}")]
    public Task<IActionResult> Obtenir(string id, CancellationToken ct = default) =>
        Executer(async () =>
        {
            var utilisateur = await _keycloak.ObtenirAsync(id, ct);
            return utilisateur is null ? NotFound() : Ok(utilisateur);
        });

    [HttpPut("{id}/roles")]
    public Task<IActionResult> ModifierRoles(string id, ModifierRolesRequest request, CancellationToken ct = default) =>
        Executer(async () =>
        {
            var demandes = (request?.Roles ?? new()).Distinct().ToList();
            var inconnus = demandes.Where(r => !RolesApplicatifs.Tous.Contains(r)).ToList();
            if (inconnus.Count > 0)
                return BadRequest(new { Error = $"Rôle non modifiable : {string.Join(", ", inconnus)}." });

            // Un MJ ou un admin reste joueur : le serveur impose la règle, pas le client.
            if (demandes.Contains(RolesApplicatifs.MaitreJeu) || demandes.Contains(RolesApplicatifs.Admin))
                demandes.Add(RolesApplicatifs.Joueur);
            demandes = demandes.Distinct().ToList();

            if (demandes.Count == 0)
                return BadRequest(new { Error = "Au moins un rôle est requis." });

            var cible = await _keycloak.ObtenirAsync(id, ct);
            if (cible is null)
                return NotFound();
            if (cible.SuperAdmin)
                return Conflict(new { Error = MessageSuperAdmin });

            var perdAdmin = cible.Roles.Contains(RolesApplicatifs.Admin) && !demandes.Contains(RolesApplicatifs.Admin);
            if (perdAdmin)
            {
                if (EstMoi(id))
                    return Conflict(new { Error = "Vous ne pouvez pas retirer votre propre rôle administrateur." });
                if (cible.Actif && await _keycloak.CompterAdminsActifsAsync(ct) <= 1)
                    return Conflict(new { Error = "Le dernier administrateur actif ne peut pas être rétrogradé." });
            }

            await _keycloak.DefinirRolesAsync(id, demandes, ct);
            _logger.LogInformation(
                "Admin {Admin} a modifié les rôles de {Cible} ({Utilisateur}) : [{Avant}] -> [{Apres}].",
                CallerId, id, cible.NomUtilisateur,
                string.Join(",", cible.Roles), string.Join(",", demandes));
            return NoContent();
        });

    [HttpPut("{id}/activation")]
    public Task<IActionResult> ModifierActivation(string id, ModifierActivationRequest request, CancellationToken ct = default) =>
        Executer(async () =>
        {
            var cible = await _keycloak.ObtenirAsync(id, ct);
            if (cible is null)
                return NotFound();

            if (!request.Actif)
            {
                if (cible.SuperAdmin)
                    return Conflict(new { Error = MessageSuperAdmin });
                if (EstMoi(id))
                    return Conflict(new { Error = "Vous ne pouvez pas désactiver votre propre compte." });
                if (cible.Actif && cible.Roles.Contains(RolesApplicatifs.Admin)
                    && await _keycloak.CompterAdminsActifsAsync(ct) <= 1)
                    return Conflict(new { Error = "Le dernier administrateur actif ne peut pas être désactivé." });
            }

            await _keycloak.DefinirActivationAsync(id, request.Actif, ct);
            _logger.LogInformation(
                "Admin {Admin} a {Action} le compte {Cible} ({Utilisateur}).",
                CallerId, request.Actif ? "activé" : "désactivé", id, cible.NomUtilisateur);
            return NoContent();
        });

    [HttpPost("{id}/reinitialisation-mdp")]
    public Task<IActionResult> ReinitialiserMotDePasse(string id, CancellationToken ct = default) =>
        Executer(async () =>
        {
            var cible = await _keycloak.ObtenirAsync(id, ct);
            if (cible is null)
                return NotFound();
            if (string.IsNullOrWhiteSpace(cible.Email))
                return BadRequest(new { Error = "Ce compte n'a pas d'adresse e-mail." });

            await _keycloak.EnvoyerReinitialisationMotDePasseAsync(id, ct);
            _logger.LogInformation(
                "Admin {Admin} a déclenché la réinitialisation du mot de passe de {Cible} ({Utilisateur}).",
                CallerId, id, cible.NomUtilisateur);
            return NoContent();
        });

    private bool EstMoi(string id) =>
        string.Equals(CallerId, id, StringComparison.OrdinalIgnoreCase);

    /// <summary>Traduit les pannes Keycloak en 503 sans exposer le corps de la réponse brute.</summary>
    private async Task<IActionResult> Executer(Func<Task<IActionResult>> action)
    {
        try
        {
            return await action();
        }
        catch (KeycloakIndisponibleException ex)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { Error = ex.Message });
        }
    }
}
