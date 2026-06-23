using RiskTrace.Core.Interfaces.Logger;
using RiskTrace.Domain.Events;
using RiskTrace.UseCases.Interfaces.ReviewResults;

namespace RiskTrace.UseCases.UseCases.ReviewResults;

public sealed class HandleAiResponseUseCase(
    ILogger<HandleAiResponseUseCase> logger) : IHandleAiResponseUseCase
{
    public Task ExecuteAsync(
        TemporaryAiResponseEvent response,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        logger.LogInformation(
            "Received temporary AI response {EventType} for document {DocumentId} in session {SessionId} at {OccurredAt}. Status: {Status}. User: {UserId}. File: {FileName}. Storage path: {StoragePath}.",
            response.EventType ?? string.Empty,
            response.DocumentId ?? string.Empty,
            response.SessionId ?? string.Empty,
            response.OccurredAt ?? string.Empty,
            response.Payload?.Status ?? string.Empty,
            response.Payload?.UserId ?? string.Empty,
            response.Payload?.FileName ?? string.Empty,
            response.Payload?.StoragePath ?? string.Empty);

        return Task.CompletedTask;
    }
}
