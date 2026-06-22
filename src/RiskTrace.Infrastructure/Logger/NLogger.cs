using NLog;
using NLog.Config;
using NLog.Targets;
using RiskTrace.Core.Constants;
using RiskTrace.Core.Interfaces.Logger;

namespace RiskTrace.Infrastructure.Logger;

public sealed class NLogger<TCategoryName> : ILogger<TCategoryName>
{
    private readonly NLog.Logger _logger;

    public NLogger()
    {
        var filePath = CommonConstants.IsWindows
            ? LoggerConstants.WindowsDirectory
            : OperatingSystem.IsLinux()
                ? LoggerConstants.LinuxDirectory
                : LoggerConstants.MacDirectory;

        if (!Directory.Exists(filePath))
        {
            Directory.CreateDirectory(filePath);
        }

        var loggingConfiguration = new LoggingConfiguration();

        var fileTarget = new FileTarget("fileTarget")
        {
            FileName = Path.Combine(filePath, "risktrace_${date:format=yyyyMMdd}.log"),
            Layout = LoggerConstants.Layout,
            ArchiveAboveSize = LoggerConstants.FileSizeLimitBytes,
            MaxArchiveFiles = LoggerConstants.MaxArchiveFiles,
            KeepFileOpen = false
        };

        loggingConfiguration.AddRuleForAllLevels(fileTarget);

        LogManager.Configuration = loggingConfiguration;
        _logger = LogManager.GetLogger(typeof(TCategoryName).FullName!);
    }

    public void LogInformation(string message, params object?[]? propertyValues)
    {
        _logger.Info(message, propertyValues ?? Array.Empty<object?>());
    }

    public void LogInformation(Exception exception, string message, params object?[]? propertyValues)
    {
        _logger.Info(exception, message, propertyValues ?? Array.Empty<object?>());
    }

    public void LogDebug(string message, params object?[]? propertyValues)
    {
        _logger.Debug(message, propertyValues ?? Array.Empty<object?>());
    }

    public void LogDebug(Exception exception, string message, params object?[]? propertyValues)
    {
        _logger.Debug(exception, message, propertyValues ?? Array.Empty<object?>());
    }

    public void LogError(string message, params object?[]? propertyValues)
    {
        _logger.Error(message, propertyValues ?? Array.Empty<object?>());
    }

    public void LogError(Exception exception, string message, params object?[]? propertyValues)
    {
        _logger.Error(exception, message, propertyValues ?? Array.Empty<object?>());
    }

    public void LogWarning(string message, params object?[]? propertyValues)
    {
        _logger.Warn(message, propertyValues ?? Array.Empty<object?>());
    }

    public void LogWarning(Exception exception, string message, params object?[]? propertyValues)
    {
        _logger.Warn(exception, message, propertyValues ?? Array.Empty<object?>());
    }
}
