using PropertyKwikCheck.Core.Abstractions;
using PropertyKwikCheck.Core.Common;
using PropertyKwikCheck.Core.Security;

namespace PropertyKwikCheck.Infrastructure.Services;

/// <summary>
/// Per-screen datasets (spec §8.8). All tuples are positional — preserve order/length.
/// Invoices, site-visit schedule, issued reports and master categories are DB-derived.
/// </summary>
public sealed class ScreenService(IReportingRepository reporting) : IScreenService
{
    private static object?[] R(params object?[] cells) => cells;

    public async Task<object> GetScreenAsync(string name, CurrentUser user) => name switch
    {
        "billing" => await BillingAsync(),
        "yard" => await YardAsync(),
        "mis" => await MisAsync(),
        "reports" => await ReportsAsync(),
        "documents" => await DocumentsAsync(),
        "master" => await MasterAsync(),
        _ => throw AppException.NotFound("Unknown screen"),
    };

    private async Task<object> BillingAsync()
    {
        var invoices = await reporting.InvoicesAsync();
        return new Dictionary<string, object?>
        {
            ["stats"] = new List<object?[]>
            {
                R("Invoices Raised", invoices.Count.ToString(), "blue", "billing"),
                R("Amount Billed", "₹ 48,20,000", "good", "rupee"),
                R("Pending", "12", "amber", "clock"),
                R("Overdue", "3", "poor", "alert"),
            },
            ["invoices"] = invoices,
        };
    }

    private async Task<object> YardAsync()
    {
        var schedule = await reporting.YardScheduleAsync();
        return new Dictionary<string, object?>
        {
            ["valuers"] = new List<object?[]>
            {
                R("Rahul Mehta", 3, "Mumbai", "property"),
                R("Ajay Malviya", 2, "Jaipur", "plot"),
            },
            ["schedule"] = schedule,
        };
    }

    private async Task<object> MisAsync() => new Dictionary<string, object?>
    {
        ["reports"] = new List<object?[]>
        {
            R("TAT / SLA Report", "Turnaround performance by bank & valuer", "98.2%", "SLA met", "trend", "good"),
            R("Valuer Productivity", "Cases closed per valuer", "48", "top performer", "user", "blue"),
            R("Rejection Analysis", "Reasons & rates", "2.1%", "rejection rate", "reject", "poor"),
        },
        ["weekly"] = await reporting.WeeklyLeadCountsAsync(),
        ["snapshot"] = await reporting.MisSnapshotAsync(),
    };

    private async Task<object> ReportsAsync()
    {
        var rows = await reporting.IssuedReportsAsync(50);
        return new Dictionary<string, object?>
        {
            ["stats"] = new List<object?[]>
            {
                R("Reports Issued", rows.Count.ToString(), "good", "doc"),
                R("This Month", "214", "blue", "trend"),
                R("Avg / Day", "9.4", "blue", "clock"),
            },
            ["rows"] = rows,
        };
    }

    private async Task<object> DocumentsAsync() => new Dictionary<string, object?>
    {
        ["folders"] = new List<object?[]>
        {
            R("Sale Deeds & Titles", 842, "doc", "blue"),
            R("Site Photographs", 1960, "map", "good"),
            R("Valuation Reports", 3214, "doc", "navy"),
            R("Encumbrance Certificates", 604, "shield", "amber"),
            R("Bank NOCs / NDAs", 288, "doc", "poor"),
            R("Revenue Records (RoR)", 511, "layers", "slate"),
        },
        ["recent"] = await reporting.RecentDocumentsAsync(20),
    };

    private async Task<object> MasterAsync()
    {
        var cats = await reporting.MasterCategoriesAsync();
        return cats.Select(c => R(c.Label, c.Count, "layers", c.Samples, c.Key)).ToList();
    }
}
