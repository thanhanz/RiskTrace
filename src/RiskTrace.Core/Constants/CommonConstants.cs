namespace RiskTrace.Core.Constants;

/// <summary>
///     Common constants used across the application.
/// </summary>
public static class CommonConstants
{
    /// <summary>
    ///     Indicates whether the application is running on Windows.
    /// </summary>
    public static readonly bool IsWindows = OperatingSystem.IsWindows();

    /// <summary>
    ///     Indicates whether the application is running on macOS.
    /// </summary>
    public static readonly bool IsMacOS = OperatingSystem.IsMacOS();

    /// <summary>
    ///     Indicates whether the application is running on Linux.
    /// </summary>
    public static readonly bool IsLinux = OperatingSystem.IsLinux();

    /// <summary>
    ///     The default Windows working directory for RiskTrace files.
    /// </summary>
    public static readonly string WindowsWorkingDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "RiskTrace");

    /// <summary>
    ///     The default macOS working directory for RiskTrace files.
    /// </summary>
    public static readonly string MacWorkingDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        "Library",
        "Logs",
        "RiskTrace");

    /// <summary>
    ///     The default Linux working directory for RiskTrace files.
    ///     In Docker, Directory.GetCurrentDirectory() is usually /app.
    /// </summary>
    public static readonly string LinuxWorkingDirectory = Directory.GetCurrentDirectory();
}