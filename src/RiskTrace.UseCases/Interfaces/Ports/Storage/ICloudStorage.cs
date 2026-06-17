namespace RiskTrace.UseCases.Ports.Storage;

public interface ICloudStorage
{
    long MultipartThresholdBytes { get; }

    long MultipartPartSizeBytes { get; }

    Task<PresignedUploadResult> CreatePresignedUploadUrlAsync(
        string objectKey,
        string contentType,
        CancellationToken cancellationToken = default);

    Task<MultipartUploadResult> CreateMultipartUploadAsync(
        string objectKey,
        string contentType,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PresignedPartUploadResult>> CreatePresignedPartUploadUrlsAsync(
        string objectKey,
        string uploadId,
        int partCount,
        CancellationToken cancellationToken = default);

    Task CompleteMultipartUploadAsync(
        string objectKey,
        string uploadId,
        IReadOnlyList<CompletedUploadPart> parts,
        CancellationToken cancellationToken = default);

    Task<bool> ObjectExistsAsync(
        string objectKey,
        CancellationToken cancellationToken = default);

    // TODO: Add multipart abort cleanup if abandoned uploads become a cost issue.
}

public sealed record PresignedUploadResult(
    string UploadUrl,
    DateTime ExpiresAt);

public sealed record MultipartUploadResult(
    string UploadId);

public sealed record PresignedPartUploadResult(
    int PartNumber,
    string UploadUrl,
    DateTime ExpiresAt);

public sealed record CompletedUploadPart(
    int PartNumber,
    string ETag);
