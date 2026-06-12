using RiskTrace.Core.Abstractions;
using RiskTrace.Core.Common;
using RiskTrace.Domain.Constants;
using RiskTrace.Domain.Entities;
using RiskTrace.Domain.Enums;
using RiskTrace.Domain.Request;
using RiskTrace.Domain.Response;
using RiskTrace.UseCases.Interfaces.Sessions;
using RiskTrace.UseCases.Ports.Auth;
using RiskTrace.UseCases.Ports.Repositories;

namespace RiskTrace.UseCases.UseCases.Sessions;

public sealed class CreateSessionUseCase(
    ICurrentUserProvider currentUserProvider,
    IReviewSessionRepository reviewSessionRepository,
    IUnitOfWork unitOfWork) : ICreateSessionUseCase
{
    public async Task<ApiResponse<SessionResponse>> ExecuteAsync(
        CreateSessionRequest request,
        CancellationToken cancellationToken = default)
    {
        if (currentUserProvider.UserId is not { } userId)
        {
            return ApiResponse<SessionResponse>.Failure(
                SessionErrorCodes.Unauthorized,
                "User is not authenticated.");
        }

        var now = DateTime.UtcNow;
        var session = new ReviewSession
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = string.IsNullOrWhiteSpace(request.Title) ? "Untitled Session" : request.Title.Trim(),
            Status = SessionStatus.DRAFT,
            CreatedAt = now,
            UpdatedAt = null,
            IsActive = true
        };

        await reviewSessionRepository.AddAsync(session, cancellationToken);
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
