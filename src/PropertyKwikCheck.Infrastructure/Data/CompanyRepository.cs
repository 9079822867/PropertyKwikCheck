using Dapper;
using PropertyKwikCheck.Core.Abstractions;
using PropertyKwikCheck.Core.Domain;

namespace PropertyKwikCheck.Infrastructure.Data;

public sealed class CompanyRepository(IDbConnectionFactory factory) : ICompanyRepository
{
    private const string SelectWithCounts = """
        SELECT c.id, c.name, c.type, c.spoc_name, c.spoc_user_id, c.status, c.created_at, c.updated_at,
               (SELECT COUNT(*) FROM leads l WHERE l.lender_company_id = c.id AND l.deleted_at IS NULL) AS lead_count,
               (SELECT COUNT(*) FROM leads l WHERE l.lender_company_id = c.id AND l.deleted_at IS NULL
                  AND l.stage NOT IN ('completed','rejected','duplicate')) AS active_lead_count
        FROM companies c
        """;

    public async Task<Company?> GetByIdAsync(long id)
    {
        using var conn = await factory.OpenAsync();
        return await conn.QuerySingleOrDefaultAsync<Company>($"{SelectWithCounts} WHERE c.id = @id", new { id });
    }

    public async Task<List<Company>> ListAsync(long? onlyId)
    {
        using var conn = await factory.OpenAsync();
        var where = onlyId is null ? "" : "WHERE c.id = @onlyId";
        return (await conn.QueryAsync<Company>($"{SelectWithCounts} {where} ORDER BY c.name", new { onlyId })).ToList();
    }

    public async Task<long> InsertAsync(Company company)
    {
        using var conn = await factory.OpenAsync();
        const string sql = """
            INSERT INTO companies (name, type, spoc_name, spoc_user_id, status)
            OUTPUT INSERTED.id
            VALUES (@Name, @Type, @SpocName, @SpocUserId, @Status);
            """;
        return await conn.ExecuteScalarAsync<long>(sql, company);
    }

    public async Task UpdateAsync(Company company)
    {
        using var conn = await factory.OpenAsync();
        const string sql = """
            UPDATE companies SET name=@Name, type=@Type, spoc_name=@SpocName, spoc_user_id=@SpocUserId,
              status=@Status, updated_at=SYSUTCDATETIME()
            WHERE id=@Id;
            """;
        await conn.ExecuteAsync(sql, company);
    }

    public async Task DeleteAsync(long id)
    {
        using var conn = await factory.OpenAsync();
        // Soft-disable rather than hard-delete (companies are referenced by leads).
        await conn.ExecuteAsync("UPDATE companies SET status='Inactive', updated_at=SYSUTCDATETIME() WHERE id=@id", new { id });
    }
}
