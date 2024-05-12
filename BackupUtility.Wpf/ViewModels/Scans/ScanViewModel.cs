namespace BackupUtilities.Wpf.ViewModels.Scans;

using System;
using System.Windows.Input;
using BackupUtilities.Services.Interfaces;
using BackupUtilities.Services.Interfaces.Status;
using BackupUtilities.Wpf.Views.Scans;
using Prism.Commands;
using Prism.Mvvm;

/// <summary>
/// View model for <see cref="ScanView"/>.
/// </summary>
public class ScanViewModel : BindableBase
{
    private readonly IScanStatusManager _longRunningOperationManager;
    private readonly IErrorHandler _errorHandler;
    private readonly IProjectManager _projectManager;
    private readonly ICompleteScan _completeScan;
    private bool _areButtonsEnabled;
    private bool _showAdvancedStatusControls;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScanViewModel"/> class.
    /// </summary>
    /// <param name="longRunningOperationManager">The long running operation manager.</param>
    /// <param name="errorHandler">The error handler.</param>
    /// <param name="projectManager">The project manager.</param>
    /// <param name="completeScan">The complete scan operation.</param>
    public ScanViewModel(
        IScanStatusManager longRunningOperationManager,
        IErrorHandler errorHandler,
        IProjectManager projectManager,
        ICompleteScan completeScan)
    {
        _longRunningOperationManager = longRunningOperationManager;
        _errorHandler = errorHandler;
        _projectManager = projectManager;
        _completeScan = completeScan;

        _areButtonsEnabled = true;
        _showAdvancedStatusControls = false;

        _longRunningOperationManager.Changed += OnLongRunningOperationChanged;

        RunCompleteScanCommand = new DelegateCommand(OnRunCompleteScan);
    }

    /// <summary>
    /// Gets or sets a value indicating whether buttons should be enabled.
    /// </summary>
    public bool AreButtonsEnabled
    {
        get { return _areButtonsEnabled; }
        set { SetProperty(ref _areButtonsEnabled, value); }
    }

    /// <summary>
    /// Gets or sets a value indicating whether advanced status controls should be shown.
    /// </summary>
    public bool ShowAdvancedStatusControls
    {
        get { return _showAdvancedStatusControls; }
        set { SetProperty(ref _showAdvancedStatusControls, value); }
    }

    /// <summary>
    /// Gets the command to run a new scan.
    /// </summary>
    public ICommand RunCompleteScanCommand { get; }

    private void OnLongRunningOperationChanged(object? sender, EventArgs e)
    {
        AreButtonsEnabled = !_longRunningOperationManager.IsRunning;
    }

    private async void OnRunCompleteScan()
    {
        if (_longRunningOperationManager.IsRunning)
        {
            return;
        }

        if (_projectManager.CurrentProject == null || !_projectManager.CurrentProject.IsReady)
        {
            return;
        }

        try
        {
            await _projectManager.CurrentProject.CreateScanAsync();

            await _completeScan.RunAsync();
        }
        catch (Exception ex)
        {
            _errorHandler.Error = ex;
        }
    }
}
