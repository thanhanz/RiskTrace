using RiskTrace.Domain.Events;

namespace RiskTrace.UseCases.Interfaces.ReviewResults;

public interface IHandleAiResponseUseCase
{
    Task ExecuteAsync(
        TemporaryAiResponseEvent response,
        CancellationToken cancellationToken = default);
}
