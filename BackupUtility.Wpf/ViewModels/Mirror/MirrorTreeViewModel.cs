namespace BackupUtilities.Wpf.ViewModels.Mirror;

using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Threading.Tasks;
using BackupUtilities.Data.Interfaces;
using BackupUtilities.Services.Interfaces;
using BackupUtilities.Wpf.Contracts;
using BackupUtilities.Wpf.ViewModels.Working;
using BackupUtilities.Wpf.Views.Mirror;
using Microsoft.Extensions.Logging;
using Prism.Mvvm;

/// <summary>
/// View model for <see cref="MirrorTreeView"/>.
/// </summary>
public class MirrorTreeViewModel : BindableBase
{
    private readonly ILogger<FolderTreeViewModel> _logger;
    private readonly ISelectedFolderService _selectedFolderService;
    private readonly IProjectManager _projectManager;
    private IBackupProject? _currentProject;

    /// <summary>
    /// Initializes a new instance of the <see cref="MirrorTreeViewModel"/> class.
    /// </summary>
    /// <param name="logger">The logger to use.</param>
    /// <param name="selectedFolderService">The service to manage the currently selected folder.</param>
    /// <param name="projectManager">Manages the current project.</param>
    public MirrorTreeViewModel(
        ILogger<FolderTreeViewModel> logger,
        ISelectedFolderService selectedFolderService,
        IProjectManager projectManager)
    {
        _logger = logger;
        _selectedFolderService = selectedFolderService;
        _projectManager = projectManager;

        TopLevelItems = new ObservableCollection<MirrorTreeViewItemViewModel>();

        _projectManager.CurrentProjectChanged += OnCurrentProjectChanged;
        OnCurrentProjectChanged(null, EventArgs.Empty);
    }

    /// <summary>
    /// Gets the top level items of the folder tree.
    /// </summary>
    public ObservableCollection<MirrorTreeViewItemViewModel> TopLevelItems { get; }

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

            var folderRepository = _currentProject.Data.FolderRepository;

            TopLevelItems.Clear();
            var rootFolder = await folderRepository.GetRootFolders(DriveType.Mirror);
            foreach (var folder in rootFolder)
            {
                await InsertNodeAsync(folderRepository, null, folder);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while initializing the view model.");
        }
    }

    private async Task InsertNodeAsync(
        IFolderRepository folderRepository,
        MirrorTreeViewItemViewModel? parent,
        Folder folder)
    {
        var node = new MirrorTreeViewItemViewModel(_selectedFolderService, folder);

        var subFolders = await folderRepository.GetSubFoldersAsync(folder);
        foreach (var subFolder in subFolders)
        {
            await InsertNodeAsync(folderRepository, node, subFolder);
        }

        if (parent != null)
        {
            parent.Children.Add(node);
        }
        else
        {
            TopLevelItems.Add(node);
        }
    }
}
