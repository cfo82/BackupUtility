namespace BackupUtilities.Wpf.ViewModels;

using System;
using System.Windows.Input;
using BackupUtilities.Services.Interfaces;
using BackupUtilities.Wpf.Views;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Mvvm;

/// <summary>
/// The view model for <see cref="ToolBarView"/>.
/// </summary>
public class ToolBarViewModel : BindableBase
{
    private readonly ILogger<ToolBarViewModel> _logger;
    private readonly IProjectManager _projectManager;
    private readonly IFolderEnumerator _folderEnumerator;
    private readonly IFileEnumerator _fileEnumerator;
    private readonly IDuplicateFileAnalysis _duplicateFileAnalysis;
    private readonly IOrphanedFileEnumerator _orphanedFileEnumerator;
    private IBackupProject? _currentProject;
    private bool _isReady = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="ToolBarViewModel"/> class.
    /// </summary>
    /// <param name="logger">The logger to use.</param>
    /// <param name="projectManager">The project manager.</param>
    /// <param name="folderEnumerator">The folder enumerator.</param>
    /// <param name="fileEnumerator">The file enumerator.</param>
    /// <param name="duplicateFileAnalysis">The duplicate file analysis.</param>
    /// <param name="orphanedFileEnumerator">The enumerator to search for orphaned files.</param>
    public ToolBarViewModel(
        ILogger<ToolBarViewModel> logger,
        IProjectManager projectManager,
        IFolderEnumerator folderEnumerator,
        IFileEnumerator fileEnumerator,
        IDuplicateFileAnalysis duplicateFileAnalysis,
        IOrphanedFileEnumerator orphanedFileEnumerator)
    {
        _logger = logger;
        _projectManager = projectManager;
        _folderEnumerator = folderEnumerator;
        _fileEnumerator = fileEnumerator;
        _duplicateFileAnalysis = duplicateFileAnalysis;
        _orphanedFileEnumerator = orphanedFileEnumerator;

        OpenCommand = new DelegateCommand(OnOpen);
        CreateCommand = new DelegateCommand(OnCreate);
        ScanFoldersCommand = new DelegateCommand(OnScanFolders);
        ScanFilesCommand = new DelegateCommand(OnScanFiles);
        RunDuplicateFileAnalysisCommand = new DelegateCommand(OnRunDuplicateFileAnalysis);
        SearchOrphanedFilesCommand = new DelegateCommand(OnSearchOrphanedFiles);

        _projectManager.CurrentProjectChanged += OnCurrentProjectChanged;
        OnCurrentProjectChanged(null, EventArgs.Empty);
    }

    /// <summary>
    /// Gets a command to open an existing backup project.
    /// </summary>
    public ICommand OpenCommand { get; private set; }

    /// <summary>
    /// Gets a command to create a new backup project.
    /// </summary>
    public ICommand CreateCommand { get; private set; }

    /// <summary>
    /// Gets a command to scan the folders.
    /// </summary>
    public ICommand ScanFoldersCommand { get; private set; }

    /// <summary>
    /// Gets a command to scan the files.
    /// </summary>
    public ICommand ScanFilesCommand { get; private set; }

    /// <summary>
    /// Gets a command to run the duplicate files analysis.
    /// </summary>
    public ICommand RunDuplicateFileAnalysisCommand { get; private set; }

    /// <summary>
    /// Gets the command to search orphaned files on the mirror drive.
    /// </summary>
    public ICommand SearchOrphanedFilesCommand { get; private set; }

    /// <summary>
    /// Gets or sets a value indicating whether the project is ready to be run.
    /// </summary>
    public bool IsReady
    {
        get { return _isReady; }
        set { SetProperty(ref _isReady, value); }
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
    }

    private async void OnOpen()
    {
        try
        {
            var dialog = new OpenFileDialog();
            dialog.FileName = string.Empty;
            dialog.DefaultExt = ".bproj";
            dialog.Filter = "Backup projects (.bproj)|*.bproj";

            var result = dialog.ShowDialog();

            if (result ?? false)
            {
                await _projectManager.OpenProjectAsync(dialog.FileName);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while enumerating folders.");
        }
    }

    private async void OnCreate()
    {
        try
        {
            var dialog = new SaveFileDialog();
            dialog.FileName = string.Empty;
            dialog.DefaultExt = ".bproj";
            dialog.Filter = "Backup projects (.bproj)|*.bproj";

            var result = dialog.ShowDialog();

            if (result ?? false)
            {
                await _projectManager.CreateProjectAsync(dialog.FileName);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while enumerating folders.");
        }
    }

    private async void OnScanFolders()
    {
        try
        {
            await _folderEnumerator.EnumerateFoldersAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while enumerating folders.");
        }
    }

    private async void OnScanFiles()
    {
        try
        {
            await _fileEnumerator.EnumerateFilesAsync(true);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while enumerating folders.");
        }
    }

    private async void OnRunDuplicateFileAnalysis()
    {
        try
        {
            await _duplicateFileAnalysis.RunDuplicateFileAnalysis();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while enumerating folders.");
        }
    }

    private async void OnSearchOrphanedFiles()
    {
        try
        {
            await _orphanedFileEnumerator.EnumerateOrphanedFilesAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while enumerating folders.");
        }
    }
}
