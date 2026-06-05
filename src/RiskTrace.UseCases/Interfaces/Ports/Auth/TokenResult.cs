namespace RiskTrace.UseCases.Ports.Auth;

public sealed record TokenResult(string Token, DateTime ExpiresAt);
