namespace BackupUtilities.Services;

using System;
using BackupUtilities.Services.Interfaces;
using Microsoft.Extensions.Logging;

/// <summary>
/// Default implementation of <see cref="IErrorHandler"/>.
/// </summary>
public class ErrorHandler : IErrorHandler
{
    private readonly ILogger<ErrorHandler> _logger;
    private Exception? _error;

    /// <summary>
    /// Initializes a new instance of the <see cref="ErrorHandler"/> class.
    /// </summary>
    /// <param name="logger">The logger to use for logging.</param>
    public ErrorHandler(
        ILogger<ErrorHandler> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public event EventHandler? ErrorChanged;

    /// <inheritdoc />
    public Exception? Error
    {
        get
        {
            return _error;
        }

        set
        {
            if (_error != value)
            {
                _error = value;

                if (_error != null)
                {
                    _logger.LogError(_error, "Error occured");
                }

                ErrorChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
