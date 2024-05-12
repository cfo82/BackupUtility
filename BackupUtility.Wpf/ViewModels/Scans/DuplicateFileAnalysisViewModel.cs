namespace BackupUtilities.Wpf.ViewModels.Scans;

using System;
using System.Windows.Input;
using BackupUtilities.Services.Interfaces;
using BackupUtilities.Services.Interfaces.Status;
using BackupUtilities.Wpf.Views.Scans;
using Prism.Commands;
using Prism.Mvvm;

/// <summary>
/// The view model for the duplicate file analysis step inside the <see cref="ScanView"/>.
/// </summary>
public class DuplicateFileAnalysisViewModel : BindableBase
{
    private readonly IScanStatusManager _longRunningOperationManager;
    private readonly IErrorHandler _errorHandler;
    private readonly IDuplicateFileAnalysis _duplicateFileAnalysis;
    private bool _isRunButtonEnabled;
    private string _progressText;
    private bool _isProgressBarIndeterminate;
    private double _progress;

    /// <summary>
    /// Initializes a new instance of the <see cref="DuplicateFileAnalysisViewModel"/> class.
    /// </summary>
    /// <param name="longRunningOperationManager">The long running operation manager.</param>
    /// <param name="errorHandler">The error handler.</param>
    /// <param name="duplicateFileAnalysis">The duplicate file analysis.</param>
    public DuplicateFileAnalysisViewModel(
        IScanStatusManager longRunningOperationManager,
        IErrorHandler errorHandler,
        IDuplicateFileAnalysis duplicateFileAnalysis)
    {
        _longRunningOperationManager = longRunningOperationManager;
        _errorHandler = errorHandler;
        _duplicateFileAnalysis = duplicateFileAnalysis;

        _isRunButtonEnabled = true;
        _progressText = "Not yet started"; // TODO: Read this value from the database (current scan)
        _isProgressBarIndeterminate = false;

        _longRunningOperationManager.Changed += OnLongRunningOperationChanged;

        RunDuplicateFileAnalysisCommand = new DelegateCommand(OnRunDuplicateFileAnalysis);
    }

    /// <summary>
    /// Gets a command to run the duplicate file analysis.
    /// </summary>
    public ICommand RunDuplicateFileAnalysisCommand { get; }

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
        var operationStatus = _longRunningOperationManager.FullScanStatus.DuplicateFileAnalysisStatus;
        ProgressText = operationStatus.Text;
        IsProgressBarIndeterminate = operationStatus.Progress == null && operationStatus.IsRunning;
        Progress = operationStatus.Progress.HasValue ? operationStatus.Progress.Value : 0.0;
    }

    private async void OnRunDuplicateFileAnalysis()
    {
        try
        {
            if (_longRunningOperationManager.IsRunning)
            {
                return;
            }

            await _duplicateFileAnalysis.RunDuplicateFileAnalysis();
        }
        catch (Exception e)
        {
            _errorHandler.Error = e;
        }
    }
}
