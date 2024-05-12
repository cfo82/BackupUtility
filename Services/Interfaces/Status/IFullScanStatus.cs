namespace BackupUtilities.Services.Interfaces.Status;

/// <summary>
/// This is the root scan interface.
/// </summary>
public interface IFullScanStatus : IScanStatus
{
    /// <summary>
    /// Gets the status of the folder scan.
    /// </summary>
    public IScanStatus FolderScanStatus { get; }

    /// <summary>
    /// Gets the status of the file scan.
    /// </summary>
    public IFileScanStatus FileScanStatus { get; }

    /// <summary>
    /// Gets the status of the duplicate file analysis.
    /// </summary>
    public IScanStatus DuplicateFileAnalysisStatus { get; }

    /// <summary>
    /// Gets the status of the orphaned file scan.
    /// </summary>
    public IScanStatus OrphanedFileScanStatus { get; }
}
