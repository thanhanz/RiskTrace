namespace RiskTrace.Core.Common;

public sealed record ErrorResponse(
    int Code,
    string Message);
