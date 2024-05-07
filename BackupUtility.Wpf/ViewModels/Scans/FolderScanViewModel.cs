namespace BackupUtilities.Wpf.ViewModels.Scans;

using System;
using System.Windows.Input;
using BackupUtilities.Services.Interfaces;
using BackupUtilities.Wpf.Views.Scans;
using Prism.Commands;
using Prism.Mvvm;

/// <summary>
/// The view model for the folder scan step inside the <see cref="ScanView"/>.
/// </summary>
public class FolderScanViewModel : BindableBase
{
    private readonly ILongRunningOperationManager _longRunningOperationManager;
    private readonly IErrorHandler _errorHandler;
    private readonly IFolderEnumerator _folderEnumerator;
    private bool _isRunButtonEnabled;
    private string _progressText;
    private bool _isProgressBarIndeterminate;
    private double _progress;

    /// <summary>
    /// Initializes a new instance of the <see cref="FolderScanViewModel"/> class.
    /// </summary>
    /// <param name="longRunningOperationManager">The long running operation manager.</param>
    /// <param name="errorHandler">The error handler.</param>
    /// <param name="folderEnumerator">The folder enumerator.</param>
    public FolderScanViewModel(
        ILongRunningOperationManager longRunningOperationManager,
        IErrorHandler errorHandler,
        IFolderEnumerator folderEnumerator)
    {
        _longRunningOperationManager = longRunningOperationManager;
        _errorHandler = errorHandler;
        _folderEnumerator = folderEnumerator;

        _isRunButtonEnabled = true;
        _progressText = "Not yet started"; // TODO: Read this value from the database (current scan)
        _isProgressBarIndeterminate = false;

        _longRunningOperationManager.OperationChanged += OnLongRunningOperationChanged;

        RunFolderScanCommand = new DelegateCommand(OnRunFolderScan);
    }

    /// <summary>
    /// Gets a command to run the folder scan.
    /// </summary>
    public ICommand RunFolderScanCommand { get; }

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
        if (_longRunningOperationManager.ScanType == ScanType.FolderScan)
        {
            ProgressText = _longRunningOperationManager.Text;
            IsProgressBarIndeterminate = _longRunningOperationManager.Percentage == null && _longRunningOperationManager.IsRunning;
            Progress = _longRunningOperationManager.Percentage.HasValue ? _longRunningOperationManager.Percentage.Value : 0.0;
        }
    }

    private async void OnRunFolderScan()
    {
        try
        {
            if (_longRunningOperationManager.IsRunning)
            {
                return;
            }

            await _folderEnumerator.EnumerateFoldersAsync();
        }
        catch (Exception e)
        {
            _errorHandler.Error = e;
        }
    }
}
