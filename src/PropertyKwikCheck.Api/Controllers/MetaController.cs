using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PropertyKwikCheck.Core.Abstractions;

namespace PropertyKwikCheck.Api.Controllers;

[Authorize]
public sealed class MetaController(ILeadService leads) : ApiControllerBase
{
    /// <summary>Sidebar badge counts (spec §8.6). Scoped to the current user.</summary>
    [HttpGet("meta")]
    public async Task<IActionResult> Meta()
        => Ok(new { bucketCounts = await leads.BucketCountsAsync(CurrentUser) });
}
