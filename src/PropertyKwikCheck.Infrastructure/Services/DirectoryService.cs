using PropertyKwikCheck.Core.Abstractions;
using PropertyKwikCheck.Core.Common;
using PropertyKwikCheck.Core.Domain;
using PropertyKwikCheck.Core.Dtos;
using PropertyKwikCheck.Core.Mapping;
using PropertyKwikCheck.Core.Rbac;
using PropertyKwikCheck.Core.Security;

namespace PropertyKwikCheck.Infrastructure.Services;

public sealed class DirectoryService(
    IUserRepository users,
    ICompanyRepository companies,
    IPasswordHasher hasher,
    IAuditRepository audit) : IDirectoryService
{
    // ---- users ---------------------------------------------------------------

    public async Task<List<UserDto>> ListUsersAsync(CurrentUser user)
    {
        if (!user.Can(Capability.ManageUsers) && user.Scope != Scope.OwnCompany)
            throw AppException.Forbidden();

        var companyFilter = user.Scope == Scope.OwnCompany ? user.CompanyId : null;
        var rows = await users.ListAsync(companyFilter);
        return rows.Select(u => DirectoryMapper.ToDto(u)).ToList();
    }

    public async Task<UserDto> CreateUserAsync(CreateUserRequest request, CurrentUser user, AuditContext auditCtx)
    {
        user.Require(Capability.ManageUsers);
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Name))
            throw AppException.Validation("name and email are required");
        if (await users.GetByEmailAsync(request.Email) is not null)
            throw AppException.Conflict("A user with this email already exists", "EMAIL_TAKEN");

        var entity = new User
        {
            Name = request.Name,
            Email = request.Email,
            RoleId = request.RoleId,
            UserTypeId = request.UserTypeId,
            CompanyId = request.CompanyId,
            Phone = request.Phone,
            LicenceNo = request.LicenceNo,
            Status = request.Status ?? "Active",
            PasswordHash = hasher.Hash(string.IsNullOrEmpty(request.Password)
                ? Guid.NewGuid().ToString("N")   // placeholder until invite/set-password flow
                : request.Password),
        };

        var id = await users.InsertAsync(entity);
        await audit.AddAsync(Audit(auditCtx, "user.create", id));
        var created = await users.GetByIdAsync(id);
        return DirectoryMapper.ToDto(created!);
    }

    public async Task<UserDto> UpdateUserAsync(long id, UpdateUserRequest request, CurrentUser user, AuditContext auditCtx)
    {
        user.Require(Capability.ManageUsers);
        var entity = await users.GetByIdAsync(id) ?? throw AppException.NotFound("User not found");

        if (request.Name is not null) entity.Name = request.Name;
        if (request.RoleId is not null) entity.RoleId = request.RoleId.Value;
        if (request.UserTypeId is not null) entity.UserTypeId = request.UserTypeId.Value;
        if (request.CompanyId is not null) entity.CompanyId = request.CompanyId;
        if (request.Phone is not null) entity.Phone = request.Phone;
        if (request.LicenceNo is not null) entity.LicenceNo = request.LicenceNo;
        if (request.Status is not null) entity.Status = request.Status;
        if (!string.IsNullOrEmpty(request.Password)) entity.PasswordHash = hasher.Hash(request.Password);

        await users.UpdateAsync(entity);
        await audit.AddAsync(Audit(auditCtx, "user.update", id));
        var updated = await users.GetByIdAsync(id);
        return DirectoryMapper.ToDto(updated!);
    }

    public async Task DeleteUserAsync(long id, CurrentUser user, AuditContext auditCtx)
    {
        user.Require(Capability.ManageUsers);
        _ = await users.GetByIdAsync(id) ?? throw AppException.NotFound("User not found");
        await users.SoftDeleteAsync(id);
        await audit.AddAsync(Audit(auditCtx, "user.delete", id));
    }

    // ---- companies -----------------------------------------------------------

    public async Task<List<CompanyDto>> ListCompaniesAsync(CurrentUser user)
    {
        if (!user.Can(Capability.ManageCompanies) && user.Scope != Scope.OwnCompany)
            throw AppException.Forbidden();

        var onlyId = user.Scope == Scope.OwnCompany ? user.CompanyId : null;
        var rows = await companies.ListAsync(onlyId);
        return rows.Select(DirectoryMapper.ToDto).ToList();
    }

    public async Task<CompanyDto> CreateCompanyAsync(CreateCompanyRequest request, CurrentUser user, AuditContext auditCtx)
    {
        user.Require(Capability.ManageCompanies);
        if (string.IsNullOrWhiteSpace(request.Name)) throw AppException.Validation("name is required");

        var entity = new Company
        {
            Name = request.Name,
            Type = request.Type,
            SpocName = request.Spoc,
            Status = request.Status ?? "Active",
        };
        var id = await companies.InsertAsync(entity);
        await audit.AddAsync(Audit(auditCtx, "company.create", id, "company"));
        var created = await companies.GetByIdAsync(id);
        return DirectoryMapper.ToDto(created!);
    }

    public async Task<CompanyDto> UpdateCompanyAsync(long id, UpdateCompanyRequest request, CurrentUser user, AuditContext auditCtx)
    {
        user.Require(Capability.ManageCompanies);
        var entity = await companies.GetByIdAsync(id) ?? throw AppException.NotFound("Company not found");

        if (request.Name is not null) entity.Name = request.Name;
        if (request.Type is not null) entity.Type = request.Type;
        if (request.Spoc is not null) entity.SpocName = request.Spoc;
        if (request.Status is not null) entity.Status = request.Status;

        await companies.UpdateAsync(entity);
        await audit.AddAsync(Audit(auditCtx, "company.update", id, "company"));
        var updated = await companies.GetByIdAsync(id);
        return DirectoryMapper.ToDto(updated!);
    }

    public async Task DeleteCompanyAsync(long id, CurrentUser user, AuditContext auditCtx)
    {
        user.Require(Capability.ManageCompanies);
        _ = await companies.GetByIdAsync(id) ?? throw AppException.NotFound("Company not found");
        await companies.DeleteAsync(id);
        await audit.AddAsync(Audit(auditCtx, "company.delete", id, "company"));
    }

    private static AuditEntry Audit(AuditContext ctx, string action, long id, string entity = "user") => new()
    {
        ActorUserId = ctx.Actor?.Id,
        Action = action,
        EntityType = entity,
        EntityId = id.ToString(),
        Ip = ctx.Ip,
        UserAgent = ctx.UserAgent,
    };
}
