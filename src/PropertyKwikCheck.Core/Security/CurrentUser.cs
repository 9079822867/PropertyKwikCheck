using PropertyKwikCheck.Core.Rbac;

namespace PropertyKwikCheck.Core.Security;

/// <summary>
/// The authenticated principal resolved from the JWT, carried into services so they
/// can enforce capability + scope checks (spec §9) without hitting the request pipeline.
/// </summary>
public sealed record CurrentUser(
    long Id,
    string Name,
    string Email,
    int RoleId,
    int UserTypeId,
    long? CompanyId)
{
    public RbacPolicy.Policy Policy => RbacPolicy.For(UserTypeId);

    public bool Can(Capability cap) => Policy.Can(cap);

    public Scope Scope => Policy.Scope;

    /// <summary>Throws 403 when the user lacks <paramref name="cap"/>.</summary>
    public void Require(Capability cap)
    {
        if (!Can(cap))
            throw Common.AppException.Forbidden($"Missing capability: {cap}");
    }
}
