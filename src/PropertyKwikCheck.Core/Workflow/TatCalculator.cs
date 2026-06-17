namespace PropertyKwikCheck.Core.Workflow;

public static class TatState
{
    public const string Ok = "ok";
    public const string Warn = "warn";
    public const string Over = "over";
}

/// <summary>
/// Turnaround-time helpers (spec §12). <c>tat_pct</c> is the percent of the
/// (assignedOn → tatDue) window elapsed; the state thresholds are
/// ok ≤ 92 &lt; warn ≤ 100 &lt; over.
/// </summary>
public static class TatCalculator
{
    public static string StateFor(double tatPct) => tatPct switch
    {
        <= 92 => TatState.Ok,
        <= 100 => TatState.Warn,
        _ => TatState.Over,
    };

    /// <summary>
    /// Percent of the window elapsed at <paramref name="now"/>. Returns 0 when the
    /// window is missing or not yet started, and may exceed 100 once breached.
    /// </summary>
    public static double PercentElapsed(DateTime? assignedOn, DateTime? tatDue, DateTime now)
    {
        if (assignedOn is null || tatDue is null) return 0;
        var total = (tatDue.Value - assignedOn.Value).TotalSeconds;
        if (total <= 0) return 0;
        var elapsed = (now - assignedOn.Value).TotalSeconds;
        if (elapsed <= 0) return 0;
        return Math.Round(elapsed / total * 100, 2);
    }
}
