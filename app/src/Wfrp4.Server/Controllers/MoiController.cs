using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wfrp4.Shared.DTOs;

namespace Wfrp4.Server.Controllers;

/// <summary>
/// Expose au client les rôles effectifs du jeton après transformation : l'id_token OIDC ne porte pas
/// realm_access. Sert uniquement à l'affichage, chaque endpoint revérifie ses propres droits.
/// </summary>
[ApiController]
[Route("api/moi")]
[Authorize]
public class MoiController : ControllerBase
{
    [HttpGet]
    public ActionResult<MoiDto> Get()
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(id))
            return Unauthorized();

        return new MoiDto
        {
            Id = id,
            Roles = RolesApplicatifs.Tous.Where(User.IsInRole).ToList(),
        };
    }
}
