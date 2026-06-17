namespace PropertyKwikCheck.Core.Domain;

/// <summary>
/// The central entity (spec §6). Property names map to the snake_case <c>leads</c>
/// columns via Dapper's underscore matching. <see cref="ReportData"/> holds the
/// full per-template report payload as a raw JSON string.
/// </summary>
public sealed class Lead
{
    public long Id { get; set; }
    public string ReqId { get; set; } = "";
    public string AssetFamily { get; set; } = "";
    public string PropertyType { get; set; } = "";
    public string Stage { get; set; } = Workflow.Stage.Fresh;
    public string ReportStatus { get; set; } = "Open";

    public string? Applicant { get; set; }
    public string? CoApplicant { get; set; }
    public string? Contact { get; set; }
    public string? Pin { get; set; }
    public string? Location { get; set; }

    public long? LenderCompanyId { get; set; }
    public string? LenderName { get; set; }
    public string? Branch { get; set; }

    public long? ValuatorUserId { get; set; }
    public string? ValuatorName { get; set; }
    public string? RoCompany { get; set; }

    public string? ExecName { get; set; }
    public string? ExecPhone { get; set; }
    public string? ExecEmail { get; set; }

    public string? LoanNo { get; set; }
    public string? ClaimNo { get; set; }
    public string? Source { get; set; }
    public string? RegNo { get; set; }

    public DateTime? LeadDate { get; set; }
    public DateTime? AssignedOn { get; set; }
    public DateTime? InspectionDate { get; set; }
    public DateTime? IssuedDate { get; set; }
    public DateTime? TatDue { get; set; }
    public int TatPct { get; set; }
    public string TatState { get; set; } = Workflow.TatState.Ok;

    public long? Value { get; set; }
    public string? Remarks { get; set; }
    public string? HoldRemarks { get; set; }

    public string? ReportData { get; set; }

    public long? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}
