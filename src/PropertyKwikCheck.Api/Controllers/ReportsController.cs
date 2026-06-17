using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PropertyKwikCheck.Core.Abstractions;

namespace PropertyKwikCheck.Api.Controllers;

[Authorize]
public sealed class ReportsController(IReportPdfService pdf) : ApiControllerBase
{
    /// <summary>Renders/serves the lead's valuation report PDF (spec §8.12).</summary>
    [HttpGet("leads/{id:long}/report.pdf")]
    public async Task<IActionResult> Report(long id)
    {
        var dl = await pdf.GenerateAsync(id, CurrentUser);
        // inline so the browser can preview; download still works
        Response.Headers.ContentDisposition = $"inline; filename=\"{dl.FileName}\"";
        return File(dl.Content, dl.Mime);
    }
}
