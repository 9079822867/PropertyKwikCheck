using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PropertyKwikCheck.Core.Abstractions;
using PropertyKwikCheck.Core.Rbac;

namespace PropertyKwikCheck.Api.Controllers;

[Authorize]
public sealed class AnalyticsController(IAnalyticsService analytics) : ApiControllerBase
{
    [HttpGet("analytics")]
    public async Task<IActionResult> Analytics()
    {
        CurrentUser.Require(Capability.ViewAnalytics);
        return Ok(await analytics.GetAnalyticsAsync(CurrentUser));
    }
}
