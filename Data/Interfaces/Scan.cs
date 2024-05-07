namespace BackupUtilities.Data.Interfaces;

/// <summary>
/// Represents a single scan in the database.
/// </summary>
public class Scan
{
    /// <summary>
    /// Gets or sets the primary key of the scan.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the Date when the scan started.
    /// </summary>
    public string? StartDate { get; set; }

    /// <summary>
    /// Gets or sets the date when the scan finished.
    /// </summary>
    public string? EndDate { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the folder scan has finished.
    /// </summary>
    public bool StageFolderScanFinished { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the file scan has been completey initialized.
    /// This means from that point onwards the file scan can be started with the argument <c>continueLastScan</c> set
    /// to <c>true</c>.
    /// </summary>
    public bool StageFileScanInitialized { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the file scan has finished.
    /// </summary>
    public bool StageFileScanFinished { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the duplicate file analysis has finished.
    /// </summary>
    public bool StageDuplicateFileAnalysisFinished { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the orphaned file enumeration has finished.
    /// </summary>
    public bool StageOrphanedFileEnumerationFinished { get; set; }
}
