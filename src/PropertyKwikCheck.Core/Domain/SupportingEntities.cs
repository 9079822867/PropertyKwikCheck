namespace PropertyKwikCheck.Core.Domain;

/// <summary>Audit of every stage move (spec §6.2 lead_stage_history).</summary>
public sealed class LeadStageHistory
{
    public long Id { get; set; }
    public long LeadId { get; set; }
    public string? FromStage { get; set; }
    public string ToStage { get; set; } = "";
    public long? ActorUserId { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>Append-only audit record (spec §13).</summary>
public sealed class AuditEntry
{
    public long Id { get; set; }
    public long? ActorUserId { get; set; }
    public string Action { get; set; } = "";
    public string EntityType { get; set; } = "";
    public string? EntityId { get; set; }
    public string? BeforeJson { get; set; }
    public string? AfterJson { get; set; }
    public string? Ip { get; set; }
    public string? UserAgent { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>Hashed rotating refresh token (spec §6.2 refresh_tokens / §8.9).</summary>
public sealed class RefreshTokenRecord
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public string TokenHash { get; set; } = "";
    public DateTime ExpiresAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
