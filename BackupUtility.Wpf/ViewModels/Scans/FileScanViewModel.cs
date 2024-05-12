namespace BackupUtilities.Wpf.ViewModels.Scans;

using System;
using System.Windows.Input;
using BackupUtilities.Services.Interfaces;
using BackupUtilities.Services.Interfaces.Scans;
using BackupUtilities.Services.Interfaces.Status;
using BackupUtilities.Wpf.Views.Scans;
using Prism.Commands;
using Prism.Mvvm;

/// <summary>
/// The view model for the file scan step inside the <see cref="ScanView"/>.
/// </summary>
public class FileScanViewModel : BindableBase
{
    private readonly IScanStatusManager _longRunningOperationManager;
    private readonly IErrorHandler _errorHandler;
    private readonly IFileEnumerator _fileEnumerator;
    private bool _isRunButtonEnabled;
    private string _progressText;
    private bool _isProgressBarIndeterminate;
    private double _progress;
    private string _folderProgressText;
    private double _folderProgress;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileScanViewModel"/> class.
    /// </summary>
    /// <param name="longRunningOperationManager">The long running operation manager.</param>
    /// <param name="errorHandler">The error handler.</param>
    /// <param name="fileEnumerator">The file enumerator.</param>
    public FileScanViewModel(
        IScanStatusManager longRunningOperationManager,
        IErrorHandler errorHandler,
        IFileEnumerator fileEnumerator)
    {
        _longRunningOperationManager = longRunningOperationManager;
        _errorHandler = errorHandler;
        _fileEnumerator = fileEnumerator;

        _isRunButtonEnabled = true;
        _progressText = "Not yet started"; // TODO: Read this value from the database (current scan)
        _isProgressBarIndeterminate = false;
        _folderProgressText = string.Empty;
        _folderProgress = 0;

        _longRunningOperationManager.Changed += OnLongRunningOperationChanged;

        RunFileScanCommand = new DelegateCommand(OnRunFileScan);
        ContinueCancelledFileScanCommand = new DelegateCommand(OnContinueCancelledFileScan);
        RescanKnownFilesCommand = new DelegateCommand(OnRescanKnownFiles);
    }

    /// <summary>
    /// Gets a command to run the file scan.
    /// </summary>
    public ICommand RunFileScanCommand { get; }

    /// <summary>
    /// Gets a command to run a cancelled file scan.
    /// </summary>
    public ICommand ContinueCancelledFileScanCommand { get; }

    /// <summary>
    /// Gets a command to rescan known files.
    /// </summary>
    public ICommand RescanKnownFilesCommand { get; }

    /// <summary>
    /// Gets or sets a value indicating whether the run button should be enabled.
    /// </summary>
    public bool IsRunButtonEnabled
    {
        get { return _isRunButtonEnabled; }
        set { SetProperty(ref _isRunButtonEnabled, value); }
    }

    /// <summary>
    /// Gets or sets a text indicating what kind of progress is happening.
    /// </summary>
    public string ProgressText
    {
        get { return _progressText; }
        set { SetProperty(ref _progressText, value); }
    }

    /// <summary>
    /// Gets or sets a value indicating whether an indeterminate progress bar should be displayed.
    /// </summary>
    public bool IsProgressBarIndeterminate
    {
        get { return _isProgressBarIndeterminate; }
        set { SetProperty(ref _isProgressBarIndeterminate, value); }
    }

    /// <summary>
    /// Gets or sets the progress running this operation.
    /// </summary>
    public double Progress
    {
        get { return _progress; }
        set { SetProperty(ref _progress, value); }
    }

    /// <summary>
    /// Gets or sets a text indicating the progress of enumerating the current folder.
    /// </summary>
    public string FolderProgressText
    {
        get { return _folderProgressText; }
        set { SetProperty(ref _folderProgressText, value); }
    }

    /// <summary>
    /// Gets or sets the progress of enumerating the current folder.
    /// </summary>
    public double FolderProgress
    {
        get { return _folderProgress; }
        set { SetProperty(ref _folderProgress, value); }
    }

    private void OnLongRunningOperationChanged(object? sender, EventArgs e)
    {
        IsRunButtonEnabled = !_longRunningOperationManager.IsRunning;
        var operationStatus = _longRunningOperationManager.FullScanStatus.FileScanStatus;
        ProgressText = operationStatus.Text;
        IsProgressBarIndeterminate = operationStatus.Progress == null && operationStatus.IsRunning;
        Progress = operationStatus.Progress.HasValue ? operationStatus.Progress.Value : 0.0;
        FolderProgressText = operationStatus.FolderEnumerationText;
        FolderProgress = operationStatus.FolderEnumerationProgress;
    }

    private async void OnRunFileScan()
    {
        try
        {
            if (_longRunningOperationManager.IsRunning)
            {
                return;
            }

            await _fileEnumerator.EnumerateFilesAsync(false);
        }
        catch (Exception e)
        {
            _errorHandler.Error = e;
        }
    }

    private async void OnContinueCancelledFileScan()
    {
        try
        {
            if (_longRunningOperationManager.IsRunning)
            {
                return;
            }

            await _fileEnumerator.EnumerateFilesAsync(true);
        }
        catch (Exception e)
        {
            _errorHandler.Error = e;
        }
    }

    private void OnRescanKnownFiles()
    {
    }
}
