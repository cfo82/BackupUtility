namespace BackupUtilities.Wpf.ViewModels.Working;

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using BackupUtilities.Data.Interfaces;
using BackupUtilities.Data.Repositories;
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
    private readonly IErrorHandler _errorHandler;
    private readonly ISelectedFolderService _selectedFolderService;
    private readonly IProjectManager _projectManager;
    private IBackupProject? _currentProject;

    /// <summary>
    /// Initializes a new instance of the <see cref="FolderTreeViewModel"/> class.
    /// </summary>
    /// <param name="logger">The logger to use.</param>
    /// <param name="errorHandler">The error handler.</param>
    /// <param name="selectedFolderService">The service to manage the currently selected folder.</param>
    /// <param name="projectManager">Manages the current project.</param>
    public FolderTreeViewModel(
        ILogger<FolderTreeViewModel> logger,
        IErrorHandler errorHandler,
        ISelectedFolderService selectedFolderService,
        IProjectManager projectManager)
    {
        _logger = logger;
        _errorHandler = errorHandler;
        _selectedFolderService = selectedFolderService;
        _projectManager = projectManager;

        TopLevelItems = new ObservableCollection<TreeViewItemViewModel>();

        _selectedFolderService.SelectedFolderChanged += OnSelectedFolderChanged;

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

            var settingsRepository = _currentProject.Data.SettingsRepository;
            var folderRepository = _currentProject.Data.FolderRepository;

            var settings = await settingsRepository.GetSettingsAsync(null);
            var rootFolder = await folderRepository.GetFolderAsync(settings.RootPath);
            if (rootFolder != null)
            {
                TopLevelItems.Clear();
                TopLevelItems.Add(new TreeViewItemViewModel(_errorHandler, _selectedFolderService, _currentProject.Data, rootFolder, null));
            }

            _selectedFolderService.SelectedFolder = rootFolder;
        }
        catch (Exception ex)
        {
            _errorHandler.Error = ex;
        }
    }

    private async void OnSelectedFolderChanged(object? sender, SelectedFolderChangedEventArgs e)
    {
        try
        {
            if (_currentProject == null || !_currentProject.IsReady)
            {
                return;
            }

            var selectedFolder = _selectedFolderService.SelectedFolder;
            if (selectedFolder == null)
            {
                return;
            }

            var folderRepository = _currentProject.Data.FolderRepository;

            var fullPath = await folderRepository.GetFullPathForFolderAsync(selectedFolder);
            if (!fullPath.Any())
            {
                return;
            }

            var previouslySelectedItem = await FindTreeViewItemForFolderAsync(folderRepository, e.PreviousSelection, f => { });
            var newlySelectedItem = await FindTreeViewItemForFolderAsync(folderRepository, e.NewSelection, f => f.IsExpanded = true);

            try
            {
                _selectedFolderService.FireEvents = false;

                if (previouslySelectedItem != null)
                {
                    previouslySelectedItem.IsSelected = false;
                }

                if (newlySelectedItem != null)
                {
                    newlySelectedItem.IsSelected = true;
                }
            }
            finally
            {
                _selectedFolderService.FireEvents = true;
            }
        }
        catch (Exception ex)
        {
            _errorHandler.Error = ex;
        }
    }

    private async Task<TreeViewItemViewModel?> FindTreeViewItemForFolderAsync(
        IFolderRepository folderRepository,
        Folder? folder,
        Action<TreeViewItemViewModel> folderAction)
    {
        if (folder == null)
        {
            return null;
        }

        var fullPath = await folderRepository.GetFullPathForFolderAsync(folder);
        if (!fullPath.Any())
        {
            return null;
        }

        TreeViewItemViewModel? currentFolder = null;
        var last = fullPath.Last();
        foreach (var element in fullPath)
        {
            var collection = currentFolder == null ? TopLevelItems : currentFolder.Children;
            currentFolder = collection.FirstOrDefault(f => f.Id == element.Id);
            if (currentFolder == null)
            {
                return null;
            }

            if (element != last)
            {
                folderAction.Invoke(currentFolder);
            }

            await currentFolder.IsFilledCompletionSource.Task;
        }

        return currentFolder;
    }
}
