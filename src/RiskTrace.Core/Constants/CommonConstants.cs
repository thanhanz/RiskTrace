namespace RiskTrace.Core.Constants;

/// <summary>
///     Common constants used across the application.
/// </summary>
public class CommonConstants
{
    /// <summary>
    ///     Indicates whether the application is running on Windows.
    /// </summary>
    public static readonly bool IsWindows = OperatingSystem.IsWindows();

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
}
