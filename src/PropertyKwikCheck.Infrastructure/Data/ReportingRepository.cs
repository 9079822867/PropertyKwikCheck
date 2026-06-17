using System.Globalization;
using Dapper;
using PropertyKwikCheck.Core.Abstractions;
using PropertyKwikCheck.Core.Mapping;

namespace PropertyKwikCheck.Infrastructure.Data;

public sealed class ReportingRepository(IDbConnectionFactory factory) : IReportingRepository
{
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    private static readonly Dictionary<string, string> CategoryLabels = new()
    {
        ["banks"] = "Banks & Lenders",
        ["valuers"] = "Valuers",
        ["executives"] = "Bank Executives",
        ["cities"] = "Cities",
        ["roles"] = "Roles",
        ["sources"] = "Lead Sources",
        ["ro_companies"] = "RO Companies",
        ["doc_types"] = "Document Types",
        ["rejection_reasons"] = "Rejection Reasons",
        ["asset_types"] = "Asset Types",
        ["valuation_purposes"] = "Valuation Purposes",
    };

    public async Task<List<(string Valuer, int Count)>> ValuerProductivityAsync()
    {
        using var conn = await factory.OpenAsync();
        var rows = await conn.QueryAsync<(string Valuer, int Count)>("""
            SELECT valuator_name AS Valuer, COUNT(*) AS Count
            FROM leads
            WHERE valuator_name IS NOT NULL AND deleted_at IS NULL
            GROUP BY valuator_name
            ORDER BY COUNT(*) DESC
            """);
        return rows.ToList();
    }

    public async Task<List<object?[]>> InvoicesAsync()
    {
        using var conn = await factory.OpenAsync();
        var rows = await conn.QueryAsync<(string InvoiceNo, string Company, string Period, int LeadCount, long Amount, string Status)>("""
            SELECT i.invoice_no AS InvoiceNo, c.name AS Company, i.period AS Period,
                   i.lead_count AS LeadCount, i.amount AS Amount, i.status AS Status
            FROM invoices i
            JOIN companies c ON c.id = i.company_id
            ORDER BY i.created_at DESC
            """);
        return rows.Select(r => new object?[]
        {
            r.InvoiceNo, r.Company, r.Period, r.LeadCount, Inr.Format(r.Amount), r.Status, ToneForInvoice(r.Status),
        }).ToList();
    }

    public async Task<List<object?[]>> SiteVisitsForDashboardAsync()
    {
        using var conn = await factory.OpenAsync();
        var rows = await conn.QueryAsync<(DateTime ScheduledAt, string ReqId, string AssetFamily, string? Location)>("""
            SELECT TOP (20) sv.scheduled_at AS ScheduledAt, l.req_id AS ReqId,
                   l.asset_family AS AssetFamily, sv.location AS Location
            FROM site_visits sv
            JOIN leads l ON l.id = sv.lead_id
            WHERE sv.status IN ('Scheduled','En route','Checked-in')
            ORDER BY sv.scheduled_at
            """);
        return rows.Select(r => new object?[]
        {
            r.ScheduledAt.Day.ToString(Inv),
            r.ScheduledAt.ToString("MMM", Inv).ToUpperInvariant(),
            r.ReqId, r.AssetFamily, r.Location ?? "",
            r.ScheduledAt.ToString("h:mm tt", Inv),
        }).ToList();
    }

    public async Task<List<object?[]>> YardScheduleAsync()
    {
        using var conn = await factory.OpenAsync();
        var rows = await conn.QueryAsync<(DateTime ScheduledAt, string? Valuer, string ReqId, string AssetFamily, string? Location, string Status)>("""
            SELECT sv.scheduled_at AS ScheduledAt, u.name AS Valuer, l.req_id AS ReqId,
                   l.asset_family AS AssetFamily, sv.location AS Location, sv.status AS Status
            FROM site_visits sv
            JOIN leads l ON l.id = sv.lead_id
            LEFT JOIN users u ON u.id = sv.valuer_user_id
            ORDER BY sv.scheduled_at
            """);
        return rows.Select(r => new object?[]
        {
            r.ScheduledAt.ToString("HH:mm", Inv), r.Valuer ?? "—", r.ReqId, r.AssetFamily,
            r.Location ?? "", r.Status, ToneForVisit(r.Status),
        }).ToList();
    }

    public async Task<List<(string Category, int Count, List<string> Samples)>> MasterCategoriesAsync()
    {
        using var conn = await factory.OpenAsync();
        var rows = await conn.QueryAsync<(string Category, string Value, int Sort)>("""
            SELECT category AS Category, value AS Value, sort AS Sort
            FROM master_lookups
            WHERE active = 1
            ORDER BY category, sort, value
            """);
        return rows
            .GroupBy(r => r.Category)
            .Select(g => (
                Category: CategoryLabels.GetValueOrDefault(g.Key, g.Key),
                Count: g.Count(),
                Samples: g.Take(4).Select(x => x.Value).ToList()))
            .ToList();
    }

    public async Task<List<object?[]>> IssuedReportsAsync(int limit)
    {
        using var conn = await factory.OpenAsync();
        var rows = await conn.QueryAsync<(string ReqId, string? Applicant, string PropertyType, string? Lender, DateTime? IssuedDate)>("""
            SELECT TOP (@limit) req_id AS ReqId, applicant AS Applicant, property_type AS PropertyType,
                   lender_name AS Lender, issued_date AS IssuedDate
            FROM leads
            WHERE stage = 'completed' AND deleted_at IS NULL
            ORDER BY issued_date DESC
            """, new { limit });
        return rows.Select(r => new object?[]
        {
            r.ReqId, r.Applicant ?? "", r.PropertyType, r.Lender ?? "",
            r.IssuedDate?.ToString("d MMM yyyy", Inv) ?? "",
        }).ToList();
    }

    public async Task<List<object?[]>> WeeklyLeadCountsAsync()
    {
        using var conn = await factory.OpenAsync();
        var rows = (await conn.QueryAsync<(string Day, int N)>("""
            SELECT DATENAME(weekday, created_at) AS Day, COUNT(*) AS N
            FROM leads
            WHERE deleted_at IS NULL AND created_at >= DATEADD(day, -7, SYSUTCDATETIME())
            GROUP BY DATENAME(weekday, created_at)
            """)).ToDictionary(r => r.Day, r => r.N, StringComparer.OrdinalIgnoreCase);

        string[] order = ["Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday"];
        return order.Select(d => new object?[] { d[..3], rows.GetValueOrDefault(d, 0) }).ToList();
    }

    public async Task<List<object?[]>> MisSnapshotAsync()
    {
        using var conn = await factory.OpenAsync();
        var counts = (await conn.QueryAsync<(string Stage, int N)>(
            "SELECT stage AS Stage, COUNT(*) AS N FROM leads WHERE deleted_at IS NULL GROUP BY stage"))
            .ToDictionary(r => r.Stage, r => r.N);
        int C(string s) => counts.GetValueOrDefault(s, 0);
        return
        [
            new object?[] { "Fresh leads", C("fresh").ToString() },
            new object?[] { "In QC", C("qc").ToString() },
            new object?[] { "Pricing", C("pricing").ToString() },
            new object?[] { "Completed", C("completed").ToString() },
        ];
    }

    private static string ToneForInvoice(string status) => status switch
    {
        "Paid" => "good",
        "Overdue" => "poor",
        _ => "amber",
    };

    private static string ToneForVisit(string status) => status switch
    {
        "Completed" or "Checked-in" => "good",
        "Cancelled" => "poor",
        _ => "amber",
    };
}
