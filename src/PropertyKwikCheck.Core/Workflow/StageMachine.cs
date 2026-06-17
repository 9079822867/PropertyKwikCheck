namespace PropertyKwikCheck.Core.Workflow;

/// <summary>
/// Enforces the legal lead state transitions (spec §4). Illegal jumps must be
/// rejected by the service layer with 409 Conflict.
/// </summary>
public static class StageMachine
{
    // from-stage -> set of legal to-stages (excluding the TAT-breach edge, handled separately).
    private static readonly Dictionary<string, HashSet<string>> Transitions = new()
    {
        [Stage.Fresh] = [Stage.Ro, Stage.Assigned, Stage.Rejected, Stage.Duplicate],
        [Stage.Ro] = [Stage.Assigned, Stage.Rejected, Stage.Duplicate],
        [Stage.Assigned] = [Stage.Reassigned, Stage.RoConfirmation, Stage.Rejected, Stage.Duplicate],
        [Stage.Reassigned] = [Stage.RoConfirmation, Stage.Rejected, Stage.Duplicate],
        [Stage.RoConfirmation] = [Stage.Qc, Stage.Rejected, Stage.Duplicate],
        [Stage.Qc] = [Stage.QcHold, Stage.Pricing, Stage.Rejected],
        [Stage.QcHold] = [Stage.Qc, Stage.Rejected],
        [Stage.Pricing] = [Stage.Completed, Stage.Qc],
        [Stage.Completed] = [],
        [Stage.OutOfTat] = [Stage.Assigned, Stage.Reassigned, Stage.RoConfirmation, Stage.Qc, Stage.Rejected, Stage.Duplicate],
        [Stage.Duplicate] = [],
        [Stage.Rejected] = [],
    };

    /// <summary>
    /// True when moving from <paramref name="from"/> to <paramref name="to"/> is allowed.
    /// A no-op (same stage) is always allowed. A TAT breach into <see cref="Stage.OutOfTat"/>
    /// is allowed from any non-terminal stage.
    /// </summary>
    public static bool CanTransition(string from, string to)
    {
        if (!Stage.IsValid(from) || !Stage.IsValid(to)) return false;
        if (from == to) return true;
        if (to == Stage.OutOfTat) return !Stage.IsTerminal(from);
        return Transitions.TryGetValue(from, out var allowed) && allowed.Contains(to);
    }

    public static IReadOnlyCollection<string> AllowedFrom(string from) =>
        Transitions.TryGetValue(from, out var allowed) ? allowed : [];
}
