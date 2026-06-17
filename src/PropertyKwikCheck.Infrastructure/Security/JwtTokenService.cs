using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using PropertyKwikCheck.Core.Abstractions;
using PropertyKwikCheck.Core.Domain;

namespace PropertyKwikCheck.Infrastructure.Security;

/// <summary>
/// Issues short-lived JWT access tokens (claims: sub, roleId, userTypeId, companyId)
/// and opaque rotating refresh tokens stored hashed at rest (spec §8.9 / §9).
/// </summary>
public sealed class JwtTokenService(JwtOptions options, IClock clock) : IJwtTokenService
{
    private readonly JwtOptions _opts = options;

    public TimeSpan RefreshTokenLifetime => TimeSpan.FromDays(_opts.RefreshTokenDays);

    public string CreateAccessToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opts.SigningKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new("name", user.Name),
            new("roleId", user.RoleId.ToString()),
            new("userTypeId", user.UserTypeId.ToString()),
        };
        if (user.CompanyId is not null)
            claims.Add(new Claim("companyId", user.CompanyId.Value.ToString()));

        var now = clock.UtcNow;
        var token = new JwtSecurityToken(
            issuer: _opts.Issuer,
            audience: _opts.Audience,
            claims: claims,
            notBefore: now,
            expires: now.AddMinutes(_opts.AccessTokenMinutes),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string CreateRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes);
    }

    public string HashRefreshToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes);
    }
}
