using RiskTrace.Core.Common;
using RiskTrace.Domain.Constants;
using RiskTrace.Domain.Entities;
using RiskTrace.Domain.Response;
using RiskTrace.UseCases.Interfaces.Sessions;
using RiskTrace.UseCases.Ports.Auth;
using RiskTrace.UseCases.Ports.Repositories;

namespace RiskTrace.UseCases.UseCases.Sessions;

public sealed class GetUserSessionsUseCase(
    ICurrentUserProvider currentUserProvider,
    IReviewSessionRepository reviewSessionRepository) : IGetUserSessionsUseCase
{
    public async Task<ApiResponse<IReadOnlyList<SessionResponse>>> ExecuteAsync(
        CancellationToken cancellationToken = default)
    {
        if (currentUserProvider.UserId is not { } userId)
        {
            return ApiResponse<IReadOnlyList<SessionResponse>>.Failure(
                SessionErrorCodes.Unauthorized,
                "User is not authenticated.");
        }

        var sessions = await reviewSessionRepository.GetActiveByUserIdAsync(userId, cancellationToken);
        var response = sessions
            .Select(ToResponse)
            .ToList();

        return ApiResponse<IReadOnlyList<SessionResponse>>.Success(response);
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
