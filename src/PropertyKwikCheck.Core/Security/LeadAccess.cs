using PropertyKwikCheck.Core.Common;
using PropertyKwikCheck.Core.Domain;
using PropertyKwikCheck.Core.Rbac;

namespace PropertyKwikCheck.Core.Security;

/// <summary>Row-level visibility check for a single lead (spec §9.2).</summary>
public static class LeadAccess
{
    public static bool CanSee(Lead lead, CurrentUser user) => user.Scope switch
    {
        Scope.OwnLeads => lead.ValuatorUserId == user.Id,
        Scope.OwnCompany => lead.LenderCompanyId == user.CompanyId,
        _ => true,
    };

    /// <summary>Throws 404 (not 403) when out of scope, so existence isn't leaked.</summary>
    public static void EnsureVisible(Lead lead, CurrentUser user)
    {
        if (!CanSee(lead, user)) throw AppException.NotFound();
    }
}
