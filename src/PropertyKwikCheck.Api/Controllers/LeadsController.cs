using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PropertyKwikCheck.Core.Abstractions;
using PropertyKwikCheck.Core.Dtos;
using PropertyKwikCheck.Core.Workflow;

namespace PropertyKwikCheck.Api.Controllers;

[Authorize]
public sealed class LeadsController(ILeadService leads) : ApiControllerBase
{
    [HttpGet("leads")]
    public async Task<ActionResult<LeadListResponse>> List(
        [FromQuery] string bucket = Stage.Assigned,
        [FromQuery] string? q = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = LeadQuery.DefaultPageSize,
        [FromQuery] string? sort = null)
    {
        var query = new LeadQuery(bucket, q, page, pageSize, sort, LeadScope.From(CurrentUser));
        return Ok(await leads.ListAsync(query));
    }

    [HttpGet("leads/{id:long}")]
    public async Task<ActionResult<LeadDto>> Get(long id) => Ok(await leads.GetAsync(id, CurrentUser));

    [HttpPost("leads")]
    public async Task<ActionResult<LeadDto>> Create([FromBody] CreateLeadRequest request)
    {
        var created = await leads.CreateAsync(request, CurrentUser, Audit);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [HttpPatch("leads/{id:long}")]
    public async Task<ActionResult<LeadDto>> Update(long id, [FromBody] UpdateLeadRequest request)
        => Ok(await leads.UpdateAsync(id, request, CurrentUser, Audit));

    [HttpDelete("leads/{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
        await leads.DeleteAsync(id, CurrentUser, Audit);
        return Ok(new { ok = true });
    }
}
