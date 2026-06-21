using RiskTrace.Core.Abstractions;
using RiskTrace.Core.Common;
using RiskTrace.Core.Interfaces.Logger;
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
    IUnitOfWork unitOfWork,
    ILogger<CreateSessionUseCase> logger) : ICreateSessionUseCase
{
    public async Task<ApiResponse<SessionResponse>> ExecuteAsync(
        CreateSessionRequest request,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Handling create session request with title {Title}.",
            request.Title ?? string.Empty);

        if (currentUserProvider.UserId is not { } userId)
        {
            logger.LogWarning("Create session failed: user is not authenticated.");
            return ApiResponse<SessionResponse>.Failure(
                CommonErrors.Unauthorized("User is not authenticated."));
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

        logger.LogInformation("Create session succeeded for user {UserId} with session {SessionId}.", userId, session.Id);

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
