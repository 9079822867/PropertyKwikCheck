using PropertyKwikCheck.Core.Abstractions;
using PropertyKwikCheck.Core.Common;
using PropertyKwikCheck.Core.Domain;
using PropertyKwikCheck.Core.Dtos;
using PropertyKwikCheck.Core.Mapping;
using PropertyKwikCheck.Core.Security;

namespace PropertyKwikCheck.Infrastructure.Services;

public sealed class AuthService(
    IUserRepository users,
    IRefreshTokenRepository refreshTokens,
    IPasswordHasher hasher,
    IJwtTokenService jwt,
    IAuditRepository audit,
    IClock clock) : IAuthService
{
    public async Task<LoginResponse> LoginAsync(LoginRequest request, string? ip, string? userAgent)
    {
        var user = await users.GetByEmailAsync(request.Email);
        if (user is null || user.Status != "Active" || !hasher.Verify(request.Password, user.PasswordHash))
            throw AppException.Unauthorized();

        var pair = await IssuePairAsync(user);
        await users.TouchLastLoginAsync(user.Id, clock.UtcNow);
        await audit.AddAsync(new AuditEntry
        {
            ActorUserId = user.Id,
            Action = "auth.login",
            EntityType = "user",
            EntityId = user.Id.ToString(),
            Ip = ip,
            UserAgent = userAgent,
        });

        return new LoginResponse
        {
            Token = pair.Token,
            RefreshToken = pair.RefreshToken,
            User = DirectoryMapper.ToDto(user),
        };
    }

    public async Task<TokenPair> RefreshAsync(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken)) throw AppException.Unauthorized("Invalid token");

        var hash = jwt.HashRefreshToken(refreshToken);
        var record = await refreshTokens.FindActiveByHashAsync(hash);
        if (record is null || record.ExpiresAt <= clock.UtcNow || record.RevokedAt is not null)
            throw AppException.Unauthorized("Invalid token");

        var user = await users.GetByIdAsync(record.UserId) ?? throw AppException.Unauthorized("Invalid token");

        // Rotate: revoke the presented token, issue a fresh pair.
        await refreshTokens.RevokeAsync(record.Id, clock.UtcNow);
        return await IssuePairAsync(user);
    }

    public async Task LogoutAsync(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken)) return;
        var hash = jwt.HashRefreshToken(refreshToken);
        var record = await refreshTokens.FindActiveByHashAsync(hash);
        if (record is not null) await refreshTokens.RevokeAsync(record.Id, clock.UtcNow);
    }

    public async Task<UserDto> MeAsync(CurrentUser user)
    {
        var full = await users.GetByIdAsync(user.Id) ?? throw AppException.Unauthorized("Invalid token");
        return DirectoryMapper.ToDto(full);
    }

    private async Task<TokenPair> IssuePairAsync(User user)
    {
        var access = jwt.CreateAccessToken(user);
        var refresh = jwt.CreateRefreshToken();
        await refreshTokens.StoreAsync(new RefreshTokenRecord
        {
            UserId = user.Id,
            TokenHash = jwt.HashRefreshToken(refresh),
            ExpiresAt = clock.UtcNow.Add(jwt.RefreshTokenLifetime),
        });
        return new TokenPair { Token = access, RefreshToken = refresh };
    }
}
