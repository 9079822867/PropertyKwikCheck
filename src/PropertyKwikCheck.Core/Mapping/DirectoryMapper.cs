using System.Globalization;
using PropertyKwikCheck.Core.Domain;
using PropertyKwikCheck.Core.Dtos;

namespace PropertyKwikCheck.Core.Mapping;

public static class DirectoryMapper
{
    public static UserDto ToDto(User u, int? leadCount = null) => new()
    {
        Id = u.Id,
        Name = u.Name,
        Email = u.Email,
        Role = u.RoleName ?? "",
        UserType = u.UserTypeName ?? "",
        Company = u.CompanyName,
        Phone = u.Phone,
        LicenceNo = u.LicenceNo,
        Status = u.Status,
        Leads = leadCount?.ToString(CultureInfo.InvariantCulture) ?? "—",
    };

    public static CompanyDto ToDto(Company c) => new()
    {
        Id = c.Id,
        Name = c.Name,
        Type = c.Type,
        Spoc = c.SpocName,
        Leads = (object?)c.LeadCount ?? "—",
        Active = (object?)c.ActiveLeadCount ?? "—",
        Status = c.Status,
    };
}
