using System.Security.Claims;
using PropertyKwikCheck.Core.Security;

namespace PropertyKwikCheck.Api.Security;

public static class CurrentUserExtensions
{
    /// <summary>Builds the <see cref="CurrentUser"/> from the validated JWT claims, or null if unauthenticated.</summary>
    public static CurrentUser? GetCurrentUser(this ClaimsPrincipal principal)
    {
        if (principal.Identity?.IsAuthenticated != true) return null;

        var sub = principal.FindFirstValue("sub") ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!long.TryParse(sub, out var id)) return null;

        var name = principal.FindFirstValue("name") ?? "";
        var email = principal.FindFirstValue("email") ?? principal.FindFirstValue(ClaimTypes.Email) ?? "";
        int.TryParse(principal.FindFirstValue("roleId"), out var roleId);
        int.TryParse(principal.FindFirstValue("userTypeId"), out var userTypeId);
        long? companyId = long.TryParse(principal.FindFirstValue("companyId"), out var c) ? c : null;

        return new CurrentUser(id, name, email, roleId, userTypeId, companyId);
    }
}
