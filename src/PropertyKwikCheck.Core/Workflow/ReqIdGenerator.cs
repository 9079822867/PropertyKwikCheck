namespace PropertyKwikCheck.Core.Workflow;

/// <summary>
/// Builds a human request id from a ptype and numeric lead id, in the spec's
/// canonical format <c>{PREFIX}-2026-{id:05d}</c> (spec §16.2).
/// </summary>
public static class ReqIdGenerator
{
    public const string Year = "2026";

    /// <summary>Resolve the asset family for a ptype, or null if the ptype is unknown.</summary>
    public static string? FamilyFor(string ptype) => PropertyTypes.Resolve(ptype)?.Family;

    /// <summary>
    /// Generate the reqId for a lead. Throws <see cref="ArgumentException"/> for an unknown ptype.
    /// </summary>
    public static string Generate(string ptype, long id)
    {
        var meta = PropertyTypes.Resolve(ptype)
            ?? throw new ArgumentException($"Unknown ptype '{ptype}'", nameof(ptype));
        return $"{meta.Prefix}-{Year}-{id:D5}";
    }
}
