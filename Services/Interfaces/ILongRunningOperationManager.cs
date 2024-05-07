namespace BackupUtilities.Services.Interfaces;

/// <summary>
/// Represents the status of a long running operation.
/// </summary>
public interface ILongRunningOperationManager
{
    /// <summary>
    /// Update the current long running operation.
    /// </summary>
    event EventHandler<EventArgs>? OperationChanged;

    /// <summary>
    /// Gets a value indicating whether an operation is currently running.
    /// </summary>
    public bool IsRunning { get; }

    /// <summary>
    /// Gets the title of the operation.
    /// </summary>
    public string Title { get; }

    /// <summary>
    /// Gets the current type of operation.
    /// </summary>
    public ScanType ScanType { get; }

    /// <summary>
    /// Gets the current status as a text.
    /// </summary>
    public string Text { get; }

    /// <summary>
    /// Gets the current operation progress in percent between (0 and 100) if known. If the percentage
    /// is unknown the value will be null.
    /// </summary>
    public double? Percentage { get; }

    /// <summary>
    /// Begin a new long running operation.
    /// </summary>
    /// <param name="title">The operations title.</param>
    /// <param name="scanType">The type of scan that is running.</param>
    /// <returns>A task for async programming.</returns>
    Task BeginOperationAsync(string title, ScanType scanType);

    /// <summary>
    /// Update the current long running operation.
    /// </summary>
    /// <param name="text">The status text.</param>
    /// <param name="percentage">The operation progress.</param>
    /// <returns>A task for async programming.</returns>
    Task UpdateOperationAsync(string text, double? percentage);

    /// <summary>
    /// Update the current long running operation with only a new percentage.
    /// </summary>
    /// <param name="percentage">The operation progress.</param>
    /// <returns>A task for async programming.</returns>
    Task UpdateOperationAsync(double? percentage);

    /// <summary>
    /// End the current long running operation.
    /// </summary>
    /// <returns>A task for async programming.</returns>
    Task EndOperationAsync();
}
