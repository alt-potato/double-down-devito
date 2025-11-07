namespace Project.Api.Utilities.Extensions;

public static class LoggerExtensions
{
    /// <summary>
    /// Logs the exception and throws it.
    /// </summary>
    /// <remarks>
    /// I KNOW THE TYPE PARAMETER IS SO INCREDIBLY CURSED I'M SO SORRY
    /// (it's literally just so i can put it after a null coalescing operator)
    /// </remarks>
    public static T LogAndThrow<T>(this ILogger logger, Exception ex, string? logMessage = null)
    {
        logger.LogError(ex, "{Message}", logMessage ?? ex.Message);
        throw ex;
    }

    public static void LogAndThrow(this ILogger logger, Exception ex, string? logMessage = null)
    {
        logger.LogError(ex, "{Message}", logMessage ?? ex.Message);
        throw ex;
    }
}
