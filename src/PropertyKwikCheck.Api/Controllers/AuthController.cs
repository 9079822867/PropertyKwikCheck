using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PropertyKwikCheck.Core.Abstractions;
using PropertyKwikCheck.Core.Dtos;

namespace PropertyKwikCheck.Api.Controllers;

public sealed class AuthController(IAuthService auth) : ApiControllerBase
{
    [AllowAnonymous]
    [HttpPost("auth/login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var ua = Request.Headers.UserAgent.ToString();
        return Ok(await auth.LoginAsync(request, ip, ua));
    }

    [AllowAnonymous]
    [HttpPost("auth/refresh")]
    public async Task<ActionResult<TokenPair>> Refresh([FromBody] RefreshRequest request)
        => Ok(await auth.RefreshAsync(request.RefreshToken));

    [Authorize]
    [HttpPost("auth/logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshRequest request)
    {
        await auth.LogoutAsync(request.RefreshToken);
        return Ok(new { ok = true });
    }

    [Authorize]
    [HttpGet("auth/me")]
    public async Task<ActionResult<UserDto>> Me() => Ok(await auth.MeAsync(CurrentUser));
}
