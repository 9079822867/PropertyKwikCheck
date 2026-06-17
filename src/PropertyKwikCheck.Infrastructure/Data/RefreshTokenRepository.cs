using Dapper;
using PropertyKwikCheck.Core.Abstractions;
using PropertyKwikCheck.Core.Domain;

namespace PropertyKwikCheck.Infrastructure.Data;

public sealed class RefreshTokenRepository(IDbConnectionFactory factory) : IRefreshTokenRepository
{
    public async Task StoreAsync(RefreshTokenRecord token)
    {
        using var conn = await factory.OpenAsync();
        await conn.ExecuteAsync("""
            INSERT INTO refresh_tokens (user_id, token_hash, expires_at)
            VALUES (@UserId, @TokenHash, @ExpiresAt)
            """, token);
    }

    public async Task<RefreshTokenRecord?> FindActiveByHashAsync(string tokenHash)
    {
        using var conn = await factory.OpenAsync();
        return await conn.QuerySingleOrDefaultAsync<RefreshTokenRecord>("""
            SELECT id, user_id, token_hash, expires_at, revoked_at, created_at
            FROM refresh_tokens
            WHERE token_hash = @tokenHash AND revoked_at IS NULL
            """, new { tokenHash });
    }

    public async Task RevokeAsync(long id, DateTime at)
    {
        using var conn = await factory.OpenAsync();
        await conn.ExecuteAsync("UPDATE refresh_tokens SET revoked_at=@at WHERE id=@id", new { id, at });
    }

    public async Task RevokeAllForUserAsync(long userId, DateTime at)
    {
        using var conn = await factory.OpenAsync();
        await conn.ExecuteAsync(
            "UPDATE refresh_tokens SET revoked_at=@at WHERE user_id=@userId AND revoked_at IS NULL",
            new { userId, at });
    }
}
