namespace BackupUtilities.Wpf.ViewModels.Scans;

using BackupUtilities.Services.Interfaces;
using BackupUtilities.Services.Interfaces.Status;
using BackupUtilities.Wpf.Views.Scans;

/// <summary>
/// View model for <see cref="SimpleScanView"/>.
/// </summary>
public class AdvancedScanViewModel : ScanViewModelBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AdvancedScanViewModel"/> class.
    /// </summary>
    /// <param name="longRunningOperationManager">The long running operation manager.</param>
    /// <param name="errorHandler">The error handler.</param>
    /// <param name="folderEnumerator">The folder enumerator.</param>
    /// <param name="fileEnumerator">The file enumerator.</param>
    /// <param name="duplicateFileAnalysis">The duplicate file analysis.</param>
    /// <param name="orphanedFileEnumerator">The enumerator to search for orphaned files.</param>
    public AdvancedScanViewModel(
        IScanStatusManager longRunningOperationManager,
        IErrorHandler errorHandler,
        IFolderEnumerator folderEnumerator,
        IFileEnumerator fileEnumerator,
        IDuplicateFileAnalysis duplicateFileAnalysis,
        IOrphanedFileEnumerator orphanedFileEnumerator)
        : base(longRunningOperationManager, errorHandler, folderEnumerator, fileEnumerator, duplicateFileAnalysis, orphanedFileEnumerator)
    {
    }
}
