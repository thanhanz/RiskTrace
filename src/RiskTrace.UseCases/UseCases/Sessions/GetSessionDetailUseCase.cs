using RiskTrace.Core.Common;
using RiskTrace.Domain.Entities;
using RiskTrace.Domain.Response;
using RiskTrace.UseCases.Interfaces.Sessions;
using RiskTrace.UseCases.Ports.Auth;
using RiskTrace.UseCases.Ports.Repositories;

namespace RiskTrace.UseCases.UseCases.Sessions;

public sealed class GetSessionDetailUseCase(
    ICurrentUserProvider currentUserProvider,
    IReviewSessionRepository reviewSessionRepository) : IGetSessionDetailUseCase
{
    public async Task<ApiResponse<SessionResponse>> ExecuteAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        if (currentUserProvider.UserId is not { } userId)
        {
            return ApiResponse<SessionResponse>.Failure(
                CommonErrors.Unauthorized("User is not authenticated."));
        }

        var session = await reviewSessionRepository.GetActiveByIdAsync(sessionId, userId, cancellationToken);

        if (session is null)
        {
            return ApiResponse<SessionResponse>.Failure(
                CommonErrors.NotFound("Session not found."));
        }

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
