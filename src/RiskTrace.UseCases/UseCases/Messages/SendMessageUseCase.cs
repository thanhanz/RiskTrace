using RiskTrace.Core.Abstractions;
using RiskTrace.Core.Common;
using RiskTrace.Core.Interfaces.Logger;
using RiskTrace.Domain.Entities;
using RiskTrace.Domain.Enums;
using RiskTrace.Domain.Response;
using RiskTrace.UseCases.Interfaces.Messages;
using RiskTrace.UseCases.Ports.Auth;
using RiskTrace.UseCases.Ports.Repositories;

namespace RiskTrace.UseCases.UseCases.Messages;

public sealed class SendMessageUseCase(
    ICurrentUserProvider currentUserProvider,
    IReviewSessionRepository reviewSessionRepository,
    IMessageRepository messageRepository,
    IUnitOfWork unitOfWork,
    ILogger<SendMessageUseCase> logger) : ISendMessageUseCase
{
    public async Task<ApiResponse<MessageResponse>> ExecuteAsync(
        Guid sessionId,
        string content,
        MessageRole role,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Handling send message request for session {SessionId} with role {Role}.",
            sessionId,
            role);

        if (currentUserProvider.UserId is not { } userId)
        {
            logger.LogWarning("Send message failed for session {SessionId}: user is not authenticated.", sessionId);
            return ApiResponse<MessageResponse>.Failure(
                CommonErrors.Unauthorized("User is not authenticated."));
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            logger.LogWarning("Send message failed for session {SessionId}: content is required.", sessionId);
            return ApiResponse<MessageResponse>.Failure(
                CommonErrors.FieldRequired("Content"));
        }

        var session = await reviewSessionRepository.GetActiveByIdAsync(sessionId, userId, cancellationToken);
        if (session is null)
        {
            logger.LogWarning(
                "Send message failed for session {SessionId} and user {UserId}: session not found.",
                sessionId,
                userId);
            return ApiResponse<MessageResponse>.Failure(
                CommonErrors.NotFound("Session not found."));
        }

        var message = new Message
        {
            Id = Guid.CreateVersion7(),
            SessionId = sessionId,
            Role = role,
            Content = content.Trim(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null,
            IsActive = true
        };

        await messageRepository.AddAsync(message, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Send message succeeded for session {SessionId} with message {MessageId}.",
            sessionId,
            message.Id);

        return ApiResponse<MessageResponse>.Success(ToResponse(message));
    }

    private static MessageResponse ToResponse(Message message)
    {
        return new MessageResponse(
            Id: message.Id,
            SessionId: message.SessionId,
            Role: message.Role,
            Content: message.Content);
    }
}
