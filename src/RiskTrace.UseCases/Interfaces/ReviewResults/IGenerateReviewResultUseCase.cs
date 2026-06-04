namespace RiskTrace.UseCases.Interfaces.ReviewResults;

public interface IGenerateReviewResultUseCase
{
    Task ExecuteAsync(CancellationToken cancellationToken = default);
}
