namespace RiskTrace.Domain.Request;

public sealed record RegisterRequest(
    string Email,
    string Password,
    string FullName);
