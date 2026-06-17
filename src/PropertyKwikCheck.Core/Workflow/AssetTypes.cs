namespace PropertyKwikCheck.Core.Workflow;

/// <summary>Asset family values (spec §1 / §16.2).</summary>
public static class AssetFamily
{
    public const string Property = "property";
    public const string Plot = "plot";
    public const string Agri = "agri";

    public static readonly string[] All = [Property, Plot, Agri];

    public static bool IsValid(string? family) => family is not null && Array.IndexOf(All, family) >= 0;
}

/// <summary>
/// Property sub-type (<c>ptype</c>) metadata: maps each ptype to its asset family
/// and reqId prefix (spec §16.2).
/// </summary>
public static class PropertyTypes
{
    public sealed record Meta(string Ptype, string Family, string Prefix, string ReportLabel);

    private static readonly Meta[] Defs =
    [
        new("Residential", AssetFamily.Property, "KC-RESI", "Property Inspection"),
        new("Commercial", AssetFamily.Property, "KC-COMM", "Property Inspection"),
        new("Industrial", AssetFamily.Property, "KC-IND", "Property Inspection"),
        new("Plot", AssetFamily.Plot, "KC-PLOT", "Plot Valuation"),
        new("Plot / Land", AssetFamily.Plot, "KC-PLOT", "Plot Valuation"),
        new("Agricultural Land", AssetFamily.Agri, "KC-AGRI", "Agri Land Valuation"),
    ];

    public static readonly string[] AllPtypes =
        ["Residential", "Commercial", "Industrial", "Plot", "Agricultural Land"];

    public static Meta? Resolve(string? ptype) =>
        ptype is null ? null : Defs.FirstOrDefault(d => string.Equals(d.Ptype, ptype, StringComparison.OrdinalIgnoreCase));

    public static bool IsValid(string? ptype) => Resolve(ptype) is not null;
}
