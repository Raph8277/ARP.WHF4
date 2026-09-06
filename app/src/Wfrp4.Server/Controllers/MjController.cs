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

    [HttpGet("pdf/template")]
    public IActionResult GetTemplate([FromServices] IWebHostEnvironment env)
    {
        var path = Path.Combine(env.ContentRootPath, "PdfTemplates", "wfrp4-mj-page-background.jpg");
        if (!System.IO.File.Exists(path))
            return NotFound();
        return PhysicalFile(path, "image/jpeg");
    }
}
