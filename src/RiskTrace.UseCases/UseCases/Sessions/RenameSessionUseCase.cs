using RiskTrace.Core.Abstractions;
using RiskTrace.Core.Common;
using RiskTrace.Domain.Constants;
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
    IUnitOfWork unitOfWork) : IRenameSessionUseCase
{
    public async Task<ApiResponse<SessionResponse>> ExecuteAsync(
        Guid sessionId,
        RenameSessionRequest request,
        CancellationToken cancellationToken = default)
    {
        if (currentUserProvider.UserId is not { } userId)
        {
            return ApiResponse<SessionResponse>.Failure(
                SessionErrorCodes.Unauthorized,
                "User is not authenticated.");
        }

        var title = request.Title?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(title))
        {
            return ApiResponse<SessionResponse>.Failure(
                SessionErrorCodes.InvalidTitle,
                "Title is required.");
        }

        var session = await reviewSessionRepository.GetActiveByIdAsync(sessionId, userId, cancellationToken);
        if (session is null)
        {
            return ApiResponse<SessionResponse>.Failure(
                SessionErrorCodes.SessionNotFound,
                "Session not found.");
        }

        session.Title = title;
        session.UpdatedAt = DateTime.UtcNow;

        await unitOfWork.SaveChangesAsync(cancellationToken);

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
