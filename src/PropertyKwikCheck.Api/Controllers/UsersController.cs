using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PropertyKwikCheck.Core.Abstractions;
using PropertyKwikCheck.Core.Dtos;
using PropertyKwikCheck.Core.Rbac;

namespace PropertyKwikCheck.Api.Controllers;

[Authorize]
public sealed class UsersController(IDirectoryService directory, IUserRepository users) : ApiControllerBase
{
    [HttpGet("users")]
    public async Task<ActionResult<List<UserDto>>> List() => Ok(await directory.ListUsersAsync(CurrentUser));

    [HttpPost("users")]
    public async Task<ActionResult<UserDto>> Create([FromBody] CreateUserRequest request)
    {
        var created = await directory.CreateUserAsync(request, CurrentUser, Audit);
        return StatusCode(201, created);
    }

    [HttpPatch("users/{id:long}")]
    public async Task<ActionResult<UserDto>> Update(long id, [FromBody] UpdateUserRequest request)
        => Ok(await directory.UpdateUserAsync(id, request, CurrentUser, Audit));

    [HttpDelete("users/{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
        await directory.DeleteUserAsync(id, CurrentUser, Audit);
        return Ok(new { ok = true });
    }

    // Lookups backing the user create/edit forms.
    [HttpGet("roles")]
    public async Task<IActionResult> Roles() => Ok(await users.RolesAsync());

    [HttpGet("usertypes")]
    public async Task<IActionResult> UserTypes() => Ok(await users.UserTypesAsync());

    /// <summary>RO valuators for lead assignment, optionally filtered to one RO company (spec: assign company → valuator).</summary>
    [HttpGet("valuators")]
    public async Task<IActionResult> Valuators([FromQuery] long? companyId = null)
    {
        CurrentUser.Require(Capability.AssignReassign);
        var rows = await users.ValuatorsAsync(companyId);
        return Ok(rows.Select(u => new
        {
            id = u.Id,
            name = u.Name,
            company = u.CompanyName,
            companyId = u.CompanyId,
            licenceNo = u.LicenceNo,
        }));
    }
}
