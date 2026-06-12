using RiskTrace.Domain.Entities;

namespace RiskTrace.UseCases.Ports.Repositories;

public interface IReviewSessionRepository
{
    Task AddAsync(ReviewSession session, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ReviewSession>> GetActiveByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<ReviewSession?> GetActiveByIdAsync(
        Guid sessionId,
        Guid userId,
        CancellationToken cancellationToken = default);
}
