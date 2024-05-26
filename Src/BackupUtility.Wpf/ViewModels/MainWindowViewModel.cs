namespace BackupUtilities.Wpf.ViewModels;

using System;
using BackupUtilities.Services.Interfaces;
using BackupUtilities.Wpf.Contracts;
using BackupUtilities.Wpf.Views;
using Prism.Mvvm;

/// <summary>
/// The view model for <see cref="MainWindow"/>.
/// </summary>
public class MainWindowViewModel : BindableBase
{
    private readonly IProjectManager _projectManager;
    private readonly ISelectedFolderService _selectedFolderService;
    private IBackupProject? _currentProject;
    private IScan? _currentScan;
    private bool _isReady = false;
    private bool _isDuplicateFileAnalysisFinished = false;
    private bool _isOrphanedFileScanFinished = false;
    private int _selectedTabIndex = 0;

    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindowViewModel"/> class.
    /// </summary>
    /// <param name="projectManager">The project manager.</param>
    /// <param name="selectedFolderService">The folder selection service.</param>
    public MainWindowViewModel(
        IProjectManager projectManager,
        ISelectedFolderService selectedFolderService)
    {
        _projectManager = projectManager;
        _selectedFolderService = selectedFolderService;

        _projectManager.CurrentProjectChanged += OnCurrentProjectChanged;
        _selectedFolderService.SelectedFolderChanged += OnSelectedFolderChanged;

        OnCurrentProjectChanged(null, EventArgs.Empty);
        OnCurrentScanChanged(null, EventArgs.Empty);
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
    /// Gets or sets a value indicating whether the duplicate file analysis has finished. Meaning that the
    /// working tree view can be enabled.
    /// </summary>
    public bool IsDuplicateFileAnalysisFinished
    {
        get { return _isDuplicateFileAnalysisFinished; }
        set { SetProperty(ref _isDuplicateFileAnalysisFinished, value); }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the orphaned file scan has finished. Meaning that the
    /// mirror tree view can be enabled.
    /// </summary>
    public bool IsOrphanedFileScanFinished
    {
        get { return _isOrphanedFileScanFinished; }
        set { SetProperty(ref _isOrphanedFileScanFinished, value); }
    }

    /// <summary>
    /// Gets or sets a value defining which tab has been selected.
    /// </summary>
    public int SelectedTabIndex
    {
        get { return _selectedTabIndex; }
        set { SetProperty(ref _selectedTabIndex, value); }
    }

    private void OnCurrentProjectChanged(object? sender, EventArgs e)
    {
        if (_currentProject != null)
        {
            _currentProject.IsReadyChanged -= OnIsReadyChanged;
            _currentProject.CurrentScanChanged -= OnCurrentScanChanged;
        }

        _currentProject = _projectManager.CurrentProject;

        if (_currentProject != null)
        {
            _currentProject.IsReadyChanged += OnIsReadyChanged;
            _currentProject.CurrentScanChanged += OnCurrentScanChanged;
        }

        OnIsReadyChanged(null, EventArgs.Empty);
        OnCurrentScanChanged(null, EventArgs.Empty);
    }

    private void OnIsReadyChanged(object? sender, EventArgs e)
    {
        UpdateState();
    }

    private void OnCurrentScanChanged(object? sender, EventArgs e)
    {
        if (_currentScan != null)
        {
            _currentScan.Changed -= OnCurrentScanDataChanged;
        }

        _currentScan = _currentProject?.CurrentScan;

        if (_currentScan != null)
        {
            _currentScan.Changed += OnCurrentScanDataChanged;
        }

        OnCurrentScanDataChanged(null, EventArgs.Empty);
    }

    private void OnCurrentScanDataChanged(object? sender, EventArgs e)
    {
        UpdateState();
    }

    private void UpdateState()
    {
        IsReady = _currentProject != null && _currentProject.IsReady;
        if (!IsReady)
        {
            SelectedTabIndex = 0; // zero is defined to be the settings.
        }

        IsDuplicateFileAnalysisFinished = _currentScan?.Data.StageDuplicateFileAnalysisFinished ?? false;
        if (!IsDuplicateFileAnalysisFinished && SelectedTabIndex == 2)
        {
            SelectedTabIndex = 0;
        }

        IsOrphanedFileScanFinished = _currentScan?.Data.StageOrphanedFileEnumerationFinished ?? false;
        if (!IsOrphanedFileScanFinished && SelectedTabIndex == 3)
        {
            SelectedTabIndex = 0;
        }
    }

    private void OnSelectedFolderChanged(object? sender, SelectedFolderChangedEventArgs e)
    {
        SelectedTabIndex = 2;
    }
}
