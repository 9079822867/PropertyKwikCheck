using Dapper;
using PropertyKwikCheck.Core.Abstractions;
using PropertyKwikCheck.Core.Domain;

namespace PropertyKwikCheck.Infrastructure.Data;

public sealed class AuditRepository(IDbConnectionFactory factory) : IAuditRepository
{
    public async Task AddAsync(AuditEntry entry)
    {
        using var conn = await factory.OpenAsync();
        await conn.ExecuteAsync("""
            INSERT INTO audit_log (actor_user_id, action, entity_type, entity_id, before_json, after_json, ip, user_agent)
            VALUES (@ActorUserId, @Action, @EntityType, @EntityId, @BeforeJson, @AfterJson, @Ip, @UserAgent)
            """, entry);
    }
}
