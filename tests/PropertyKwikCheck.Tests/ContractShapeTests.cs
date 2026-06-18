using FluentAssertions;
using Moq;
using PropertyKwikCheck.Core.Abstractions;
using PropertyKwikCheck.Core.Common;
using PropertyKwikCheck.Core.Domain;
using PropertyKwikCheck.Infrastructure.Services;

namespace PropertyKwikCheck.Tests;

/// <summary>
/// The frontend reads analytics/screens tuples positionally, so element order and
/// length must stay exact (spec §8.7–8.8).
/// </summary>
public class ContractShapeTests
{
    private readonly Mock<ILeadRepository> _leads = new();
    private readonly Mock<IReportingRepository> _reporting = new();

    public ContractShapeTests()
    {
        _leads.Setup(r => r.CountsByStageAsync(It.IsAny<LeadScope>()))
            .ReturnsAsync(new Dictionary<string, int> { ["fresh"] = 6, ["qc"] = 3 });
        _leads.Setup(r => r.RecentAsync(It.IsAny<int>(), It.IsAny<LeadScope>()))
            .ReturnsAsync([TestData.Lead()]);
        _reporting.Setup(r => r.ValuerProductivityAsync()).ReturnsAsync([("Rahul Mehta", 48)]);
        _reporting.Setup(r => r.SiteVisitsForDashboardAsync()).ReturnsAsync([]);
        _reporting.Setup(r => r.YardScheduleAsync()).ReturnsAsync([]);
        _reporting.Setup(r => r.InvoicesAsync()).ReturnsAsync([]);
        _reporting.Setup(r => r.IssuedReportsAsync(It.IsAny<int>())).ReturnsAsync([]);
        _reporting.Setup(r => r.MasterCategoriesAsync()).ReturnsAsync([("Banks & Lenders", "banks", 10, new List<string> { "HDFC Bank Ltd" })]);
    }

    private static Dictionary<string, object?> AsDict(object o) => (Dictionary<string, object?>)o;

    [Fact]
    public async Task Analytics_payload_has_expected_keys_and_tuple_widths()
    {
        var svc = new AnalyticsService(_leads.Object, _reporting.Object);
        var payload = AsDict(await svc.GetAnalyticsAsync(TestData.SuperAdmin()));

        payload.Should().ContainKeys("stats", "kpi", "stageDefs", "ptypeDonut", "casesOverview",
            "pipeline", "stateData", "districtData", "activities", "siteVisits", "valuerProductivity",
            "tatTrend", "recent");

        var stats = (List<object?[]>)payload["stats"]!;
        stats.Should().OnlyContain(row => row.Length == 7);

        var kpi = (List<object?[]>)payload["kpi"]!;
        kpi.Should().OnlyContain(row => row.Length == 4);

        var stateData = (List<object?[]>)payload["stateData"]!;
        stateData.Should().OnlyContain(row => row.Length == 7);
    }

    [Fact]
    public async Task Billing_screen_has_stats_and_invoices()
    {
        var svc = new ScreenService(_reporting.Object);
        var payload = AsDict(await svc.GetScreenAsync("billing", TestData.SuperAdmin()));

        payload.Should().ContainKeys("stats", "invoices");
        ((List<object?[]>)payload["stats"]!).Should().OnlyContain(row => row.Length == 4);
    }

    [Fact]
    public async Task Master_screen_returns_tuple_list()
    {
        var svc = new ScreenService(_reporting.Object);
        var payload = await svc.GetScreenAsync("master", TestData.SuperAdmin());

        var rows = (List<object?[]>)payload;
        rows.Should().OnlyContain(row => row.Length == 5);
    }

    [Fact]
    public async Task Unknown_screen_throws_404()
    {
        var svc = new ScreenService(_reporting.Object);
        var act = () => svc.GetScreenAsync("nope", TestData.SuperAdmin());
        (await act.Should().ThrowAsync<AppException>()).Which.StatusCode.Should().Be(404);
    }
}
