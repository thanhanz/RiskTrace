using Microsoft.Extensions.Logging;
using RiskTrace.Core.Abstractions;
using RiskTrace.Core.Common;
using RiskTrace.Domain.Entities;
using RiskTrace.Domain.Enums;
using RiskTrace.Domain.Events;
using RiskTrace.Domain.Messaging;
using RiskTrace.Domain.Request.Documents;
using RiskTrace.Domain.Response.Documents;
using RiskTrace.UseCases.Interfaces.Documents;
using RiskTrace.UseCases.Ports.Auth;
using RiskTrace.UseCases.Ports.Messaging;
using RiskTrace.UseCases.Ports.Repositories;
using RiskTrace.UseCases.Ports.Storage;

namespace RiskTrace.UseCases.UseCases.Documents;

public sealed class CompleteDocumentUploadUseCase(
    ICurrentUserProvider currentUserProvider,
    IReviewSessionRepository reviewSessionRepository,
    IDocumentRepository documentRepository,
    ICloudStorage cloudStorage,
    IUnitOfWork unitOfWork,
    IMessageQueueService messageQueueService,
    ILogger<CompleteDocumentUploadUseCase> logger) : ICompleteDocumentUploadUseCase
{
    private const string PdfContentType = "application/pdf";

    public async Task<ApiResponse<DocumentResponse>> ExecuteAsync(
        Guid sessionId,
        CompleteDocumentUploadRequest request,
        CancellationToken cancellationToken = default)
    {
        if (currentUserProvider.UserId is not { } userId)
        {
            return ApiResponse<DocumentResponse>.Failure(
                CommonErrors.Unauthorized("User is not authenticated."));
        }

        var session = await reviewSessionRepository.GetActiveByIdAsync(sessionId, userId, cancellationToken);
        if (session is null)
        {
            return ApiResponse<DocumentResponse>.Failure(
                CommonErrors.NotFound("Session not found."));
        }

        var validationError = ValidateRequest(request, userId, sessionId);
        if (validationError is not null)
        {
            return ApiResponse<DocumentResponse>.Failure(validationError);
        }

        if (request.UploadType == FileUploadType.Single)
        {
            var objectExists = await cloudStorage.ObjectExistsAsync(request.ObjectKey, cancellationToken);
            if (!objectExists)
            {
                return ApiResponse<DocumentResponse>.Failure(
                    CommonErrors.BadRequest("Uploaded file was not found in cloud storage."));
            }
        }
        else
        {
            await cloudStorage.CompleteMultipartUploadAsync(
                request.ObjectKey,
                request.UploadId!,
                request.Parts!
                    .Select(part => new CompletedUploadPart(part.PartNumber, part.ETag))
                    .ToList(),
                cancellationToken);
        }

        var now = DateTime.UtcNow;
        var document = new Document
        {
            Id = request.DocumentId,
            SessionId = sessionId,
            FileName = Path.GetFileName(request.FileName).Trim(),
            FilePath = request.ObjectKey,
            ContentType = request.ContentType,
            FileSize = request.FileSize,
            Status = DocumentStatus.UPLOADED,
            CreatedAt = now,
            UpdatedAt = null,
            IsActive = true
        };

        await documentRepository.AddAsync(document, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await PublishDocumentUploadedAsync(document, userId, cancellationToken);

        return ApiResponse<DocumentResponse>.Success(ToResponse(document));
    }

    private async Task PublishDocumentUploadedAsync(
        Document document,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var event = new DocumentUploadedEvent
        {
            DocumentId = document.Id,
            SessionId = document.SessionId,
            UserId = userId,
            StoragePath = document.FilePath,
            FileName = document.FileName
        };

        try
        {
            await messageQueueService.PublishAsync(
                MessagingConstants.RoutingKeys.DocumentUploadedRequest,
                event,
                cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to publish document upload event for document {DocumentId}.",
                document.Id);
        }
    }

    private static ErrorResponse? ValidateRequest(
        CompleteDocumentUploadRequest request,
        Guid userId,
        Guid sessionId)
    {
        if (request.DocumentId == Guid.Empty)
        {
            return CommonErrors.BadRequest("Document id is required.");
        }

        if (string.IsNullOrWhiteSpace(request.ObjectKey))
        {
            return CommonErrors.FieldRequired(nameof(request.ObjectKey));
        }

        var expectedObjectKeyPrefix = $"users/{userId}/sessions/{sessionId}/documents/{request.DocumentId}/";
        if (!request.ObjectKey.StartsWith(expectedObjectKeyPrefix, StringComparison.Ordinal))
        {
            return CommonErrors.BadRequest("Object key does not match this document upload.");
        }

        if (string.IsNullOrWhiteSpace(request.FileName))
        {
            return CommonErrors.FieldRequired(nameof(request.FileName));
        }

        if (!string.Equals(request.ContentType, PdfContentType, StringComparison.OrdinalIgnoreCase))
        {
            return CommonErrors.BadRequest("Only PDF files are supported.");
        }

        if (request.FileSize <= 0)
        {
            return CommonErrors.BadRequest("File size must be greater than zero.");
        }

        if (request.UploadType == FileUploadType.Single)
        {
            return null;
        }

        if (request.UploadType != FileUploadType.Multipart)
        {
            return CommonErrors.BadRequest("Upload type must be single or multipart.");
        }

        if (string.IsNullOrWhiteSpace(request.UploadId))
        {
            return CommonErrors.FieldRequired(nameof(request.UploadId));
        }

        if (request.Parts is null || request.Parts.Count == 0)
        {
            return CommonErrors.BadRequest("Multipart upload parts are required.");
        }

        if (request.Parts.Any(part => part.PartNumber <= 0 || string.IsNullOrWhiteSpace(part.ETag)))
        {
            return CommonErrors.BadRequest("Each multipart part must include a positive part number and ETag.");
        }

        return null;
    }

    private static DocumentResponse ToResponse(Document document)
    {
        return new DocumentResponse(
            Id: document.Id,
            SessionId: document.SessionId,
            FileName: document.FileName,
            FilePath: document.FilePath,
            ContentType: document.ContentType,
            FileSize: document.FileSize,
            Status: document.Status,
            CreatedAt: document.CreatedAt,
            UpdatedAt: document.UpdatedAt);
    }
}
