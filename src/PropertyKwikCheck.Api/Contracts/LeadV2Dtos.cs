using System.ComponentModel.DataAnnotations;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace PropertyKwikCheck.Api.Contracts;

/// <summary>
/// Public v2 lead resource returned to third-party integrators. Strongly typed and
/// fully documented; the free-form <see cref="Report"/> carries the stage-by-stage payload.
/// </summary>
public sealed class LeadV2Dto
{
    /// <summary>Internal numeric identifier of the lead.</summary>
    [JsonPropertyName("id")] public long Id { get; set; }

    /// <summary>Human-facing request id, e.g. <c>4WRP04812</c>.</summary>
    [JsonPropertyName("reqId")] public string ReqId { get; set; } = "";

    /// <summary>Asset family: <c>property</c>, <c>plot</c> or <c>agri</c>.</summary>
    [JsonPropertyName("assetFamily")] public string AssetFamily { get; set; } = "";

    /// <summary>Property sub-type, e.g. Residential, Commercial, Plot, Agricultural Land.</summary>
    [JsonPropertyName("propertyType")] public string PropertyType { get; set; } = "";

    /// <summary>Pipeline stage code. Resolve labels via <c>GET /api/statustypes</c>.</summary>
    [JsonPropertyName("stage")] public string Stage { get; set; } = "";

    /// <summary>Human-readable report status for the current stage.</summary>
    [JsonPropertyName("reportStatus")] public string ReportStatus { get; set; } = "";

    /// <summary>Primary applicant / borrower name.</summary>
    [JsonPropertyName("applicant")] public string? Applicant { get; set; }

    /// <summary>Applicant contact number.</summary>
    [JsonPropertyName("contact")] public string? Contact { get; set; }

    /// <summary>Requesting lender / bank name.</summary>
    [JsonPropertyName("lender")] public string? Lender { get; set; }

    /// <summary>Lender branch.</summary>
    [JsonPropertyName("branch")] public string? Branch { get; set; }

    /// <summary>Assigned RO valuator name.</summary>
    [JsonPropertyName("valuator")] public string? Valuator { get; set; }

    /// <summary>RO valuation firm the valuator belongs to.</summary>
    [JsonPropertyName("roCompany")] public string? RoCompany { get; set; }

    /// <summary>Adopted / fair-market value in INR, when valued.</summary>
    [JsonPropertyName("value")] public long? Value { get; set; }

    /// <summary>Lead intake date (yyyy-MM-dd).</summary>
    [JsonPropertyName("leadDate")] public string? LeadDate { get; set; }

    /// <summary>Date the lead was assigned to a valuator (d/M/yyyy).</summary>
    [JsonPropertyName("assignedOn")] public string? AssignedOn { get; set; }

    /// <summary>Full report payload (flat key/value), captured stage-by-stage.</summary>
    [JsonPropertyName("report")] public JsonObject? Report { get; set; }
}

/// <summary>Paged list envelope for <c>GET /api/v2/leads</c>.</summary>
public sealed class LeadV2ListResponse
{
    /// <summary>The page of leads.</summary>
    [JsonPropertyName("rows")] public List<LeadV2Dto> Rows { get; set; } = [];

    /// <summary>Total leads matching the query across all pages.</summary>
    [JsonPropertyName("total")] public int Total { get; set; }
}

/// <summary>Body of <c>POST /api/v2/leads</c>.</summary>
public sealed class CreateLeadV2Request
{
    /// <summary>
    /// Property sub-type. One of: Residential, Commercial, Industrial, Plot, Agricultural Land.
    /// Determines the asset family, report template and request-id prefix.
    /// </summary>
    [Required]
    [JsonPropertyName("propertyType")] public string PropertyType { get; set; } = "";

    /// <summary>Optional initial report fields (e.g. applicant, lender, loanNo, branch).</summary>
    [JsonPropertyName("report")] public JsonObject? Report { get; set; }
}

/// <summary>Body of <c>PUT /api/v2/leads/{id}</c> — all fields optional (partial update).</summary>
public sealed class UpdateLeadV2Request
{
    /// <summary>Target stage code to transition to. Must be a legal next stage. See <c>GET /api/statustypes</c>.</summary>
    [JsonPropertyName("stage")] public string? Stage { get; set; }

    /// <summary>Report fields to merge into the stored payload.</summary>
    [JsonPropertyName("report")] public JsonObject? Report { get; set; }

    /// <summary>Adopted / fair-market value in INR.</summary>
    [JsonPropertyName("value")] public long? Value { get; set; }

    /// <summary>Free-text remarks.</summary>
    [JsonPropertyName("remarks")] public string? Remarks { get; set; }
}
