using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace PropertyKwikCheck.Core.Dtos;

/// <summary>
/// The lead shape the frontend consumes (spec §6). camelCase keys; <c>data</c> is the
/// report payload returned verbatim. Snake_case DB columns are never exposed.
/// </summary>
public sealed class LeadDto
{
    [JsonPropertyName("id")] public long Id { get; set; }
    [JsonPropertyName("reqId")] public string ReqId { get; set; } = "";
    [JsonPropertyName("type")] public string Type { get; set; } = "";
    [JsonPropertyName("ptype")] public string Ptype { get; set; } = "";
    [JsonPropertyName("stage")] public string Stage { get; set; } = "";
    [JsonPropertyName("reportStatus")] public string ReportStatus { get; set; } = "";

    [JsonPropertyName("applicant")] public string? Applicant { get; set; }
    [JsonPropertyName("coApplicant")] public string? CoApplicant { get; set; }
    [JsonPropertyName("contact")] public string? Contact { get; set; }
    [JsonPropertyName("pin")] public string? Pin { get; set; }
    [JsonPropertyName("location")] public string? Location { get; set; }

    [JsonPropertyName("lender")] public string? Lender { get; set; }
    [JsonPropertyName("branch")] public string? Branch { get; set; }
    [JsonPropertyName("valuator")] public string? Valuator { get; set; }
    [JsonPropertyName("roCompany")] public string? RoCompany { get; set; }

    [JsonPropertyName("exec")] public string? Exec { get; set; }
    [JsonPropertyName("execPhone")] public string? ExecPhone { get; set; }
    [JsonPropertyName("execEmail")] public string? ExecEmail { get; set; }

    [JsonPropertyName("loanNo")] public string? LoanNo { get; set; }
    [JsonPropertyName("claimNo")] public string? ClaimNo { get; set; }
    [JsonPropertyName("regNo")] public string? RegNo { get; set; }
    [JsonPropertyName("source")] public string? Source { get; set; }

    [JsonPropertyName("leadDate")] public string? LeadDate { get; set; }
    [JsonPropertyName("assignedOn")] public string? AssignedOn { get; set; }
    [JsonPropertyName("inspectionDate")] public string? InspectionDate { get; set; }
    [JsonPropertyName("issuedDate")] public string? IssuedDate { get; set; }

    [JsonPropertyName("tatPct")] public int TatPct { get; set; }
    [JsonPropertyName("tatState")] public string TatState { get; set; } = "ok";

    [JsonPropertyName("value")] public long? Value { get; set; }
    [JsonPropertyName("remarks")] public string? Remarks { get; set; }
    [JsonPropertyName("holdRemarks")] public string? HoldRemarks { get; set; }

    [JsonPropertyName("data")] public JsonObject? Data { get; set; }
}

/// <summary>Response of <c>GET /api/leads</c> (spec §8.1).</summary>
public sealed class LeadListResponse
{
    [JsonPropertyName("rows")] public List<LeadDto> Rows { get; set; } = [];
    [JsonPropertyName("total")] public int Total { get; set; }
    [JsonPropertyName("counts")] public Dictionary<string, int> Counts { get; set; } = [];
}

/// <summary>Body of <c>POST /api/leads</c> (spec §8.3).</summary>
public sealed class CreateLeadRequest
{
    [JsonPropertyName("ptype")] public string Ptype { get; set; } = "";
    [JsonPropertyName("data")] public JsonObject? Data { get; set; }
}

/// <summary>
/// Body of <c>PATCH /api/leads/{id}</c> (spec §8.4). Discriminated by <see cref="Action"/>:
/// null = edit-save (deep-merge <see cref="Data"/> + shallow-assign columns);
/// "reassign" = set valuator; "reject" = move to rejected.
/// </summary>
public sealed class UpdateLeadRequest
{
    [JsonPropertyName("action")] public string? Action { get; set; }
    [JsonPropertyName("valuator")] public string? Valuator { get; set; }
    [JsonPropertyName("valuatorUserId")] public long? ValuatorUserId { get; set; }
    [JsonPropertyName("roCompanyId")] public long? RoCompanyId { get; set; }
    [JsonPropertyName("roCompany")] public string? RoCompany { get; set; }
    [JsonPropertyName("stage")] public string? Stage { get; set; }
    [JsonPropertyName("value")] public long? Value { get; set; }
    [JsonPropertyName("reportStatus")] public string? ReportStatus { get; set; }
    [JsonPropertyName("remarks")] public string? Remarks { get; set; }
    [JsonPropertyName("holdRemarks")] public string? HoldRemarks { get; set; }
    [JsonPropertyName("data")] public JsonObject? Data { get; set; }
}
