namespace BackupUtilities.Wpf.ViewModels;

using System;
using BackupUtilities.Services.Interfaces;
using BackupUtilities.Wpf.Views;
using Prism.Mvvm;

/// <summary>
/// The view model for <see cref="MainWindow"/>.
/// </summary>
public class MainWindowViewModel : BindableBase
{
    private readonly IProjectManager _projectManager;
    private readonly ILongRunningOperationManager _longRunningOperationManager;
    private IBackupProject? _currentProject;
    private bool _isReady = false;
    private int _selectedTabIndex = 0;
    private bool _isLongRunningOperationInProgress = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindowViewModel"/> class.
    /// </summary>
    /// <param name="projectManager">The project manager.</param>
    /// <param name="longRunningOperationManager">The long running operation manager.</param>
    public MainWindowViewModel(
        IProjectManager projectManager,
        ILongRunningOperationManager longRunningOperationManager)
    {
        _projectManager = projectManager;
        _longRunningOperationManager = longRunningOperationManager;

        _projectManager.CurrentProjectChanged += OnCurrentProjectChanged;
        OnCurrentProjectChanged(null, EventArgs.Empty);

        _longRunningOperationManager.OperationChanged += OnLongRunningOperationChanged;
        OnLongRunningOperationChanged(null, EventArgs.Empty);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the project is ready to be run.
    /// </summary>
    public bool IsReady
    {
        get { return _isReady; }
        set { SetProperty(ref _isReady, value); }
    }

    /// <summary>
    /// Gets or sets a value defining which tab has been selected.
    /// </summary>
    public int SelectedTabIndex
    {
        get { return _selectedTabIndex; }
        set { SetProperty(ref _selectedTabIndex, value); }
    }

    /// <summary>
    /// Gets or sets a value indicating whether a long running operation is in progress.
    /// </summary>
    public bool IsLongRunningOperationInProgress
    {
        get { return _isLongRunningOperationInProgress; }
        set { SetProperty(ref _isLongRunningOperationInProgress, value); }
    }

    private void OnCurrentProjectChanged(object? sender, EventArgs e)
    {
        if (_currentProject != null)
        {
            _currentProject.IsReadyChanged -= OnIsReadyChanged;
        }

        _currentProject = _projectManager.CurrentProject;

        if (_currentProject != null)
        {
            _currentProject.IsReadyChanged += OnIsReadyChanged;
        }

        OnIsReadyChanged(null, EventArgs.Empty);
    }

    private void OnIsReadyChanged(object? sender, EventArgs e)
    {
        IsReady = _currentProject != null && _currentProject.IsReady;
        if (!IsReady)
        {
            SelectedTabIndex = 0; // zero is defined to be the settings.
        }
    }

    private void OnLongRunningOperationChanged(object? sender, EventArgs e)
    {
        IsLongRunningOperationInProgress = _longRunningOperationManager.IsRunning;
    }
}
