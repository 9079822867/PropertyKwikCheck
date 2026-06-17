using System.Globalization;
using System.Text.Json.Nodes;
using PropertyKwikCheck.Core.Domain;
using PropertyKwikCheck.Core.Dtos;

namespace PropertyKwikCheck.Core.Mapping;

/// <summary>
/// Maps a <see cref="Lead"/> row to the camelCase <see cref="LeadDto"/> the frontend
/// expects (spec §6.3), including date formatting and verbatim <c>data</c> passthrough.
/// </summary>
public static class LeadMapper
{
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    /// <summary><c>assignedOn</c> is serialized d/M/yyyy (the format the UI parses).</summary>
    public static string? FormatAssignedOn(DateTime? d) => d?.ToString("d/M/yyyy", Inv);

    /// <summary>Other top-level dates serialize as ISO yyyy-MM-dd.</summary>
    public static string? FormatIso(DateTime? d) => d?.ToString("yyyy-MM-dd", Inv);

    public static JsonObject? ParseData(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        return JsonNode.Parse(json) as JsonObject;
    }

    public static LeadDto ToDto(Lead l) => new()
    {
        Id = l.Id,
        ReqId = l.ReqId,
        Type = l.AssetFamily,
        Ptype = l.PropertyType,
        Stage = l.Stage,
        ReportStatus = l.ReportStatus,
        Applicant = l.Applicant,
        CoApplicant = l.CoApplicant,
        Contact = l.Contact,
        Pin = l.Pin,
        Location = l.Location,
        Lender = l.LenderName,
        Branch = l.Branch,
        Valuator = l.ValuatorName,
        RoCompany = l.RoCompany,
        Exec = l.ExecName,
        ExecPhone = l.ExecPhone,
        ExecEmail = l.ExecEmail,
        LoanNo = l.LoanNo,
        ClaimNo = l.ClaimNo,
        RegNo = l.RegNo,
        Source = l.Source,
        LeadDate = FormatIso(l.LeadDate),
        AssignedOn = FormatAssignedOn(l.AssignedOn),
        InspectionDate = FormatIso(l.InspectionDate),
        IssuedDate = FormatIso(l.IssuedDate),
        TatPct = l.TatPct,
        TatState = l.TatState,
        Value = l.Value,
        Remarks = l.Remarks,
        HoldRemarks = l.HoldRemarks,
        Data = ParseData(l.ReportData),
    };
}
