namespace BackupUtilities.Services.Interfaces;

/// <summary>
/// Base interface for scan status objects.
/// </summary>
public interface IScanStatus
{
    /// <summary>
    /// Event that is fired when this object has changed.
    /// </summary>
    event EventHandler<EventArgs>? Changed;

    /// <summary>
    /// Gets a value indicating whether the scan is currently running.
    /// </summary>
    bool IsRunning { get; }

    /// <summary>
    /// Gets the status title.
    /// </summary>
    string Title { get; }

    /// <summary>
    /// Gets the status text.
    /// </summary>
    string Text { get; }

    /// <summary>
    /// Gets the progress with values in the range of [0.0, 1.0].
    /// </summary>
    double? Progress { get; }

    /// <summary>
    /// Reset the status.
    /// </summary>
    /// <returns>A task for async programming.</returns>
    Task ResetAsync();

    /// <summary>
    /// Begins the scan.
    /// </summary>
    /// <returns>A task for async programming.</returns>
    Task BeginAsync();

    /// <summary>
    /// Update the current status.
    /// </summary>
    /// <param name="text">The status text.</param>
    /// <param name="progress">The operation progress.</param>
    /// <returns>A task for async programming.</returns>
    Task UpdateAsync(string text, double? progress);

    /// <summary>
    /// Update the current status with only a new progress.
    /// </summary>
    /// <param name="progress">The operation progress.</param>
    /// <returns>A task for async programming.</returns>
    Task UpdateAsync(double? progress);

    /// <summary>
    /// Signals the end of the scan.
    /// </summary>
    /// <returns>A task for async programming.</returns>
    Task EndAsync();
}
