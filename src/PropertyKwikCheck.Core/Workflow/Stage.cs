namespace PropertyKwikCheck.Core.Workflow;

/// <summary>
/// The 12 lead pipeline buckets (spec §4). Stored verbatim as strings in the DB
/// (varchar + check constraint) and exposed verbatim over the API.
/// </summary>
public static class Stage
{
    public const string Fresh = "fresh";
    public const string Ro = "ro";
    public const string Assigned = "assigned";
    public const string Reassigned = "reassigned";
    public const string RoConfirmation = "ro_confirmation";
    public const string Qc = "qc";
    public const string QcHold = "qc_hold";
    public const string Pricing = "pricing";
    public const string Completed = "completed";
    public const string OutOfTat = "out_of_tat";
    public const string Duplicate = "duplicate";
    public const string Rejected = "rejected";

    /// <summary>Every bucket, in sidebar order (spec §4 table).</summary>
    public static readonly string[] All =
    [
        Fresh, Ro, Assigned, Reassigned, RoConfirmation, Qc, QcHold, Pricing,
        Completed, OutOfTat, Duplicate, Rejected
    ];

    /// <summary>The linear forward pipeline (spec STAGE_ORDER).</summary>
    public static readonly string[] Order =
    [
        Fresh, Ro, Assigned, Reassigned, RoConfirmation, Qc, QcHold, Pricing, Completed
    ];

    /// <summary>Terminal buckets — no outgoing transitions.</summary>
    public static readonly string[] Terminal = [Completed, Duplicate, Rejected];

    public static bool IsValid(string? stage) => stage is not null && Array.IndexOf(All, stage) >= 0;

    public static bool IsTerminal(string stage) => Array.IndexOf(Terminal, stage) >= 0;
}
