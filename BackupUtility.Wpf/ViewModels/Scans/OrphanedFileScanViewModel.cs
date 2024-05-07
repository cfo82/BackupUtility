namespace BackupUtilities.Wpf.ViewModels.Scans;

using System;
using System.Windows.Input;
using BackupUtilities.Services.Interfaces;
using BackupUtilities.Wpf.Views.Scans;
using Prism.Commands;
using Prism.Mvvm;

/// <summary>
/// The view model for the orphaned file scan step inside the <see cref="ScanView"/>.
/// </summary>
public class OrphanedFileScanViewModel : BindableBase
{
    private readonly ILongRunningOperationManager _longRunningOperationManager;
    private readonly IErrorHandler _errorHandler;
    private readonly IOrphanedFileEnumerator _orphanedFileEnumerator;
    private bool _isRunButtonEnabled;
    private string _progressText;
    private bool _isProgressBarIndeterminate;
    private double _progress;

    /// <summary>
    /// Initializes a new instance of the <see cref="OrphanedFileScanViewModel"/> class.
    /// </summary>
    /// <param name="longRunningOperationManager">The long running operation manager.</param>
    /// <param name="errorHandler">The error handler.</param>
    /// <param name="orphanedFileEnumerator">The duplicate file analysis.</param>
    public OrphanedFileScanViewModel(
        ILongRunningOperationManager longRunningOperationManager,
        IErrorHandler errorHandler,
        IOrphanedFileEnumerator orphanedFileEnumerator)
    {
        _longRunningOperationManager = longRunningOperationManager;
        _errorHandler = errorHandler;
        _orphanedFileEnumerator = orphanedFileEnumerator;

        _isRunButtonEnabled = true;
        _progressText = "Not yet started"; // TODO: Read this value from the database (current scan)
        _isProgressBarIndeterminate = false;

        _longRunningOperationManager.OperationChanged += OnLongRunningOperationChanged;

        RunOrphanedFileScanCommand = new DelegateCommand(OnRunOrphanedFileScan);
    }

    /// <summary>
    /// Gets a command to run the orphaned file scan.
    /// </summary>
    public ICommand RunOrphanedFileScanCommand { get; }

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

    private void OnLongRunningOperationChanged(object? sender, EventArgs e)
    {
        IsRunButtonEnabled = !_longRunningOperationManager.IsRunning;
        if (_longRunningOperationManager.ScanType == ScanType.OrphanedFileScan)
        {
            ProgressText = _longRunningOperationManager.Text;
            IsProgressBarIndeterminate = _longRunningOperationManager.Percentage == null && _longRunningOperationManager.IsRunning;
            Progress = _longRunningOperationManager.Percentage.HasValue ? _longRunningOperationManager.Percentage.Value : 0.0;
        }
    }

    private async void OnRunOrphanedFileScan()
    {
        try
        {
            if (_longRunningOperationManager.IsRunning)
            {
                return;
            }

            await _orphanedFileEnumerator.EnumerateOrphanedFilesAsync();
        }
        catch (Exception e)
        {
            _errorHandler.Error = e;
        }
    }
}
