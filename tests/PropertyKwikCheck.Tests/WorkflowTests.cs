using FluentAssertions;
using PropertyKwikCheck.Core.Workflow;

namespace PropertyKwikCheck.Tests;

public class StageMachineTests
{
    [Theory]
    [InlineData("fresh", "assigned")]
    [InlineData("fresh", "ro")]
    [InlineData("assigned", "reassigned")]
    [InlineData("assigned", "ro_confirmation")]
    [InlineData("ro_confirmation", "qc")]
    [InlineData("qc", "qc_hold")]
    [InlineData("qc", "pricing")]
    [InlineData("qc_hold", "qc")]
    [InlineData("pricing", "completed")]
    public void Allows_legal_transitions(string from, string to)
        => StageMachine.CanTransition(from, to).Should().BeTrue();

    [Theory]
    [InlineData("fresh", "completed")]
    [InlineData("fresh", "qc")]
    [InlineData("assigned", "pricing")]
    [InlineData("completed", "qc")]
    [InlineData("rejected", "assigned")]
    [InlineData("duplicate", "fresh")]
    public void Rejects_illegal_transitions(string from, string to)
        => StageMachine.CanTransition(from, to).Should().BeFalse();

    [Fact]
    public void Same_stage_is_always_allowed()
        => StageMachine.CanTransition("qc", "qc").Should().BeTrue();

    [Theory]
    [InlineData("assigned")]
    [InlineData("qc")]
    [InlineData("pricing")]
    public void Active_leads_can_breach_to_out_of_tat(string from)
        => StageMachine.CanTransition(from, "out_of_tat").Should().BeTrue();

    [Theory]
    [InlineData("completed")]
    [InlineData("rejected")]
    [InlineData("duplicate")]
    public void Terminal_leads_cannot_breach_to_out_of_tat(string from)
        => StageMachine.CanTransition(from, "out_of_tat").Should().BeFalse();
}

public class ReqIdGeneratorTests
{
    [Theory]
    [InlineData("Residential", 4812, "KC-RESI-2026-04812")]
    [InlineData("Commercial", 1, "KC-COMM-2026-00001")]
    [InlineData("Industrial", 99999, "KC-IND-2026-99999")]
    [InlineData("Plot", 4913, "KC-PLOT-2026-04913")]
    [InlineData("Agricultural Land", 4927, "KC-AGRI-2026-04927")]
    public void Generates_canonical_req_id(string ptype, long id, string expected)
        => ReqIdGenerator.Generate(ptype, id).Should().Be(expected);

    [Theory]
    [InlineData("Residential", "property")]
    [InlineData("Plot", "plot")]
    [InlineData("Agricultural Land", "agri")]
    public void Resolves_family(string ptype, string family)
        => ReqIdGenerator.FamilyFor(ptype).Should().Be(family);

    [Fact]
    public void Throws_for_unknown_ptype()
        => FluentActions.Invoking(() => ReqIdGenerator.Generate("Spaceship", 1))
            .Should().Throw<ArgumentException>();
}

public class TatCalculatorTests
{
    [Theory]
    [InlineData(0, "ok")]
    [InlineData(92, "ok")]
    [InlineData(92.5, "warn")]
    [InlineData(100, "warn")]
    [InlineData(100.1, "over")]
    [InlineData(150, "over")]
    public void Maps_state_thresholds(double pct, string expected)
        => TatCalculator.StateFor(pct).Should().Be(expected);

    [Fact]
    public void Percent_is_zero_when_window_missing()
        => TatCalculator.PercentElapsed(null, null, DateTime.UtcNow).Should().Be(0);

    [Fact]
    public void Percent_is_50_at_window_midpoint()
    {
        var start = new DateTime(2026, 5, 1);
        var due = new DateTime(2026, 5, 11);
        TatCalculator.PercentElapsed(start, due, new DateTime(2026, 5, 6)).Should().Be(50);
    }
}
