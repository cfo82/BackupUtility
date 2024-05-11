namespace BackupUtilities.Wpf.ViewModels.Scans;

using System;
using System.Globalization;
using System.Windows.Input;
using BackupUtilities.Services.Interfaces;
using BackupUtilities.Wpf.Views.Scans;
using Prism.Commands;
using Prism.Mvvm;

/// <summary>
/// View model for <see cref="SimpleScanView"/>.
/// </summary>
public class SimpleScanViewModel : BindableBase
{
    private readonly IScanStatusManager _longRunningOperationManager;
    private readonly IErrorHandler _errorHandler;
    private readonly IProjectManager _projectManager;
    private readonly IFolderEnumerator _folderEnumerator;
    private readonly IFileEnumerator _fileEnumerator;
    private readonly IDuplicateFileAnalysis _duplicateFileAnalysis;
    private readonly IOrphanedFileEnumerator _orphanedFileEnumerator;
    private IBackupProject? _currentProject;
    private string _scanTitle;
    private string _settingsWorkingDrive;
    private string _settingsMirrorDrive;

    /// <summary>
    /// Initializes a new instance of the <see cref="SimpleScanViewModel"/> class.
    /// </summary>
    /// <param name="longRunningOperationManager">The long running operation manager.</param>
    /// <param name="errorHandler">The error handler.</param>
    /// <param name="projectManager">The project manager.</param>
    /// <param name="folderEnumerator">The folder enumerator.</param>
    /// <param name="fileEnumerator">The file enumerator.</param>
    /// <param name="duplicateFileAnalysis">The duplicate file analysis.</param>
    /// <param name="orphanedFileEnumerator">The enumerator to search for orphaned files.</param>
    public SimpleScanViewModel(
        IScanStatusManager longRunningOperationManager,
        IErrorHandler errorHandler,
        IProjectManager projectManager,
        IFolderEnumerator folderEnumerator,
        IFileEnumerator fileEnumerator,
        IDuplicateFileAnalysis duplicateFileAnalysis,
        IOrphanedFileEnumerator orphanedFileEnumerator)
    {
        _longRunningOperationManager = longRunningOperationManager;
        _errorHandler = errorHandler;
        _projectManager = projectManager;
        _folderEnumerator = folderEnumerator;
        _fileEnumerator = fileEnumerator;
        _duplicateFileAnalysis = duplicateFileAnalysis;
        _orphanedFileEnumerator = orphanedFileEnumerator;

        _currentProject = null;
        _scanTitle = string.Empty;
        _settingsWorkingDrive = string.Empty;
        _settingsMirrorDrive = string.Empty;

        _projectManager.CurrentProjectChanged += OnCurrentProjectChanged;

        FolderScanViewModel = new FolderScanViewModel(_longRunningOperationManager, _errorHandler, _folderEnumerator);
        FileScanViewModel = new FileScanViewModel(_longRunningOperationManager, _errorHandler, _fileEnumerator);
        DuplicateFileAnalysisViewModel = new DuplicateFileAnalysisViewModel(_longRunningOperationManager, _errorHandler, _duplicateFileAnalysis);
        OrphanedFileScanViewModel = new OrphanedFileScanViewModel(_longRunningOperationManager, _errorHandler, _orphanedFileEnumerator);

        OnCurrentProjectChanged(_projectManager, EventArgs.Empty);
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

    /// <summary>
    /// Gets or sets the current scan title.
    /// </summary>
    public string ScanTitle
    {
        get { return _scanTitle; }
        set { SetProperty(ref _scanTitle, value); }
    }

    /// <summary>
    /// Gets or sets the working drive settings.
    /// </summary>
    public string SettingsWorkingDrive
    {
        get { return _settingsWorkingDrive; }
        set { SetProperty(ref _settingsWorkingDrive, value); }
    }

    /// <summary>
    /// Gets or sets the mirror drive settings.
    /// </summary>
    public string SettingsMirrorDrive
    {
        get { return _settingsMirrorDrive; }
        set { SetProperty(ref _settingsMirrorDrive, value); }
    }

    private void OnCurrentProjectChanged(object? sender, EventArgs e)
    {
        if (_currentProject != null)
        {
            _currentProject.CurrentScanChanged -= OnCurrentScanChanged;
        }

        _currentProject = _projectManager.CurrentProject;

        if (_currentProject != null)
        {
            _currentProject.CurrentScanChanged += OnCurrentScanChanged;
        }

        OnCurrentScanChanged(null, EventArgs.Empty);
    }

    private void OnCurrentScanChanged(object? sender, EventArgs e)
    {
        var currentScan = _currentProject?.CurrentScan;

        if (currentScan == null)
        {
            ScanTitle = "No scan available.";
            SettingsWorkingDrive = string.Empty;
            SettingsMirrorDrive = string.Empty;
        }
        else
        {
            var date = currentScan.Data.CreatedDate;
            date = date.ToLocalTime();
            var dateString = date.ToString(CultureInfo.CurrentUICulture.DateTimeFormat.ShortDatePattern, CultureInfo.CurrentUICulture);
            var timeString = date.ToString(CultureInfo.CurrentUICulture.DateTimeFormat.ShortTimePattern, CultureInfo.CurrentUICulture);
            ScanTitle = $"Scan {dateString} {timeString}";
            SettingsWorkingDrive = currentScan.Settings.RootPath;
            SettingsMirrorDrive = currentScan.Settings.MirrorPath;
        }
    }
}
