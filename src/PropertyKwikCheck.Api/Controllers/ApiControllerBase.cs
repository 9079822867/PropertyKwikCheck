using Microsoft.AspNetCore.Mvc;
using PropertyKwikCheck.Api.Security;
using PropertyKwikCheck.Core.Abstractions;
using PropertyKwikCheck.Core.Common;
using PropertyKwikCheck.Core.Security;

namespace PropertyKwikCheck.Api.Controllers;

[ApiController]
[Route("api")]
public abstract class ApiControllerBase : ControllerBase
{
    /// <summary>The authenticated principal, or throws 401 if missing.</summary>
    protected CurrentUser CurrentUser =>
        User.GetCurrentUser() ?? throw AppException.Unauthorized("Not authenticated");

    protected AuditContext Audit => new(
        User.GetCurrentUser(),
        HttpContext.Connection.RemoteIpAddress?.ToString(),
        Request.Headers.UserAgent.ToString());
}
