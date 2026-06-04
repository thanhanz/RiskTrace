namespace RiskTrace.Domain.Response;

public sealed record AuthResponse(
    string AccessToken,
    string RefreshToken,
    UserInfoResponse User);
