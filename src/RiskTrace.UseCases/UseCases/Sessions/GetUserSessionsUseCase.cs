using RiskTrace.Core.Common;
using RiskTrace.Core.Interfaces.Logger;
using RiskTrace.Domain.Entities;
using RiskTrace.Domain.Response;
using RiskTrace.UseCases.Interfaces.Sessions;
using RiskTrace.UseCases.Ports.Auth;
using RiskTrace.UseCases.Ports.Repositories;

namespace RiskTrace.UseCases.UseCases.Sessions;

public sealed class GetUserSessionsUseCase(
    ICurrentUserProvider currentUserProvider,
    IReviewSessionRepository reviewSessionRepository,
    ILogger<GetUserSessionsUseCase> logger) : IGetUserSessionsUseCase
{
    public async Task<ApiResponse<IReadOnlyList<SessionResponse>>> ExecuteAsync(
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Handling get user sessions request.");

        if (currentUserProvider.UserId is not { } userId)
        {
            logger.LogWarning("Get user sessions failed: user is not authenticated.");
            return ApiResponse<IReadOnlyList<SessionResponse>>.Failure(
                CommonErrors.Unauthorized("User is not authenticated."));
        }

        var sessions = await reviewSessionRepository.GetActiveByUserIdAsync(userId, cancellationToken);
        var response = sessions
            .Select(ToResponse)
            .ToList();

        logger.LogInformation("Get user sessions succeeded for user {UserId}: returned {SessionCount} sessions.", userId, response.Count);

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
