using System.IdentityModel.Tokens.Jwt;
using FluentAssertions;
using Moq;
using PropertyKwikCheck.Core.Abstractions;
using PropertyKwikCheck.Core.Common;
using PropertyKwikCheck.Core.Domain;
using PropertyKwikCheck.Core.Dtos;
using PropertyKwikCheck.Infrastructure.Security;
using PropertyKwikCheck.Infrastructure.Services;

namespace PropertyKwikCheck.Tests;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IRefreshTokenRepository> _tokens = new();
    private readonly Mock<IAuditRepository> _audit = new();
    private readonly PasswordHasher _hasher = new();
    private readonly JwtTokenService _jwt;
    private readonly AuthService _service;

    public AuthServiceTests()
    {
        var opts = new JwtOptions { SigningKey = "test-signing-key-at-least-32-characters-long-000", AccessTokenMinutes = 15, RefreshTokenDays = 14 };
        var clock = new FixedClock(new DateTime(2026, 6, 1));
        _jwt = new JwtTokenService(opts, clock);
        _service = new AuthService(_users.Object, _tokens.Object, _hasher, _jwt, _audit.Object, clock);
    }

    private User SeedUser(string status = "Active") => new()
    {
        Id = 1, Name = "Super Admin", Email = "sa@kc.in", RoleId = 3, UserTypeId = 19,
        PasswordHash = _hasher.Hash("Password@123"), Status = status, RoleName = "Internal", UserTypeName = "Super Admin",
    };

    [Fact]
    public async Task Login_succeeds_with_valid_credentials()
    {
        _users.Setup(r => r.GetByEmailAsync("sa@kc.in")).ReturnsAsync(SeedUser());

        var result = await _service.LoginAsync(new LoginRequest { Email = "sa@kc.in", Password = "Password@123" }, "127.0.0.1", "test");

        result.Token.Should().NotBeNullOrWhiteSpace();
        result.RefreshToken.Should().NotBeNullOrWhiteSpace();
        result.User.Email.Should().Be("sa@kc.in");
        _tokens.Verify(r => r.StoreAsync(It.IsAny<RefreshTokenRecord>()), Times.Once);
        _users.Verify(r => r.TouchLastLoginAsync(1, It.IsAny<DateTime>()), Times.Once);
    }

    [Fact]
    public async Task Login_fails_with_wrong_password()
    {
        _users.Setup(r => r.GetByEmailAsync("sa@kc.in")).ReturnsAsync(SeedUser());
        var act = () => _service.LoginAsync(new LoginRequest { Email = "sa@kc.in", Password = "wrong" }, null, null);
        (await act.Should().ThrowAsync<AppException>()).Which.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task Login_fails_for_inactive_user()
    {
        _users.Setup(r => r.GetByEmailAsync("sa@kc.in")).ReturnsAsync(SeedUser(status: "Inactive"));
        var act = () => _service.LoginAsync(new LoginRequest { Email = "sa@kc.in", Password = "Password@123" }, null, null);
        (await act.Should().ThrowAsync<AppException>()).Which.StatusCode.Should().Be(401);
    }

    [Fact]
    public void Access_token_carries_identity_claims()
    {
        var token = _jwt.CreateAccessToken(SeedUser());
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        jwt.Claims.Should().Contain(c => c.Type == "sub" && c.Value == "1");
        jwt.Claims.Should().Contain(c => c.Type == "roleId" && c.Value == "3");
        jwt.Claims.Should().Contain(c => c.Type == "userTypeId" && c.Value == "19");
    }

    [Fact]
    public void Password_hash_round_trips()
    {
        var hash = _hasher.Hash("secret");
        _hasher.Verify("secret", hash).Should().BeTrue();
        _hasher.Verify("nope", hash).Should().BeFalse();
    }
}
