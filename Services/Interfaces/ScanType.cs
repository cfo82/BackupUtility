namespace BackupUtilities.Services.Interfaces;

/// <summary>
/// Defines the type of operation that is run by the <see cref="ILongRunningOperationManager"/>.
/// </summary>
public enum ScanType
{
    /// <summary>
    /// A folder scan is runnign.
    /// </summary>
    FolderScan,

    /// <summary>
    /// A file scan is running.
    /// </summary>
    FileScan,

    /// <summary>
    /// The duplicate file analysis is performed.
    /// </summary>
    DuplicateFileAnalysis,

    /// <summary>
    /// The orphaned file scan is running.
    /// </summary>
    OrphanedFileScan,
}
