namespace BackupUtilities.Services;

using BackupUtilities.Services.Interfaces;

/// <summary>
/// Default implementation of <see cref="IFullScanStatus"/>.
/// </summary>
public class FullScanStatus : ScanStatus, IFullScanStatus
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FullScanStatus"/> class.
    /// </summary>
    /// <param name="uiDispatcherService">The UI Dispatcher Service.</param>
    /// <param name="title">Title of this status object.</param>
    public FullScanStatus(IUiDispatcherService uiDispatcherService, string title)
        : base(uiDispatcherService, title)
    {
        FolderScanStatus = new ScanStatus(uiDispatcherService, "Folder Scan");
        FileScanStatus = new ScanStatus(uiDispatcherService, "File Scan");
        DuplicateFileAnalysisStatus = new ScanStatus(uiDispatcherService, "Duplicate File Analysis");
        OrphanedFileScanStatus = new ScanStatus(uiDispatcherService, "Orphaned File Scan");
    }

    /// <inheritdoc />
    public IScanStatus FolderScanStatus { get; }

    /// <inheritdoc />
    public IScanStatus FileScanStatus { get; }

    /// <inheritdoc />
    public IScanStatus DuplicateFileAnalysisStatus { get; }

    /// <inheritdoc />
    public IScanStatus OrphanedFileScanStatus { get; }
}
