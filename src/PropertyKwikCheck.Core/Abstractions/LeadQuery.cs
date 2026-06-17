using PropertyKwikCheck.Core.Rbac;
using PropertyKwikCheck.Core.Security;

namespace PropertyKwikCheck.Core.Abstractions;

/// <summary>Row-visibility filter derived from the current user (spec §9.2).</summary>
public sealed record LeadScope(Scope Mode, long? ValuatorUserId, long? CompanyId)
{
    public static readonly LeadScope Unrestricted = new(Scope.All, null, null);

    public static LeadScope From(CurrentUser user) => user.Scope switch
    {
        Scope.OwnLeads => new LeadScope(Scope.OwnLeads, user.Id, null),
        Scope.OwnCompany => new LeadScope(Scope.OwnCompany, null, user.CompanyId),
        _ => Unrestricted,
    };
}

/// <summary>Parameters for <c>GET /api/leads</c> (spec §8.1).</summary>
public sealed record LeadQuery(
    string Bucket,
    string? Q,
    int Page,
    int PageSize,
    string? Sort,
    LeadScope Scope)
{
    public const int DefaultPageSize = 50;
    public const int MaxPageSize = 200;
}
