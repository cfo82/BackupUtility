namespace BackupUtilities.Services.Interfaces.Status;

/// <summary>
/// The file scan status is an extended scan status. It contains a second progress indicator which represents
/// the scanning of the current folder. This can be useful in case of large folders with large files where otherwise
/// it would look like the process is hung but its still processing files in this folder.
/// </summary>
public interface IFileScanStatus : IScanStatus
{
    /// <summary>
    /// Gets the status text for file enumeration inside a folder.
    /// </summary>
    string FolderEnumerationText { get; }

    /// <summary>
    /// Gets the progress of enumerating the files in the current folder with values in the range of [0.0, 1.0].
    /// </summary>
    double FolderEnumerationProgress { get; }

    /// <summary>
    /// Update the current status of the folder enumeration.
    /// </summary>
    /// <param name="text">The hint text.</param>
    /// <param name="progress">The progress.</param>
    /// <returns>A task for async programming.</returns>
    Task UpdateFolderEnumerationStatusAsync(string text, double progress);
}
