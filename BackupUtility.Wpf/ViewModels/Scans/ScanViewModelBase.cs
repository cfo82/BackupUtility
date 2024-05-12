namespace BackupUtilities.Wpf.ViewModels.Scans;

using BackupUtilities.Services.Interfaces;
using BackupUtilities.Services.Interfaces.Scans;
using BackupUtilities.Services.Interfaces.Status;
using BackupUtilities.Wpf.Views.Scans;
using Prism.Mvvm;

/// <summary>
/// View model for <see cref="SimpleScanView"/>.
/// </summary>
public class ScanViewModelBase : BindableBase
{
    private readonly IScanStatusManager _longRunningOperationManager;
    private readonly IErrorHandler _errorHandler;
    private readonly IFolderEnumerator _folderEnumerator;
    private readonly IFileEnumerator _fileEnumerator;
    private readonly IDuplicateFileAnalysis _duplicateFileAnalysis;
    private readonly IOrphanedFileEnumerator _orphanedFileEnumerator;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScanViewModelBase"/> class.
    /// </summary>
    /// <param name="longRunningOperationManager">The long running operation manager.</param>
    /// <param name="errorHandler">The error handler.</param>
    /// <param name="folderEnumerator">The folder enumerator.</param>
    /// <param name="fileEnumerator">The file enumerator.</param>
    /// <param name="duplicateFileAnalysis">The duplicate file analysis.</param>
    /// <param name="orphanedFileEnumerator">The enumerator to search for orphaned files.</param>
    public ScanViewModelBase(
        IScanStatusManager longRunningOperationManager,
        IErrorHandler errorHandler,
        IFolderEnumerator folderEnumerator,
        IFileEnumerator fileEnumerator,
        IDuplicateFileAnalysis duplicateFileAnalysis,
        IOrphanedFileEnumerator orphanedFileEnumerator)
    {
        _longRunningOperationManager = longRunningOperationManager;
        _errorHandler = errorHandler;
        _folderEnumerator = folderEnumerator;
        _fileEnumerator = fileEnumerator;
        _duplicateFileAnalysis = duplicateFileAnalysis;
        _orphanedFileEnumerator = orphanedFileEnumerator;

        FolderScanViewModel = new FolderScanViewModel(_longRunningOperationManager, _errorHandler, _folderEnumerator);
        FileScanViewModel = new FileScanViewModel(_longRunningOperationManager, _errorHandler, _fileEnumerator);
        DuplicateFileAnalysisViewModel = new DuplicateFileAnalysisViewModel(_longRunningOperationManager, _errorHandler, _duplicateFileAnalysis);
        OrphanedFileScanViewModel = new OrphanedFileScanViewModel(_longRunningOperationManager, _errorHandler, _orphanedFileEnumerator);
    }

    /// <summary>
    /// Gets the view model for the folder scan step.
    /// </summary>
    public FolderScanViewModel FolderScanViewModel { get; }

    /// <summary>
    /// Gets the view model for the file scan step.
    /// </summary>
    public FileScanViewModel FileScanViewModel { get; }

    /// <summary>
    /// Gets the view model for the duplicate file analysis step.
    /// </summary>
    public DuplicateFileAnalysisViewModel DuplicateFileAnalysisViewModel { get; }

    /// <summary>
    /// Gets the view model for the orphaned file scan step.
    /// </summary>
    public OrphanedFileScanViewModel OrphanedFileScanViewModel { get; }
}
