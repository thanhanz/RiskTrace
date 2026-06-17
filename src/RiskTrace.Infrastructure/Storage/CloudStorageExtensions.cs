using Amazon.Runtime;
using Amazon.S3;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RiskTrace.UseCases.Ports.Storage;

namespace RiskTrace.Infrastructure.Storage;

internal static class CloudStorageExtensions
{
    public static IServiceCollection AddR2Storage(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions<R2StorageOptions>()
            .Bind(configuration.GetSection(R2StorageOptions.SectionName))
            .Validate(ValidateR2Options, "Cloudflare R2 storage options are invalid.")
            .ValidateOnStart();

        services.AddSingleton<IAmazonS3>(serviceProvider =>
        {
            var options = serviceProvider
                .GetRequiredService<IOptions<R2StorageOptions>>()
                .Value;

            var config = new AmazonS3Config
            {
                ServiceURL = $"https://{options.AccountId}.r2.cloudflarestorage.com",
                ForcePathStyle = true,
                AuthenticationRegion = "auto"
            };

            return new AmazonS3Client(
                new BasicAWSCredentials(options.AccessKeyId, options.SecretAccessKey),
                config);
        });

        services.AddScoped<ICloudStorage, R2CloudStorage>();

        return services;
    }

    private static bool ValidateR2Options(R2StorageOptions options)
    {
        return !string.IsNullOrWhiteSpace(options.AccountId)
            && !string.IsNullOrWhiteSpace(options.AccessKeyId)
            && !string.IsNullOrWhiteSpace(options.SecretAccessKey)
            && !string.IsNullOrWhiteSpace(options.BucketName)
            && options.PresignedUrlMinutes > 0
            && options.MultipartThresholdBytes > 0
            && options.MultipartPartSizeBytes > 0
            && options.MultipartPartSizeBytes <= options.MultipartThresholdBytes;
    }
}
