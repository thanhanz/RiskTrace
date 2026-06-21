namespace RiskTrace.Core.Interfaces.Logger;

/// <summary>
///     Represents a logger.
/// </summary>
public interface ILogger
{
    /// <summary>
    ///     Logs an information message.
    /// </summary>
    void LogInformation(string message, params object?[]? propertyValues);

    /// <summary>
    ///     Logs an information message with exception details.
    /// </summary>
    void LogInformation(Exception exception, string message, params object?[]? propertyValues);

    /// <summary>
    ///     Logs a debug message.
    /// </summary>
    void LogDebug(string message, params object?[]? propertyValues);

    /// <summary>
    ///     Logs a debug message with exception details.
    /// </summary>
    void LogDebug(Exception exception, string message, params object?[]? propertyValues);

    /// <summary>
    ///     Logs an error message.
    /// </summary>
    void LogError(string message, params object?[]? propertyValues);

    /// <summary>
    ///     Logs an error message with exception details.
    /// </summary>
    void LogError(Exception exception, string message, params object?[]? propertyValues);

    /// <summary>
    ///     Logs a warning message.
    /// </summary>
    void LogWarning(string message, params object?[]? propertyValues);

    /// <summary>
    ///     Logs a warning message with exception details.
    /// </summary>
    void LogWarning(Exception exception, string message, params object?[]? propertyValues);
}

/// <summary>
///     Represents a logger for a specific category type.
/// </summary>
/// <typeparam name="TCategoryName">The logger category type.</typeparam>
public interface ILogger<out TCategoryName> : ILogger
{
}
