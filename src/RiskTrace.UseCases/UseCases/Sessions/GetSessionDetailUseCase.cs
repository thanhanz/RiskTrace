using RiskTrace.Core.Common;
using RiskTrace.Core.Interfaces.Logger;
using RiskTrace.Domain.Entities;
using RiskTrace.Domain.Response;
using RiskTrace.UseCases.Interfaces.Sessions;
using RiskTrace.UseCases.Ports.Auth;
using RiskTrace.UseCases.Ports.Repositories;

namespace RiskTrace.UseCases.UseCases.Sessions;

public sealed class GetSessionDetailUseCase(
    ICurrentUserProvider currentUserProvider,
    IReviewSessionRepository reviewSessionRepository,
    ILogger<GetSessionDetailUseCase> logger) : IGetSessionDetailUseCase
{
    public async Task<ApiResponse<SessionResponse>> ExecuteAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Handling get session detail request for session {SessionId}.", sessionId);

        if (currentUserProvider.UserId is not { } userId)
        {
            logger.LogWarning("Get session detail failed for session {SessionId}: user is not authenticated.", sessionId);
            return ApiResponse<SessionResponse>.Failure(
                CommonErrors.Unauthorized("User is not authenticated."));
        }

        var session = await reviewSessionRepository.GetActiveByIdAsync(sessionId, userId, cancellationToken);

        if (session is null)
        {
            logger.LogWarning(
                "Get session detail failed for session {SessionId} and user {UserId}: session not found.",
                sessionId,
                userId);
            return ApiResponse<SessionResponse>.Failure(
                CommonErrors.NotFound("Session not found."));
        }

        logger.LogInformation("Get session detail succeeded for session {SessionId} and user {UserId}.", sessionId, userId);

        return ApiResponse<SessionResponse>.Success(ToResponse(session));
    }

    private static SessionResponse ToResponse(ReviewSession session)
    {
        return new SessionResponse(
            Id: session.Id,
            Title: session.Title,
            Status: session.Status,
            CreatedAt: session.CreatedAt,
            UpdatedAt: session.UpdatedAt);
    }
}
