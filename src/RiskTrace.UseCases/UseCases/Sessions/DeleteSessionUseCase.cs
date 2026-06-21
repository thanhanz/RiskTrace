using RiskTrace.Core.Abstractions;
using RiskTrace.Core.Common;
using RiskTrace.Core.Interfaces.Logger;
using RiskTrace.UseCases.Interfaces.Sessions;
using RiskTrace.UseCases.Ports.Auth;
using RiskTrace.UseCases.Ports.Repositories;

namespace RiskTrace.UseCases.UseCases.Sessions;

public sealed class DeleteSessionUseCase(
    ICurrentUserProvider currentUserProvider,
    IReviewSessionRepository reviewSessionRepository,
    IUnitOfWork unitOfWork,
    ILogger<DeleteSessionUseCase> logger) : IDeleteSessionUseCase
{
    public async Task<ApiResponse<object?>> ExecuteAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Handling delete session request for session {SessionId}.", sessionId);

        if (currentUserProvider.UserId is not { } userId)
        {
            logger.LogWarning("Delete session failed for session {SessionId}: user is not authenticated.", sessionId);
            return ApiResponse<object?>.Failure(
                CommonErrors.Unauthorized("User is not authenticated."));
        }

        var session = await reviewSessionRepository.GetActiveByIdAsync(sessionId, userId, cancellationToken);
        if (session is null)
        {
            logger.LogWarning(
                "Delete session failed for session {SessionId} and user {UserId}: session not found.",
                sessionId,
                userId);
            return ApiResponse<object?>.Failure(
                CommonErrors.NotFound("Session not found."));
        }

        session.IsActive = false;
        session.UpdatedAt = DateTime.UtcNow;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Delete session succeeded for session {SessionId} and user {UserId}.", sessionId, userId);

        return ApiResponse<object?>.Success(null);
    }
}
