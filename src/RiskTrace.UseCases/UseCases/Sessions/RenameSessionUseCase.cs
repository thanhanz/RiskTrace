using RiskTrace.Core.Abstractions;
using RiskTrace.Core.Common;
using RiskTrace.Core.Interfaces.Logger;
using RiskTrace.Domain.Entities;
using RiskTrace.Domain.Request;
using RiskTrace.Domain.Response;
using RiskTrace.UseCases.Interfaces.Sessions;
using RiskTrace.UseCases.Ports.Auth;
using RiskTrace.UseCases.Ports.Repositories;

namespace RiskTrace.UseCases.UseCases.Sessions;

public sealed class RenameSessionUseCase(
    ICurrentUserProvider currentUserProvider,
    IReviewSessionRepository reviewSessionRepository,
    IUnitOfWork unitOfWork,
    ILogger<RenameSessionUseCase> logger) : IRenameSessionUseCase
{
    public async Task<ApiResponse<SessionResponse>> ExecuteAsync(
        Guid sessionId,
        RenameSessionRequest request,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Handling rename session request for session {SessionId} with title {Title}.",
            sessionId,
            request.Title ?? string.Empty);

        if (currentUserProvider.UserId is not { } userId)
        {
            logger.LogWarning("Rename session failed for session {SessionId}: user is not authenticated.", sessionId);
            return ApiResponse<SessionResponse>.Failure(
                CommonErrors.Unauthorized("User is not authenticated."));
        }

        var title = request.Title?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(title))
        {
            logger.LogWarning("Rename session failed for session {SessionId}: title is required.", sessionId);
            return ApiResponse<SessionResponse>.Failure(
                CommonErrors.FieldRequired("Title"));
        }

        var session = await reviewSessionRepository.GetActiveByIdAsync(sessionId, userId, cancellationToken);
        if (session is null)
        {
            logger.LogWarning(
                "Rename session failed for session {SessionId} and user {UserId}: session not found.",
                sessionId,
                userId);
            return ApiResponse<SessionResponse>.Failure(
                CommonErrors.NotFound("Session not found."));
        }

        session.Title = title;
        session.UpdatedAt = DateTime.UtcNow;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Rename session succeeded for session {SessionId} and user {UserId}.", sessionId, userId);

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
