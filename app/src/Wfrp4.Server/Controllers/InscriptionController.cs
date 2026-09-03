using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wfrp4.Server.Services;
using Wfrp4.Shared.DTOs;

namespace Wfrp4.Server.Controllers;

[ApiController]
[Route("api/inscription")]
public class InscriptionController : ControllerBase
{
    private readonly KeycloakAdminService _keycloakAdminService;

    public InscriptionController(KeycloakAdminService keycloakAdminService)
    {
        _keycloakAdminService = keycloakAdminService;
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Inscrire(InscriptionRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        try
        {
            await _keycloakAdminService.CreerUtilisateurAsync(request, ct);
            return Ok(new { Message = "Compte créé avec succès. Vous pouvez maintenant vous connecter." });
        }
        catch (InscriptionConflictException ex)
        {
            return Conflict(new { Error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }
}
