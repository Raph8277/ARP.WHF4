using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wfrp4.Server.Services;
using Wfrp4.Shared.DTOs;

namespace Wfrp4.Server.Controllers;

[ApiController]
[Route("api/mj")]
[Authorize]
public class MjController : ControllerBase
{
    [HttpPost("pdf")]
    public async Task<IActionResult> ExportPdf(
        MjPdfExportRequest request,
        [FromServices] MjPdfExportService pdfService,
        CancellationToken ct)
    {
        var result = await pdfService.GenerateAsync(request, ct);
        return File(result.Content, "application/pdf", result.FileName);
    }
}
