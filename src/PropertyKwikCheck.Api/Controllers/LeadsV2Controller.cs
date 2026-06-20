using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PropertyKwikCheck.Api.Contracts;
using PropertyKwikCheck.Core.Abstractions;
using PropertyKwikCheck.Core.Dtos;
using PropertyKwikCheck.Core.Workflow;

namespace PropertyKwikCheck.Api.Controllers;

/// <summary>
/// Versioned, third-party-facing lead API (v2). Strongly typed request/response models
/// with documented parameters; backed by the same workflow engine as the internal API.
/// All endpoints require a bearer token and honour the caller's row-level scope.
/// </summary>
[Authorize]
[Produces("application/json")]
public sealed class LeadsV2Controller(ILeadService leads) : ApiControllerBase
{
    /// <summary>List leads in a pipeline stage (paged).</summary>
    /// <param name="stage">Pipeline stage code to filter by (default <c>assigned</c>). See <c>GET /api/statustypes</c>.</param>
    /// <param name="q">Optional free-text search across applicant, request id, lender, valuator and location.</param>
    /// <param name="page">1-based page number (default 1).</param>
    /// <param name="pageSize">Rows per page (default 50, max 200).</param>
    [HttpGet("v2/leads")]
    [ProducesResponseType(typeof(LeadV2ListResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<LeadV2ListResponse>> List(
        [FromQuery] string stage = Stage.Assigned,
        [FromQuery] string? q = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = LeadQuery.DefaultPageSize)
    {
        var query = new LeadQuery(stage, q, page, pageSize, null, LeadScope.From(CurrentUser));
        var result = await leads.ListAsync(query);
        return Ok(new LeadV2ListResponse
        {
            Rows = result.Rows.Select(ToV2).ToList(),
            Total = result.Total,
        });
    }

    /// <summary>Fetch a single lead by its numeric id.</summary>
    /// <param name="id">The lead's numeric identifier.</param>
    [HttpGet("v2/leads/{id:long}")]
    [ProducesResponseType(typeof(LeadV2Dto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LeadV2Dto>> Get(long id) => Ok(ToV2(await leads.GetAsync(id, CurrentUser)));

    /// <summary>Create a new lead from the given property type and optional initial report fields.</summary>
    /// <param name="request">The property type and any initial report fields.</param>
    [HttpPost("v2/leads")]
    [ProducesResponseType(typeof(LeadV2Dto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<LeadV2Dto>> Create([FromBody] CreateLeadV2Request request)
    {
        var created = await leads.CreateAsync(
            new CreateLeadRequest { Ptype = request.PropertyType, Data = request.Report },
            CurrentUser, Audit);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, ToV2(created));
    }

    /// <summary>Partially update a lead — merge report fields and/or move to a new stage.</summary>
    /// <param name="id">The lead's numeric identifier.</param>
    /// <param name="request">The fields to change. All properties are optional.</param>
    [HttpPut("v2/leads/{id:long}")]
    [ProducesResponseType(typeof(LeadV2Dto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<LeadV2Dto>> Update(long id, [FromBody] UpdateLeadV2Request request)
    {
        var updated = await leads.UpdateAsync(id, new UpdateLeadRequest
        {
            Stage = request.Stage,
            Data = request.Report,
            Value = request.Value,
            Remarks = request.Remarks,
        }, CurrentUser, Audit);
        return Ok(ToV2(updated));
    }

    /// <summary>Soft-delete a lead.</summary>
    /// <param name="id">The lead's numeric identifier.</param>
    [HttpDelete("v2/leads/{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(long id)
    {
        await leads.DeleteAsync(id, CurrentUser, Audit);
        return Ok(new { ok = true });
    }

    private static LeadV2Dto ToV2(LeadDto l) => new()
    {
        Id = l.Id,
        ReqId = l.ReqId,
        AssetFamily = l.Type,
        PropertyType = l.Ptype,
        Stage = l.Stage,
        ReportStatus = l.ReportStatus,
        Applicant = l.Applicant,
        Contact = l.Contact,
        Lender = l.Lender,
        Branch = l.Branch,
        Valuator = l.Valuator,
        RoCompany = l.RoCompany,
        Value = l.Value,
        LeadDate = l.LeadDate,
        AssignedOn = l.AssignedOn,
        Report = l.Data,
    };
}
