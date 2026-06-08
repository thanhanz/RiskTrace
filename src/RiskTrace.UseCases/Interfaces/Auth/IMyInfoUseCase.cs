using RiskTrace.Domain.Response;

namespace RiskTrace.UseCases.Interfaces.Auth;

public interface IMyInfoUseCase
{
    Task<MyInfoResult> ExecuteAsync(CancellationToken cancellationToken = default);
}
