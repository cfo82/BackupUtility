namespace BackupUtilities.Services.Interfaces;

using System;

/// <summary>
/// The error handler is responsible to display all errors to the user.
/// </summary>
public interface IErrorHandler
{
    /// <summary>
    /// Signalled when an error has occured.
    /// </summary>
    event EventHandler? ErrorChanged;

    /// <summary>
    /// Gets or sets current error to be handled.
    /// </summary>
    Exception? Error { get; set; }
}
