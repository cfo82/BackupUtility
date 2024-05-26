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
    /// Gets or sets the key of the settings that belong to this instance.
    /// </summary>
    public long SettingsId { get; set; }

    /// <summary>
    /// Gets or sets the date when the scan was created.
    /// </summary>
    public DateTime CreatedDate { get; set; }

    /// <summary>
    /// Gets or sets the Date when the scan started.
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// Gets or sets the date when the scan finished.
    /// </summary>
    public DateTime? FinishedDate { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the folder scan has finished.
    /// </summary>
    public bool StageFolderScanFinished { get; set; }

    /// <summary>
    /// Gets or sets the date when the folder scan was started.
    /// </summary>
    public DateTime? FolderScanStartDate { get; set; }

    /// <summary>
    /// Gets or sets the date when the folder scan was finished.
    /// </summary>
    public DateTime? FolderScanFinishedDate { get; set; }

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
    /// Gets or sets the date when the file scan was started.
    /// </summary>
    public DateTime? FileScanStartDate { get; set; }

    /// <summary>
    /// Gets or sets the date when the file scan was finished.
    /// </summary>
    public DateTime? FileScanFinishedDate { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the duplicate file analysis has finished.
    /// </summary>
    public bool StageDuplicateFileAnalysisFinished { get; set; }

    /// <summary>
    /// Gets or sets the date when the duplicate file analysis was started.
    /// </summary>
    public DateTime? DuplicateFileAnalysisStartDate { get; set; }

    /// <summary>
    /// Gets or sets the date when the duplicate file analysis was finished.
    /// </summary>
    public DateTime? DuplicateFileAnalysisFinishedDate { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the orphaned file enumeration has finished.
    /// </summary>
    public bool StageOrphanedFileEnumerationFinished { get; set; }

    /// <summary>
    /// Gets or sets the date when the orphaned file enumeration was started.
    /// </summary>
    public DateTime? OrphanedFileEnumerationStartDate { get; set; }

    /// <summary>
    /// Gets or sets the date when the orphaned file enumeration was finished.
    /// </summary>
    public DateTime? OrphanedFileEnumerationFinishedDate { get; set; }
}
