using System.Text.Json.Serialization;

namespace PropertyKwikCheck.Core.Dtos;

public sealed class LoginRequest
{
    [JsonPropertyName("email")] public string Email { get; set; } = "";
    [JsonPropertyName("password")] public string Password { get; set; } = "";
}

public sealed class RefreshRequest
{
    [JsonPropertyName("refreshToken")] public string RefreshToken { get; set; } = "";
}

public sealed class LoginResponse
{
    [JsonPropertyName("token")] public string Token { get; set; } = "";
    [JsonPropertyName("refreshToken")] public string RefreshToken { get; set; } = "";
    [JsonPropertyName("user")] public UserDto User { get; set; } = new();
}

public sealed class TokenPair
{
    [JsonPropertyName("token")] public string Token { get; set; } = "";
    [JsonPropertyName("refreshToken")] public string RefreshToken { get; set; } = "";
}
