namespace RiskTrace.Infrastructure.Storage;

public sealed class R2StorageOptions
{
    public const string SectionName = "Storage:R2";

    public string AccountId { get; set; } = string.Empty;

    public string AccessKeyId { get; set; } = string.Empty;

    public string SecretAccessKey { get; set; } = string.Empty;

    public string BucketName { get; set; } = string.Empty;

    public string PublicBaseUrl { get; set; } = string.Empty;

    public int PresignedUrlMinutes { get; set; } = 15;

    public long MultipartThresholdBytes { get; set; } = 26_214_400;

    public long MultipartPartSizeBytes { get; set; } = 8_388_608;
}
