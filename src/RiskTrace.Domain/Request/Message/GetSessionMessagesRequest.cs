namespace RiskTrace.Domain.Request;

public sealed record GetSessionMessagesRequest(
    int PageSize = 20,
    string? NextCursor = null);
