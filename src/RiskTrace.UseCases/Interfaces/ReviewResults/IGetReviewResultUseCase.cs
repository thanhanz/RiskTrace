namespace RiskTrace.UseCases.Interfaces.ReviewResults;

public interface IGetReviewResultUseCase
{
    Task ExecuteAsync(CancellationToken cancellationToken = default);
}
