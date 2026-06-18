using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PropertyKwikCheck.Core.Abstractions;
using PropertyKwikCheck.Core.Common;
using PropertyKwikCheck.Core.Rbac;

namespace PropertyKwikCheck.Api.Controllers;

/// <summary>Master-data lookup CRUD (spec §8.10 master data). Gated by ManageMasters.</summary>
[Authorize]
public sealed class MasterController(IMasterRepository master) : ApiControllerBase
{
    public sealed class AddItemRequest
    {
        public string Category { get; set; } = "";
        public string Value { get; set; } = "";
    }

    [HttpGet("master/{category}")]
    public async Task<IActionResult> Items(string category)
    {
        var items = await master.ItemsAsync(category);
        return Ok(items.Select(i => new { id = i.Id, value = i.Value }));
    }

    [HttpPost("master")]
    public async Task<IActionResult> Add([FromBody] AddItemRequest req)
    {
        CurrentUser.Require(Capability.ManageMasters);
        if (string.IsNullOrWhiteSpace(req.Category) || string.IsNullOrWhiteSpace(req.Value))
            throw AppException.Validation("category and value are required");
        var id = await master.AddAsync(req.Category, req.Value);
        return StatusCode(201, new { id, category = req.Category, value = req.Value });
    }

    [HttpDelete("master/{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
        CurrentUser.Require(Capability.ManageMasters);
        await master.DeleteAsync(id);
        return Ok(new { ok = true });
    }
}
