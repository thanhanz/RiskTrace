using RiskTrace.Core.Common;
using RiskTrace.Core.Interfaces.Logger;
using RiskTrace.Domain.Enums;
using RiskTrace.Domain.Request.Documents;
using RiskTrace.Domain.Response.Documents;
using RiskTrace.UseCases.Interfaces.Documents;
using RiskTrace.UseCases.Ports.Auth;
using RiskTrace.UseCases.Ports.Repositories;
using RiskTrace.UseCases.Ports.Storage;

namespace RiskTrace.UseCases.UseCases.Documents;

public sealed class InitiateDocumentUploadUseCase(
    ICurrentUserProvider currentUserProvider,
    IReviewSessionRepository reviewSessionRepository,
    ICloudStorage cloudStorage,
    ILogger<InitiateDocumentUploadUseCase> logger) : IInitiateDocumentUploadUseCase
{
    private const int MaxMultipartParts = 10_000;
    private const string PdfContentType = "application/pdf";

    public async Task<ApiResponse<DocumentUploadResponse>> ExecuteAsync(
        Guid sessionId,
        InitiateDocumentUploadRequest request,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Handling initiate document upload request for session {SessionId} and file {FileName}.",
            sessionId,
            request.FileName ?? string.Empty);

        if (currentUserProvider.UserId is not { } userId)
        {
            logger.LogWarning("Initiate document upload failed for session {SessionId}: user is not authenticated.", sessionId);
            return ApiResponse<DocumentUploadResponse>.Failure(
                CommonErrors.Unauthorized("User is not authenticated."));
        }

        var session = await reviewSessionRepository.GetActiveByIdAsync(sessionId, userId, cancellationToken);
        if (session is null)
        {
            logger.LogWarning(
                "Initiate document upload failed for session {SessionId} and user {UserId}: session not found.",
                sessionId,
                userId);
            return ApiResponse<DocumentUploadResponse>.Failure(
                CommonErrors.NotFound("Session not found."));
        }

        var fileName = NormalizeFileName(request.FileName);
        if (string.IsNullOrWhiteSpace(fileName))
        {
            logger.LogWarning("Initiate document upload failed for session {SessionId}: file name is required.", sessionId);
            return ApiResponse<DocumentUploadResponse>.Failure(
                CommonErrors.FieldRequired(nameof(request.FileName)));
        }

        if (!string.Equals(request.ContentType, PdfContentType, StringComparison.OrdinalIgnoreCase))
        {
            logger.LogWarning(
                "Initiate document upload failed for session {SessionId}: unsupported content type {ContentType}.",
                sessionId,
                request.ContentType ?? string.Empty);
            return ApiResponse<DocumentUploadResponse>.Failure(
                CommonErrors.BadRequest("Only PDF files are supported."));
        }

        if (request.FileSize <= 0)
        {
            logger.LogWarning(
                "Initiate document upload failed for session {SessionId}: invalid file size {FileSize}.",
                sessionId,
                request.FileSize);
            return ApiResponse<DocumentUploadResponse>.Failure(
                CommonErrors.BadRequest("File size must be greater than zero."));
        }

        var documentId = Guid.NewGuid();
        var objectKey = BuildObjectKey(userId, sessionId, documentId, fileName);

        //For small file size -> no need chunking
        if (request.FileSize <= cloudStorage.MultipartThresholdBytes)
        {
            var upload = await cloudStorage.CreatePresignedUploadUrlAsync(
                objectKey,
                request.ContentType,
                cancellationToken);

            logger.LogInformation(
                "Initiate document upload succeeded for document {DocumentId} in single upload mode.",
                documentId);

            return ApiResponse<DocumentUploadResponse>.Success(new DocumentUploadResponse(
                DocumentId: documentId,
                UploadType: FileUploadType.Single,
                ObjectKey: objectKey,
                UploadUrl: upload.UploadUrl,
                UploadId: null,
                PartSizeBytes: null,
                Parts: Array.Empty<DocumentUploadPartResponse>(),
                ExpiresAt: upload.ExpiresAt));
        }

        //For large file size -> Need chunking
        var partCount = (int)Math.Ceiling((double)request.FileSize / cloudStorage.MultipartPartSizeBytes);
        if (partCount > MaxMultipartParts)
        {
            logger.LogWarning(
                "Initiate document upload failed for session {SessionId}: multipart part count {PartCount} exceeded limit.",
                sessionId,
                partCount);
            return ApiResponse<DocumentUploadResponse>.Failure(
                CommonErrors.BadRequest("File is too large for multipart upload."));
        }

        var multipartUpload = await cloudStorage.CreateMultipartUploadAsync(
            objectKey,
            request.ContentType,
            cancellationToken);

        var parts = await cloudStorage.CreatePresignedPartUploadUrlsAsync(
            objectKey,
            multipartUpload.UploadId,
            partCount,
            cancellationToken);

        logger.LogInformation(
            "Initiate document upload succeeded for document {DocumentId} in multipart mode with {PartCount} parts.",
            documentId,
            partCount);

        return ApiResponse<DocumentUploadResponse>.Success(new DocumentUploadResponse(
            DocumentId: documentId,
            UploadType: FileUploadType.Multipart,
            ObjectKey: objectKey,
            UploadUrl: null,
            UploadId: multipartUpload.UploadId,
            PartSizeBytes: cloudStorage.MultipartPartSizeBytes,
            Parts: parts
                .Select(part => new DocumentUploadPartResponse(part.PartNumber, part.UploadUrl))
                .ToList(),
            ExpiresAt: parts.First().ExpiresAt));
    }

    private static string BuildObjectKey(
        Guid userId,
        Guid sessionId,
        Guid documentId,
        string fileName)
    {
        return $"users/{userId}/sessions/{sessionId}/documents/{documentId}/{fileName}";
    }

    private static string NormalizeFileName(string? fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return string.Empty;
        }

        var normalizedFileName = Path.GetFileName(fileName).Trim();
        foreach (var invalidCharacter in Path.GetInvalidFileNameChars())
        {
            normalizedFileName = normalizedFileName.Replace(invalidCharacter, '_');
        }

        return normalizedFileName;
    }
}
