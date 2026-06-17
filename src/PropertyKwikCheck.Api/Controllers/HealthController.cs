using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PropertyKwikCheck.Api.Controllers;

[AllowAnonymous]
public sealed class HealthController : ApiControllerBase
{
    [HttpGet("health")]
    public IActionResult Health() => Ok(new { status = "ok", time = DateTime.UtcNow });
}
