using System.Text.Json.Nodes;
using FluentAssertions;
using Moq;
using PropertyKwikCheck.Core.Abstractions;
using PropertyKwikCheck.Core.Common;
using PropertyKwikCheck.Core.Domain;
using PropertyKwikCheck.Core.Dtos;
using PropertyKwikCheck.Infrastructure.Services;

namespace PropertyKwikCheck.Tests;

public class LeadServiceTests
{
    private readonly Mock<ILeadRepository> _leads = new();
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IAuditRepository> _audit = new();
    private readonly LeadService _service;

    public LeadServiceTests()
    {
        _service = new LeadService(_leads.Object, _users.Object, _audit.Object, new FixedClock(new DateTime(2026, 6, 1)));
    }

    private static JsonObject Json(string s) => JsonNode.Parse(s)!.AsObject();

    [Fact]
    public async Task Create_sets_fresh_defaults_and_generates_req_id()
    {
        _leads.Setup(r => r.InsertAsync(It.IsAny<Lead>())).ReturnsAsync(100L);
        var request = new CreateLeadRequest { Ptype = "Residential", Data = Json("""{"applicant":"Mr. Test","lender":"HDFC Bank Ltd"}""") };

        var dto = await _service.CreateAsync(request, TestData.SuperAdmin(), AuditContext.System);

        dto.Stage.Should().Be("fresh");
        dto.ReportStatus.Should().Be("Open");
        dto.ReqId.Should().Be("KC-RESI-2026-00100");
        dto.Value.Should().BeNull();
        dto.Applicant.Should().Be("Mr. Test");
        _leads.Verify(r => r.UpdateAsync(It.IsAny<Lead>()), Times.Once);
        _leads.Verify(r => r.AddStageHistoryAsync(It.IsAny<LeadStageHistory>()), Times.Once);
        _audit.Verify(r => r.AddAsync(It.IsAny<AuditEntry>()), Times.Once);
    }

    [Fact]
    public async Task Create_is_forbidden_without_capability()
    {
        var request = new CreateLeadRequest { Ptype = "Residential" };
        var act = () => _service.CreateAsync(request, TestData.Valuer(), AuditContext.System);
        (await act.Should().ThrowAsync<AppException>()).Which.StatusCode.Should().Be(403);
    }

    [Fact]
    public async Task EditSave_deep_merges_report_data()
    {
        Lead? captured = null;
        _leads.Setup(r => r.GetByIdAsync(100)).ReturnsAsync(TestData.Lead());
        _leads.Setup(r => r.UpdateAsync(It.IsAny<Lead>())).Callback<Lead>(l => captured = l).Returns(Task.CompletedTask);

        var request = new UpdateLeadRequest { Data = Json("""{"b":"9","c":"3"}""") };
        await _service.UpdateAsync(100, request, TestData.SuperAdmin(), AuditContext.System);

        var data = JsonNode.Parse(captured!.ReportData!)!.AsObject();
        data["a"]!.GetValue<string>().Should().Be("1");
        data["b"]!.GetValue<string>().Should().Be("9");
        data["c"]!.GetValue<string>().Should().Be("3");
    }

    [Fact]
    public async Task EditSave_syncs_value_from_fair_market_value()
    {
        Lead? captured = null;
        _leads.Setup(r => r.GetByIdAsync(100)).ReturnsAsync(TestData.Lead());
        _leads.Setup(r => r.UpdateAsync(It.IsAny<Lead>())).Callback<Lead>(l => captured = l).Returns(Task.CompletedTask);

        var request = new UpdateLeadRequest { Data = Json("""{"fairMarketValue":"21540000"}""") };
        await _service.UpdateAsync(100, request, TestData.SuperAdmin(), AuditContext.System);

        captured!.Value.Should().Be(21540000);
    }

    [Fact]
    public async Task Illegal_stage_transition_throws_409()
    {
        _leads.Setup(r => r.GetByIdAsync(100)).ReturnsAsync(TestData.Lead(stage: "fresh"));
        var request = new UpdateLeadRequest { Stage = "completed" };

        var act = () => _service.UpdateAsync(100, request, TestData.SuperAdmin(), AuditContext.System);
        (await act.Should().ThrowAsync<AppException>()).Which.StatusCode.Should().Be(409);
    }

    [Fact]
    public async Task Reassign_from_assigned_moves_to_reassigned()
    {
        Lead? captured = null;
        _leads.Setup(r => r.GetByIdAsync(100)).ReturnsAsync(TestData.Lead(stage: "assigned"));
        _leads.Setup(r => r.UpdateAsync(It.IsAny<Lead>())).Callback<Lead>(l => captured = l).Returns(Task.CompletedTask);

        var request = new UpdateLeadRequest { Action = "reassign", Valuator = "Ajay Malviya" };
        await _service.UpdateAsync(100, request, TestData.SuperAdmin(), AuditContext.System);

        captured!.Stage.Should().Be("reassigned");
        captured.ValuatorName.Should().Be("Ajay Malviya");
    }

    [Fact]
    public async Task Reject_sets_rejected_stage()
    {
        Lead? captured = null;
        _leads.Setup(r => r.GetByIdAsync(100)).ReturnsAsync(TestData.Lead(stage: "fresh"));
        _leads.Setup(r => r.UpdateAsync(It.IsAny<Lead>())).Callback<Lead>(l => captured = l).Returns(Task.CompletedTask);

        var request = new UpdateLeadRequest { Action = "reject" };
        await _service.UpdateAsync(100, request, TestData.SuperAdmin(), AuditContext.System);

        captured!.Stage.Should().Be("rejected");
        captured.ReportStatus.Should().Be("Rejected");
    }

    [Fact]
    public async Task Valuer_sees_only_own_leads()
    {
        _leads.Setup(r => r.GetByIdAsync(100)).ReturnsAsync(TestData.Lead()); // valuator_user_id = 6

        var own = await _service.GetAsync(100, TestData.Valuer(id: 6));
        own.Id.Should().Be(100);

        var act = () => _service.GetAsync(100, TestData.Valuer(id: 7));
        (await act.Should().ThrowAsync<AppException>()).Which.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task BucketCounts_include_all_twelve_buckets()
    {
        _leads.Setup(r => r.CountsByStageAsync(It.IsAny<LeadScope>()))
            .ReturnsAsync(new Dictionary<string, int> { ["qc"] = 3 });

        var counts = await _service.BucketCountsAsync(TestData.SuperAdmin());

        counts.Should().HaveCount(12);
        counts["qc"].Should().Be(3);
        counts["fresh"].Should().Be(0);
    }
}
