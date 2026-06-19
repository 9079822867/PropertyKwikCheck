using Dapper;
using PropertyKwikCheck.Core.Abstractions;
using PropertyKwikCheck.Core.Domain;

namespace PropertyKwikCheck.Infrastructure.Data;

public sealed class UserRepository(IDbConnectionFactory factory) : IUserRepository
{
    private const string SelectJoined = """
        SELECT u.id, u.name, u.email, u.password_hash, u.role_id, u.user_type_id, u.company_id,
               u.phone, u.licence_no, u.status, u.last_login_at, u.created_at, u.updated_at, u.deleted_at,
               r.role_name AS role_name, t.name AS user_type_name, c.name AS company_name
        FROM users u
        LEFT JOIN Roles r ON r.id = u.role_id
        LEFT JOIN UserTypes t ON t.id = u.user_type_id
        LEFT JOIN companies c ON c.id = u.company_id
        """;

    public async Task<User?> GetByIdAsync(long id)
    {
        using var conn = await factory.OpenAsync();
        return await conn.QuerySingleOrDefaultAsync<User>(
            $"{SelectJoined} WHERE u.id = @id AND u.deleted_at IS NULL", new { id });
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        using var conn = await factory.OpenAsync();
        return await conn.QuerySingleOrDefaultAsync<User>(
            $"{SelectJoined} WHERE u.email = @email AND u.deleted_at IS NULL", new { email });
    }

    public async Task<List<User>> ListAsync(long? companyId)
    {
        using var conn = await factory.OpenAsync();
        var where = "WHERE u.deleted_at IS NULL" + (companyId is null ? "" : " AND u.company_id = @companyId");
        return (await conn.QueryAsync<User>($"{SelectJoined} {where} ORDER BY u.name", new { companyId })).ToList();
    }

    public async Task<long> InsertAsync(User user)
    {
        using var conn = await factory.OpenAsync();
        const string sql = """
            INSERT INTO users (name, email, password_hash, role_id, user_type_id, company_id, phone, licence_no, status)
            OUTPUT INSERTED.id
            VALUES (@Name, @Email, @PasswordHash, @RoleId, @UserTypeId, @CompanyId, @Phone, @LicenceNo, @Status);
            """;
        return await conn.ExecuteScalarAsync<long>(sql, user);
    }

    public async Task UpdateAsync(User user)
    {
        using var conn = await factory.OpenAsync();
        const string sql = """
            UPDATE users SET
              name=@Name, email=@Email, password_hash=@PasswordHash, role_id=@RoleId, user_type_id=@UserTypeId,
              company_id=@CompanyId, phone=@Phone, licence_no=@LicenceNo, status=@Status, updated_at=SYSUTCDATETIME()
            WHERE id=@Id;
            """;
        await conn.ExecuteAsync(sql, user);
    }

    public async Task SoftDeleteAsync(long id)
    {
        using var conn = await factory.OpenAsync();
        await conn.ExecuteAsync(
            "UPDATE users SET deleted_at=SYSUTCDATETIME(), status='Inactive' WHERE id=@id", new { id });
    }

    public async Task TouchLastLoginAsync(long id, DateTime at)
    {
        using var conn = await factory.OpenAsync();
        await conn.ExecuteAsync("UPDATE users SET last_login_at=@at WHERE id=@id", new { id, at });
    }

    public async Task<List<Role>> RolesAsync()
    {
        using var conn = await factory.OpenAsync();
        return (await conn.QueryAsync<Role>("SELECT id, role_name, remark FROM Roles ORDER BY id")).ToList();
    }

    public async Task<List<UserType>> UserTypesAsync()
    {
        using var conn = await factory.OpenAsync();
        return (await conn.QueryAsync<UserType>("SELECT id, company_type_id, name FROM UserTypes ORDER BY id")).ToList();
    }

    public async Task<List<User>> ValuatorsAsync(long? companyId)
    {
        using var conn = await factory.OpenAsync();
        // Field valuers = RO Valuators (8) + CANDO VALUATOR (17); active only.
        var where = "WHERE u.deleted_at IS NULL AND u.status = 'Active' AND u.user_type_id IN (8, 17)"
                  + (companyId is null ? "" : " AND u.company_id = @companyId");
        return (await conn.QueryAsync<User>($"{SelectJoined} {where} ORDER BY u.name", new { companyId })).ToList();
    }
}
