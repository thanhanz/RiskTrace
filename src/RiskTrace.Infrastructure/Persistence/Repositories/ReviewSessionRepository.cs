using Microsoft.EntityFrameworkCore;
using RiskTrace.Domain.Entities;
using RiskTrace.UseCases.Ports.Repositories;

namespace RiskTrace.Infrastructure.Persistence.Repositories;

public sealed class ReviewSessionRepository(AppDbContext dbContext) : IReviewSessionRepository
{
    public Task AddAsync(ReviewSession session, CancellationToken cancellationToken = default)
    {
        return dbContext.ReviewSessions.AddAsync(session, cancellationToken).AsTask();
    }

    public async Task<IReadOnlyList<ReviewSession>> GetActiveByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.ReviewSessions
            .Where(session => session.UserId == userId && session.IsActive)
            .OrderByDescending(session => session.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public Task<ReviewSession?> GetActiveByIdAsync(
        Guid sessionId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return dbContext.ReviewSessions
            .FirstOrDefaultAsync(
                session => session.Id == sessionId
                    && session.UserId == userId
                    && session.IsActive,
                cancellationToken);
    }
}
