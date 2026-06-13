using RiskTrace.Core.Common;
using RiskTrace.Domain.Entities;
using RiskTrace.Domain.Response;
using RiskTrace.UseCases.Interfaces.Messages;
using RiskTrace.UseCases.Ports.Auth;
using RiskTrace.UseCases.Ports.Repositories;

namespace RiskTrace.UseCases.UseCases.Messages;

public sealed class GetSessionMessagesUseCase(
    ICurrentUserProvider currentUserProvider,
    IReviewSessionRepository reviewSessionRepository,
    IMessageRepository messageRepository) : IGetSessionMessagesUseCase
{
    public async Task<ApiResponse<PaginatedResult<MessageResponse>>> ExecuteAsync(
        Guid sessionId,
        PaginationRequest pagination,
        CancellationToken cancellationToken = default)
    {
        if (currentUserProvider.UserId is not { } userId)
        {
            return ApiResponse<PaginatedResult<MessageResponse>>.Failure(
                CommonErrors.Unauthorized("User is not authenticated."));
        }

        var session = await reviewSessionRepository.GetActiveByIdAsync(sessionId, userId, cancellationToken);
        if (session is null)
        {
            return ApiResponse<PaginatedResult<MessageResponse>>.Failure(
                CommonErrors.NotFound("Session not found."));
        }

        Guid? cursorId = null;
        if (!string.IsNullOrWhiteSpace(pagination.Cursor))
        {
            if (!Guid.TryParse(pagination.Cursor, out var parsedCursor))
            {
                return ApiResponse<PaginatedResult<MessageResponse>>.Failure(
                    CommonErrors.BadRequest("Cursor is invalid."));
            }

            cursorId = parsedCursor;
        }

        var limit = pagination.Limit > 0 ? pagination.Limit : 20;
        var take = limit + 1;
        var messages = await messageRepository.GetBySessionIdAsync(
            sessionId,
            cursorId,
            take,
            cancellationToken);

        var hasNextPage = messages.Count > limit;
        var pageItems = messages
            .Take(limit)
            .Select(ToResponse)
            .ToList();

        var nextCursor = hasNextPage
            ? pageItems[^1].Id.ToString()
            : null;

        return ApiResponse<PaginatedResult<MessageResponse>>.Success(
            new PaginatedResult<MessageResponse>(
                pageItems,
                nextCursor,
                hasNextPage));
    }

    private static MessageResponse ToResponse(Message message)
    {
        return new MessageResponse(
            Id: message.Id,
            SessionId: message.SessionId,
            Role: message.Role,
            Content: message.Content,
            CreatedAt: message.CreatedAt,
            UpdatedAt: message.UpdatedAt);
    }
}
