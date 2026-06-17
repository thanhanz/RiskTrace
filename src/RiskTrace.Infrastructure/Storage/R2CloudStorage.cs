using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using RiskTrace.UseCases.Ports.Storage;

namespace RiskTrace.Infrastructure.Storage;

public sealed class R2CloudStorage(
    IAmazonS3 s3Client,
    IOptions<R2StorageOptions> options) : ICloudStorage
{
    private readonly R2StorageOptions _options = options.Value;

    public long MultipartThresholdBytes => _options.MultipartThresholdBytes;

    public long MultipartPartSizeBytes => _options.MultipartPartSizeBytes;

    public Task<PresignedUploadResult> CreatePresignedUploadUrlAsync(
        string objectKey,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(_options.PresignedUrlMinutes);
        var uploadUrl = s3Client.GetPreSignedURL(new GetPreSignedUrlRequest
        {
            BucketName = _options.BucketName,
            Key = objectKey,
            Verb = HttpVerb.PUT,
            ContentType = contentType,
            Expires = expiresAt
        });

        return Task.FromResult(new PresignedUploadResult(uploadUrl, expiresAt));
    }

    public async Task<MultipartUploadResult> CreateMultipartUploadAsync(
        string objectKey,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        var response = await s3Client.InitiateMultipartUploadAsync(new InitiateMultipartUploadRequest
        {
            BucketName = _options.BucketName,
            Key = objectKey,
            ContentType = contentType
        }, cancellationToken);

        return new MultipartUploadResult(response.UploadId);
    }

    public Task<IReadOnlyList<PresignedPartUploadResult>> CreatePresignedPartUploadUrlsAsync(
        string objectKey,
        string uploadId,
        int partCount,
        CancellationToken cancellationToken = default)
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(_options.PresignedUrlMinutes);
        var parts = Enumerable.Range(1, partCount)
            .Select(partNumber => new PresignedPartUploadResult(
                partNumber,
                s3Client.GetPreSignedURL(new GetPreSignedUrlRequest
                {
                    BucketName = _options.BucketName,
                    Key = objectKey,
                    Verb = HttpVerb.PUT,
                    Expires = expiresAt,
                    UploadId = uploadId,
                    PartNumber = partNumber
                }),
                expiresAt))
            .ToList();

        return Task.FromResult<IReadOnlyList<PresignedPartUploadResult>>(parts);
    }

    public async Task CompleteMultipartUploadAsync(
        string objectKey,
        string uploadId,
        IReadOnlyList<CompletedUploadPart> parts,
        CancellationToken cancellationToken = default)
    {
        await s3Client.CompleteMultipartUploadAsync(new CompleteMultipartUploadRequest
        {
            BucketName = _options.BucketName,
            Key = objectKey,
            UploadId = uploadId,
            PartETags = parts
                .OrderBy(part => part.PartNumber)
                .Select(part => new PartETag(part.PartNumber, part.ETag))
                .ToList()
        }, cancellationToken);
    }

    public async Task<bool> ObjectExistsAsync(
        string objectKey,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await s3Client.GetObjectMetadataAsync(new GetObjectMetadataRequest
            {
                BucketName = _options.BucketName,
                Key = objectKey
            }, cancellationToken);

            return true;
        }
        catch (AmazonS3Exception exception) when (exception.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return false;
        }
    }
}
