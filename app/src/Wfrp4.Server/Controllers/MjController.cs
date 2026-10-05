using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;
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
    [RequestSizeLimit(2 * 1024 * 1024)]
    public async Task<IActionResult> ExportPdf(
        MjPdfExportRequest request,
        [FromServices] MjPdfExportService pdfService,
        CancellationToken ct)
    {
        try
        {
            var result = await pdfService.GenerateAsync(request, ct);
            return File(result.Content, "application/pdf", result.FileName);
        }
        catch (ValidationException ex) { return BadRequest(new { Error = ex.Message }); }
        catch (PdfExportBusyException ex)
        {
            Response.Headers.RetryAfter = "2";
            return StatusCode(StatusCodes.Status429TooManyRequests, new { Error = ex.Message });
        }
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
