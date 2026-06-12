using RiskTrace.Core.Abstractions;
using RiskTrace.Core.Common;
using RiskTrace.UseCases.Interfaces.Sessions;
using RiskTrace.UseCases.Ports.Auth;
using RiskTrace.UseCases.Ports.Repositories;

namespace RiskTrace.UseCases.UseCases.Sessions;

public sealed class DeleteSessionUseCase(
    ICurrentUserProvider currentUserProvider,
    IReviewSessionRepository reviewSessionRepository,
    IUnitOfWork unitOfWork) : IDeleteSessionUseCase
{
    public async Task<ApiResponse<object?>> ExecuteAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        if (currentUserProvider.UserId is not { } userId)
        {
            return ApiResponse<object?>.Failure(
                CommonErrors.Unauthorized("User is not authenticated."));
        }

        var session = await reviewSessionRepository.GetActiveByIdAsync(sessionId, userId, cancellationToken);
        if (session is null)
        {
            return ApiResponse<object?>.Failure(
                CommonErrors.NotFound("Session not found."));
        }

        session.IsActive = false;
        session.UpdatedAt = DateTime.UtcNow;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse<object?>.Success(null);
    }
}
