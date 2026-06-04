namespace RiskTrace.Domain.Request;

public sealed record LoginRequest(
    string Email,
    string Password);
