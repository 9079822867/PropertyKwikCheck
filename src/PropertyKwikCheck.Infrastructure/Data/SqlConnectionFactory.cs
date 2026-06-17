using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;

namespace PropertyKwikCheck.Infrastructure.Data;

public interface IDbConnectionFactory
{
    Task<IDbConnection> OpenAsync();
}

/// <summary>Creates open SQL Server connections from the configured connection string.</summary>
public sealed class SqlConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    static SqlConnectionFactory()
    {
        // Map snake_case columns (req_id) to PascalCase properties (ReqId).
        DefaultTypeMap.MatchNamesWithUnderscores = true;
    }

    public SqlConnectionFactory(string connectionString) => _connectionString = connectionString;

    public async Task<IDbConnection> OpenAsync()
    {
        var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();
        return conn;
    }
}
