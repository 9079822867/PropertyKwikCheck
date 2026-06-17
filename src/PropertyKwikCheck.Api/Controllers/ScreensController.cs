using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PropertyKwikCheck.Core.Abstractions;

namespace PropertyKwikCheck.Api.Controllers;

[Authorize]
public sealed class ScreensController(IScreenService screens) : ApiControllerBase
{
    [HttpGet("screens/{name}")]
    public async Task<IActionResult> Screen(string name) => Ok(await screens.GetScreenAsync(name, CurrentUser));
}
