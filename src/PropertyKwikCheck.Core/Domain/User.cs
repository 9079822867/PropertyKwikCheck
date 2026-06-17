namespace PropertyKwikCheck.Core.Domain;

/// <summary>An application user. Identity is modelled by the user's Roles + UserTypes tables.</summary>
public sealed class User
{
    public long Id { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public int RoleId { get; set; }
    public int UserTypeId { get; set; }
    public long? CompanyId { get; set; }
    public string? Phone { get; set; }
    public string? LicenceNo { get; set; }
    public string Status { get; set; } = "Active";
    public DateTime? LastLoginAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    // Joined display labels (populated by repository joins, not stored on this row).
    public string? RoleName { get; set; }
    public string? UserTypeName { get; set; }
    public string? CompanyName { get; set; }
}

public sealed class Role
{
    public int Id { get; set; }
    public string RoleName { get; set; } = "";
    public string? Remark { get; set; }
}

public sealed class UserType
{
    public int Id { get; set; }
    public int CompanyTypeId { get; set; }
    public string Name { get; set; } = "";
}
