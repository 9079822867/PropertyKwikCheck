using PropertyKwikCheck.Core.Dtos;
using PropertyKwikCheck.Core.Security;

namespace PropertyKwikCheck.Core.Abstractions;

/// <summary>Context for an audited mutation (actor + request metadata).</summary>
public sealed record AuditContext(CurrentUser? Actor, string? Ip, string? UserAgent)
{
    public static readonly AuditContext System = new(null, null, null);
}

public interface ILeadService
{
    Task<LeadListResponse> ListAsync(LeadQuery query);
    Task<LeadDto> GetAsync(long id, CurrentUser user);
    Task<LeadDto> CreateAsync(CreateLeadRequest request, CurrentUser user, AuditContext audit);
    Task<LeadDto> UpdateAsync(long id, UpdateLeadRequest request, CurrentUser user, AuditContext audit);
    Task DeleteAsync(long id, CurrentUser user, AuditContext audit);
    Task<Dictionary<string, int>> BucketCountsAsync(CurrentUser user);
}

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request, string? ip, string? userAgent);
    Task<TokenPair> RefreshAsync(string refreshToken);
    Task LogoutAsync(string refreshToken);
    Task<UserDto> MeAsync(CurrentUser user);
}

public interface IDirectoryService
{
    Task<List<UserDto>> ListUsersAsync(CurrentUser user);
    Task<UserDto> CreateUserAsync(CreateUserRequest request, CurrentUser user, AuditContext audit);
    Task<UserDto> UpdateUserAsync(long id, UpdateUserRequest request, CurrentUser user, AuditContext audit);
    Task DeleteUserAsync(long id, CurrentUser user, AuditContext audit);

    Task<List<CompanyDto>> ListCompaniesAsync(CurrentUser user);
    Task<CompanyDto> CreateCompanyAsync(CreateCompanyRequest request, CurrentUser user, AuditContext audit);
    Task<CompanyDto> UpdateCompanyAsync(long id, UpdateCompanyRequest request, CurrentUser user, AuditContext audit);
    Task DeleteCompanyAsync(long id, CurrentUser user, AuditContext audit);
}

public interface IAnalyticsService
{
    Task<object> GetAnalyticsAsync(CurrentUser user);
}

public interface IScreenService
{
    Task<object> GetScreenAsync(string name, CurrentUser user);
}
