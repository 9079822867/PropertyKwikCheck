using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PropertyKwikCheck.Api.Services;

namespace PropertyKwikCheck.Api.Controllers;

/// <summary>
/// Public, no-login access to a lead's inspection report payload — backs the
/// shareable "View Report" page (and the verify.kwikcheck.in concept). All data is
/// composed dynamically from the lead; see <see cref="IPublicReportService"/>.
/// </summary>
[AllowAnonymous]
public sealed class PublicReportsController(IPublicReportService reports) : ApiControllerBase
{
    [HttpGet("public/reports/{id:long}")]
    public async Task<IActionResult> Get(long id) => Ok(await reports.GetReportAsync(id));
}
