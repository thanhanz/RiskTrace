namespace RiskTrace.Domain.Response;

public sealed record MyInfoResult(
    bool IsAuthenticated,
    UserInfoResponse? User);
