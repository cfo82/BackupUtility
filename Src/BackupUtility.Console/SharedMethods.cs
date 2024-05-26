namespace BackupUtilities.Console;

using Microsoft.Extensions.Logging;
using Serilog;

/// <summary>
/// Shared methods for all commands.
/// </summary>
public static class SharedMethods
{
    /// <summary>
    /// Add a logger to LogFile.
    /// </summary>
    /// <param name="loggerFactory">The logger factory.</param>
    /// <param name="logFile">The path to the logfile.</param>
    public static void WriteToLogFile(this ILoggerFactory loggerFactory, string? logFile)
    {
        if (logFile != null)
        {
            var rootLogger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.File(logFile)
                .CreateLogger();
            loggerFactory.AddSerilog(rootLogger);
        }
    }
}
