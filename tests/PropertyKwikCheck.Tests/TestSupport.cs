using PropertyKwikCheck.Core.Abstractions;
using PropertyKwikCheck.Core.Domain;
using PropertyKwikCheck.Core.Security;

namespace PropertyKwikCheck.Tests;

/// <summary>Deterministic clock for time-dependent tests.</summary>
public sealed class FixedClock(DateTime now) : IClock
{
    public DateTime UtcNow { get; } = now;
}

public static class TestData
{
    // UserType ids: 19 Super Admin (All), 8 RO Valuators (OwnLeads), 1 Client Executive (OwnCompany).
    public static CurrentUser SuperAdmin(long id = 1) => new(id, "Super Admin", "sa@kc.in", 3, 19, null);
    public static CurrentUser Valuer(long id = 6, long? company = null) => new(id, "Rahul Mehta", "rahul@kc.in", 2, 8, company);
    public static CurrentUser Client(long id = 9, long company = 2) => new(id, "Meena Patil", "meena@kc.in", 1, 1, company);
    public static CurrentUser QcReviewer(long id = 3) => new(id, "Neha Joshi", "qc@kc.in", 3, 11, null);

    public static Lead Lead(long id = 100, string stage = "assigned") => new()
    {
        Id = id,
        ReqId = "KC-RESI-2026-00100",
        AssetFamily = "property",
        PropertyType = "Residential",
        Stage = stage,
        ReportStatus = "Assigned to Valuer",
        Applicant = "Mr. Test",
        LenderCompanyId = 2,
        LenderName = "HDFC Bank Ltd",
        ValuatorUserId = 6,
        ValuatorName = "Rahul Mehta",
        ReportData = """{"a":"1","b":"2"}""",
        TatPct = 40,
        TatState = "ok",
    };
}
