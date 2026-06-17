using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PropertyKwikCheck.Core.Abstractions;
using PropertyKwikCheck.Core.Dtos;

namespace PropertyKwikCheck.Api.Controllers;

[Authorize]
public sealed class CompaniesController(IDirectoryService directory) : ApiControllerBase
{
    [HttpGet("companies")]
    public async Task<ActionResult<List<CompanyDto>>> List() => Ok(await directory.ListCompaniesAsync(CurrentUser));

    [HttpPost("companies")]
    public async Task<ActionResult<CompanyDto>> Create([FromBody] CreateCompanyRequest request)
    {
        var created = await directory.CreateCompanyAsync(request, CurrentUser, Audit);
        return StatusCode(201, created);
    }

    [HttpPatch("companies/{id:long}")]
    public async Task<ActionResult<CompanyDto>> Update(long id, [FromBody] UpdateCompanyRequest request)
        => Ok(await directory.UpdateCompanyAsync(id, request, CurrentUser, Audit));

    [HttpDelete("companies/{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
        await directory.DeleteCompanyAsync(id, CurrentUser, Audit);
        return Ok(new { ok = true });
    }
}
