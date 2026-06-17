namespace PropertyKwikCheck.Core.Rbac;

/// <summary>Workflow capabilities (spec §9.1), enforced independent of the org role/userType.</summary>
public enum Capability
{
    ViewLeads,
    CreateLead,
    EditReport,
    UploadFiles,
    AssignReassign,
    RejectDuplicate,
    SubmitForQc,
    QcApproveHold,
    PricingSignoff,
    AuthoriseIssue,
    ManageUsers,
    ManageCompanies,
    ManageMasters,
    ManageYard,
    ViewBilling,
    ViewAnalytics,
    DeleteLead,
}

/// <summary>Row-level visibility scope a user has over leads (spec §9.2).</summary>
public enum Scope
{
    /// <summary>Sees/acts on all leads.</summary>
    All,

    /// <summary>Only leads where they are the assigned valuator.</summary>
    OwnLeads,

    /// <summary>Only leads belonging to their company (lender).</summary>
    OwnCompany,
}
