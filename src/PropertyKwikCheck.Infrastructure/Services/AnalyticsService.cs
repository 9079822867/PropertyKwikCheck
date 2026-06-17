using PropertyKwikCheck.Core.Abstractions;
using PropertyKwikCheck.Core.Mapping;
using PropertyKwikCheck.Core.Security;
using PropertyKwikCheck.Core.Workflow;

namespace PropertyKwikCheck.Infrastructure.Services;

/// <summary>
/// Builds the dashboard/analytics payload (spec §8.7). Counts, recent leads, valuer
/// productivity and site visits are DB-derived; trend/state/district series use
/// representative presentation data this phase (noted for later aggregation work).
/// All tuples preserve element order/length exactly — the frontend is positional.
/// </summary>
public sealed class AnalyticsService(
    ILeadRepository leads,
    IReportingRepository reporting) : IAnalyticsService
{
    private const string Blue = "#1F5FAE", Good = "#1E9D5B", Amber = "#C7890F", Navy = "#0C2742";

    private static object?[] R(params object?[] cells) => cells;

    public async Task<object> GetAnalyticsAsync(CurrentUser user)
    {
        var scope = LeadScope.From(user);
        var counts = await leads.CountsByStageAsync(scope);
        foreach (var s in Stage.All) counts.TryAdd(s, 0);

        var recentLeads = await leads.RecentAsync(7, scope);
        var productivity = await reporting.ValuerProductivityAsync();
        var siteVisits = await reporting.SiteVisitsForDashboardAsync();

        return new Dictionary<string, object?>
        {
            ["stats"] = new List<object?[]>
            {
                R("fresh", "Fresh Leads", counts[Stage.Fresh], "blue", "folderadd", "up", "+2 today"),
                R("ro", "RO Leads", counts[Stage.Ro], "blue", "user", "flat", "—"),
                R("assigned", "Assigned Leads", counts[Stage.Assigned], "blue", "reassign", "up", "+5 today"),
                R("ro_confirmation", "RO Confirmation", counts[Stage.RoConfirmation], "amber", "check", "flat", "—"),
                R("qc", "QC Stage", counts[Stage.Qc], "amber", "shield", "down", "-1 today"),
                R("pricing", "Pricing Stage", counts[Stage.Pricing], "amber", "rupee", "up", "+3 today"),
                R("completed", "Completed", counts[Stage.Completed], "good", "check", "up", "+12 today"),
                R("rejected", "Rejected", counts[Stage.Rejected], "poor", "reject", "flat", "—"),
                R(null, "Payment Requests", 18, "navy", "billing", "up", "+4 today"),
            },
            ["kpi"] = new List<object?[]>
            {
                R("Average TAT (Days)", "4.2", "blue", "clock"),
                R("SLA Met", "98.2%", "good", "shield"),
                R("Active Valuers", productivity.Count.ToString(), "blue", "user"),
                R("Overdue", counts[Stage.OutOfTat].ToString(), "poor", "alert"),
            },
            ["stageDefs"] = Stage.Order.Select(s => R(Label(s), counts[s], Blue)).ToList(),
            ["ptypeDonut"] = new List<object?[]>
            {
                R("Agricultural Land", 28, Blue),
                R("Residential", 41, Good),
                R("Commercial", 19, Amber),
                R("Plot / Land", 12, Navy),
            },
            ["casesOverview"] = new Dictionary<string, object?>
            {
                ["x"] = new[] { "12 May", "13 May", "14 May", "15 May", "16 May", "17 May", "18 May" },
                ["created"] = new[] { 12, 18, 9, 22, 15, 19, 14 },
                ["approved"] = new[] { 8, 11, 7, 16, 12, 13, 10 },
                ["issued"] = new[] { 6, 9, 5, 12, 10, 11, 8 },
            },
            ["pipeline"] = Stage.Order.Select(s => R(Label(s), counts[s], Blue)).ToList(),
            ["stateData"] = new List<object?[]>
            {
                R("Rajasthan", 512, 352, 856, 236, 178, 1245),
                R("Maharashtra", 488, 301, 742, 211, 156, 1102),
            },
            ["districtData"] = new Dictionary<string, object?>
            {
                ["Rajasthan"] = new List<object?[]> { R("Jaipur", 128, 86, 210, 63, 47, 332) },
                ["Maharashtra"] = new List<object?[]> { R("Mumbai", 142, 91, 188, 70, 52, 401) },
            },
            ["activities"] = recentLeads.Take(6).Select(l => R(
                "doc", $"Case {l.ReqId} {ReportStatusVerb(l.Stage)}", l.ValuatorName ?? "—", "recently")).ToList(),
            ["siteVisits"] = siteVisits,
            ["valuerProductivity"] = productivity.Select(p => R(p.Valuer, p.Count)).ToList(),
            ["tatTrend"] = new Dictionary<string, object?>
            {
                ["weeks"] = new[] { "W1", "W2", "W3", "W4", "W5", "W6" },
                ["pts"] = new[] { 5.1, 4.8, 4.6, 4.4, 4.3, 4.2 },
            },
            ["recent"] = recentLeads.Select(LeadMapper.ToDto).ToList(),
        };
    }

    private static string Label(string stage) => stage switch
    {
        Stage.Fresh => "Fresh Leads",
        Stage.Ro => "RO Leads",
        Stage.Assigned => "Assigned Leads",
        Stage.Reassigned => "Reassigned",
        Stage.RoConfirmation => "RO Confirmation",
        Stage.Qc => "QC Stage",
        Stage.QcHold => "QC Hold",
        Stage.Pricing => "Pricing Stage",
        Stage.Completed => "Completed",
        _ => stage,
    };

    private static string ReportStatusVerb(string stage) => stage switch
    {
        Stage.Qc => "submitted for review",
        Stage.Completed => "approved & issued",
        Stage.Pricing => "moved to pricing",
        Stage.Rejected => "rejected",
        _ => "updated",
    };
}
