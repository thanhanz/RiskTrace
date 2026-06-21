namespace RiskTrace.Core.Constants;

/// <summary>
///     Common constants used across the application.
/// </summary>
public class CommonConstants
{
    /// <summary>
    ///     The default Windows working directory for RiskTrace files.
    /// </summary>
    public static readonly string WindowsWorkingDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "RiskTrace");
}
