using PropertyKwikCheck.Core.Domain;

namespace PropertyKwikCheck.Core.Abstractions;

/// <summary>Abstracts the wall clock so time-dependent logic (TAT, tokens) is testable.</summary>
public interface IClock
{
    DateTime UtcNow { get; }
}

public sealed class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}

public interface IJwtTokenService
{
    /// <summary>Issues a short-lived access token carrying sub/roleId/userTypeId/companyId claims.</summary>
    string CreateAccessToken(User user);

    /// <summary>Generates an opaque refresh token (the plaintext returned to the client).</summary>
    string CreateRefreshToken();

    /// <summary>Hashes a refresh token for at-rest storage/lookup.</summary>
    string HashRefreshToken(string token);

    TimeSpan RefreshTokenLifetime { get; }
}
