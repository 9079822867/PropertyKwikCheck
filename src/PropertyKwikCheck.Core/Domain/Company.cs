namespace PropertyKwikCheck.Core.Domain;

/// <summary>A lender (bank/NBFC) or the owning valuation agency (spec §6.2 companies).</summary>
public sealed class Company
{
    public long Id { get; set; }
    public string Name { get; set; } = "";
    public string Type { get; set; } = "";
    public string? SpocName { get; set; }
    public long? SpocUserId { get; set; }
    public string Status { get; set; } = "Active";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Derived counts for the directory view.
    public int? LeadCount { get; set; }
    public int? ActiveLeadCount { get; set; }
}
