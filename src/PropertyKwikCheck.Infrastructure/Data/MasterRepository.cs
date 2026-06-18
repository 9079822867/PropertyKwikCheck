using Dapper;
using PropertyKwikCheck.Core.Abstractions;

namespace PropertyKwikCheck.Infrastructure.Data;

public sealed class MasterRepository(IDbConnectionFactory factory) : IMasterRepository
{
    public async Task<List<(long Id, string Value)>> ItemsAsync(string category)
    {
        using var conn = await factory.OpenAsync();
        var rows = await conn.QueryAsync<(long Id, string Value)>(
            "SELECT id AS Id, value AS Value FROM master_lookups WHERE category = @category AND active = 1 ORDER BY sort, value",
            new { category });
        return rows.ToList();
    }

    public async Task<long> AddAsync(string category, string value)
    {
        using var conn = await factory.OpenAsync();
        return await conn.ExecuteScalarAsync<long>("""
            INSERT INTO master_lookups (category, value, sort, active)
            OUTPUT INSERTED.id
            VALUES (@category, @value, 0, 1)
            """, new { category, value });
    }

    public async Task DeleteAsync(long id)
    {
        using var conn = await factory.OpenAsync();
        await conn.ExecuteAsync("UPDATE master_lookups SET active = 0 WHERE id = @id", new { id });
    }
}
