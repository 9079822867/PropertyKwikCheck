using System.Text.Json.Serialization;

namespace PropertyKwikCheck.Core.Dtos;

/// <summary>User directory shape (spec §8.6). <c>leads</c> is a display string.</summary>
public sealed class UserDto
{
    [JsonPropertyName("id")] public long Id { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; } = "";
    [JsonPropertyName("email")] public string Email { get; set; } = "";
    [JsonPropertyName("role")] public string Role { get; set; } = "";
    [JsonPropertyName("userType")] public string UserType { get; set; } = "";
    [JsonPropertyName("company")] public string? Company { get; set; }
    [JsonPropertyName("phone")] public string? Phone { get; set; }
    [JsonPropertyName("licenceNo")] public string? LicenceNo { get; set; }
    [JsonPropertyName("status")] public string Status { get; set; } = "Active";
    [JsonPropertyName("leads")] public string Leads { get; set; } = "—";
}

public sealed class CreateUserRequest
{
    [JsonPropertyName("name")] public string Name { get; set; } = "";
    [JsonPropertyName("email")] public string Email { get; set; } = "";
    [JsonPropertyName("roleId")] public int RoleId { get; set; }
    [JsonPropertyName("userTypeId")] public int UserTypeId { get; set; }
    [JsonPropertyName("companyId")] public long? CompanyId { get; set; }
    [JsonPropertyName("phone")] public string? Phone { get; set; }
    [JsonPropertyName("licenceNo")] public string? LicenceNo { get; set; }
    [JsonPropertyName("status")] public string? Status { get; set; }
    [JsonPropertyName("password")] public string? Password { get; set; }
}

public sealed class UpdateUserRequest
{
    [JsonPropertyName("name")] public string? Name { get; set; }
    [JsonPropertyName("roleId")] public int? RoleId { get; set; }
    [JsonPropertyName("userTypeId")] public int? UserTypeId { get; set; }
    [JsonPropertyName("companyId")] public long? CompanyId { get; set; }
    [JsonPropertyName("phone")] public string? Phone { get; set; }
    [JsonPropertyName("licenceNo")] public string? LicenceNo { get; set; }
    [JsonPropertyName("status")] public string? Status { get; set; }
    [JsonPropertyName("password")] public string? Password { get; set; }
}

/// <summary>Company directory shape (spec §8.6).</summary>
public sealed class CompanyDto
{
    [JsonPropertyName("id")] public long Id { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; } = "";
    [JsonPropertyName("type")] public string Type { get; set; } = "";
    [JsonPropertyName("spoc")] public string? Spoc { get; set; }
    [JsonPropertyName("leads")] public object Leads { get; set; } = "—";
    [JsonPropertyName("active")] public object Active { get; set; } = "—";
    [JsonPropertyName("status")] public string Status { get; set; } = "Active";
}

public sealed class CreateCompanyRequest
{
    [JsonPropertyName("name")] public string Name { get; set; } = "";
    [JsonPropertyName("type")] public string Type { get; set; } = "";
    [JsonPropertyName("spoc")] public string? Spoc { get; set; }
    [JsonPropertyName("status")] public string? Status { get; set; }
}

public sealed class UpdateCompanyRequest
{
    [JsonPropertyName("name")] public string? Name { get; set; }
    [JsonPropertyName("type")] public string? Type { get; set; }
    [JsonPropertyName("spoc")] public string? Spoc { get; set; }
    [JsonPropertyName("status")] public string? Status { get; set; }
}
