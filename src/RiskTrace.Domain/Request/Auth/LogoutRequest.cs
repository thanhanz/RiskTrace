namespace RiskTrace.Domain.Request;

public sealed record LogoutRequest(
    string RefreshToken,
    string? AccessToken
    );
