using PropertyKwikCheck.Core.Domain;

namespace PropertyKwikCheck.Core.Abstractions;

public interface ILeadRepository
{
    Task<Lead?> GetByIdAsync(long id);
    Task<(List<Lead> Rows, int Total)> ListAsync(LeadQuery query);
    Task<Dictionary<string, int>> CountsByStageAsync(LeadScope scope);
    Task<long> InsertAsync(Lead lead);
    Task UpdateAsync(Lead lead);
    Task SoftDeleteAsync(long id);
    Task AddStageHistoryAsync(LeadStageHistory history);
    Task<List<Lead>> RecentAsync(int count, LeadScope scope);

    /// <summary>Recomputes tat_pct/tat_state for all active (non-terminal) leads at <paramref name="now"/>. Returns rows updated (spec §12).</summary>
    Task<int> RecomputeTatAsync(DateTime now);
}

public interface IUserRepository
{
    Task<User?> GetByIdAsync(long id);
    Task<User?> GetByEmailAsync(string email);
    Task<List<User>> ListAsync(long? companyId);
    Task<long> InsertAsync(User user);
    Task UpdateAsync(User user);
    Task SoftDeleteAsync(long id);
    Task TouchLastLoginAsync(long id, DateTime at);
    Task<List<Role>> RolesAsync();
    Task<List<UserType>> UserTypesAsync();
}

public interface ICompanyRepository
{
    Task<Company?> GetByIdAsync(long id);
    Task<List<Company>> ListAsync(long? onlyId);
    Task<long> InsertAsync(Company company);
    Task UpdateAsync(Company company);
    Task DeleteAsync(long id);
}

public interface IRefreshTokenRepository
{
    Task StoreAsync(RefreshTokenRecord token);
    Task<RefreshTokenRecord?> FindActiveByHashAsync(string tokenHash);
    Task RevokeAsync(long id, DateTime at);
    Task RevokeAllForUserAsync(long userId, DateTime at);
}

public interface IAuditRepository
{
    Task AddAsync(AuditEntry entry);
}

/// <summary>Read-only aggregates backing the analytics + per-screen datasets (spec §8.7–8.8).</summary>
public interface IReportingRepository
{
    Task<List<(string Valuer, int Count)>> ValuerProductivityAsync();
    Task<List<object?[]>> InvoicesAsync();

    /// <summary>Dashboard tuple: [day, MONTH, reqId, assetType, location, time] (spec §8.7).</summary>
    Task<List<object?[]>> SiteVisitsForDashboardAsync();

    /// <summary>Yard tuple: [time, valuer, reqId, assetType, location, statusLabel, tone] (spec §8.8).</summary>
    Task<List<object?[]>> YardScheduleAsync();

    Task<List<(string Category, int Count, List<string> Samples)>> MasterCategoriesAsync();
    Task<List<object?[]>> IssuedReportsAsync(int limit);

    /// <summary>MIS weekly leads created, [dayLabel, count] Mon→Sun (spec §8.8).</summary>
    Task<List<object?[]>> WeeklyLeadCountsAsync();

    /// <summary>MIS snapshot tuples [label, value] from live stage counts (spec §8.8).</summary>
    Task<List<object?[]>> MisSnapshotAsync();
}
