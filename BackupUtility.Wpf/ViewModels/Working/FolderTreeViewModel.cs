namespace BackupUtilities.Wpf.ViewModels.Working;

using System;
using System.Collections.ObjectModel;
using BackupUtilities.Services.Interfaces;
using BackupUtilities.Wpf.Contracts;
using BackupUtilities.Wpf.Views;
using Microsoft.Extensions.Logging;
using Prism.Mvvm;

/// <summary>
/// The view model for <see cref="FolderTreeView"/>.
/// </summary>
public class FolderTreeViewModel : BindableBase
{
    private readonly ILogger<FolderTreeViewModel> _logger;
    private readonly ISelectedFolderService _selectedFolderService;
    private readonly IProjectManager _projectManager;
    private IBackupProject? _currentProject;

    /// <summary>
    /// Initializes a new instance of the <see cref="FolderTreeViewModel"/> class.
    /// </summary>
    /// <param name="logger">The logger to use.</param>
    /// <param name="selectedFolderService">The service to manage the currently selected folder.</param>
    /// <param name="projectManager">Manages the current project.</param>
    public FolderTreeViewModel(
        ILogger<FolderTreeViewModel> logger,
        ISelectedFolderService selectedFolderService,
        IProjectManager projectManager)
    {
        _logger = logger;
        _selectedFolderService = selectedFolderService;
        _projectManager = projectManager;

        TopLevelItems = new ObservableCollection<TreeViewItemViewModel>();

        _projectManager.CurrentProjectChanged += OnCurrentProjectChanged;
        OnCurrentProjectChanged(null, EventArgs.Empty);
    }

    /// <summary>
    /// Gets the top level items of the folder tree.
    /// </summary>
    public ObservableCollection<TreeViewItemViewModel> TopLevelItems { get; }

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

    private async void OnIsReadyChanged(object? sender, EventArgs e)
    {
        try
        {
            if (_currentProject == null || !_currentProject.IsReady)
            {
                return;
            }

            var connection = _currentProject.Data.Connection;
            var settingsRepository = _currentProject.Data.SettingsRepository;
            var folderRepository = _currentProject.Data.FolderRepository;

            var settings = await settingsRepository.GetSettingsAsync(connection, null);
            var rootFolder = await folderRepository.GetFolderAsync(connection, settings.RootPath);
            if (rootFolder != null)
            {
                TopLevelItems.Clear();
                TopLevelItems.Add(new TreeViewItemViewModel(_selectedFolderService, _currentProject.Data, rootFolder, null));
            }

            _selectedFolderService.SelectedFolder = rootFolder;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while initializing the view model.");
        }
    }
}
