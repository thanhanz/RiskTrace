namespace RiskTrace.Core.Constants;

/// <summary>
///     Constants related to the logger.
/// </summary>
public static class LoggerConstants
{
    /// <summary>
    ///     The maximum file size limit in bytes.
    /// </summary>
    public const int FileSizeLimitBytes = 5 * 1024 * 1024; // 5 MiB

    /// <summary>
    ///     The maximum number of archive files.
    /// </summary>
    public const int MaxArchiveFiles = 2;

    /// <summary>
    ///     The layout pattern for log messages.
    /// </summary>
    public const string Layout =
        "${longdate:universalTime=true} [${threadid}] ${level:uppercase=true} ${logger} - ${message}${onexception: | ${exception:format=@}}";

    /// <summary>
    ///     The directory path for Windows.
    /// </summary>
    public static readonly string WindowsDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "RiskTrace",
        "logs");

    /// <summary>
    ///     The directory path for Linux containers.
    /// </summary>
    public static readonly string LinuxDirectory = Path.Combine(
        CommonConstants.LinuxWorkingDirectory,
        "logs");

    /// <summary>
    ///     The directory path for macOS.
    /// </summary>
    public static readonly string MacDirectory = CommonConstants.MacWorkingDirectory;
}