namespace BackupUtilities.Services.Interfaces;

/// <summary>
/// Represents the status of a long running operation.
/// </summary>
public interface IScanStatusManager
{
    /// <summary>
    /// Event that is fired when any scan object has changed.
    /// </summary>
    event EventHandler<EventArgs>? Changed;

    /// <summary>
    /// Gets a value indicating whether any scan is currently running.
    /// </summary>
    bool IsRunning { get; }

    /// <summary>
    /// Gets the status of the full scan. Parts of the process are members of it.
    /// </summary>
    IFullScanStatus FullScanStatus { get; }
}
