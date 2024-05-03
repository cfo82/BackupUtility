namespace BackupUtilities.Wpf.ViewModels.Mirror;

using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using BackupUtilities.Data.Interfaces;
using BackupUtilities.Services.Interfaces;
using BackupUtilities.Wpf.Contracts;
using BackupUtilities.Wpf.Views.Working;
using Prism.Mvvm;

/// <summary>
/// The view model for <see cref="FolderDetailsView"/>.
/// </summary>
public class MirrorFolderDetailsViewModel : BindableBase
{
    private readonly ISelectedFolderService _selectedFolderService;
    private readonly IProjectManager _projectManager;
    private Folder? _selectedFolder;

    /// <summary>
    /// Initializes a new instance of the <see cref="MirrorFolderDetailsViewModel"/> class.
    /// </summary>
    /// <param name="selectedFolderService">The service to manage the currently selected folder.</param>
    /// <param name="projectManager">Manages the current project.</param>
    public MirrorFolderDetailsViewModel(
        ISelectedFolderService selectedFolderService,
        IProjectManager projectManager)
    {
        _selectedFolderService = selectedFolderService;
        _projectManager = projectManager;
        Duplicates = new ObservableCollection<string>();

        _selectedFolderService.SelectedMirrorFolderChanged += OnSelectedMirrorFolderChanged;
    }

    /// <summary>
    /// Gets the id of the currently selected folder.
    /// </summary>
    public string FolderId => _selectedFolder?.Id.ToString() ?? string.Empty;

    /// <summary>
    /// Gets the name of the currently selected folder.
    /// </summary>
    public string Name => _selectedFolder?.Name ?? string.Empty;

    /// <summary>
    /// Gets a collection of pathes to duplicates of this folder.
    /// </summary>
    public ObservableCollection<string> Duplicates { get; }

    /// <summary>
    /// Gets a value indicating whether this folder has been deleted in the meantime.
    /// </summary>
    public string Touched
    {
        get
        {
            if (_selectedFolder == null)
            {
                return string.Empty;
            }

            return (_selectedFolder.Touched == 1).ToString();
        }
    }

    /// <summary>
    /// Gets a value indicating the duplication level of this folder.
    /// </summary>
    public string IsDuplicate
    {
        get
        {
            if (_selectedFolder == null)
            {
                return string.Empty;
            }

            switch (_selectedFolder.IsDuplicate)
            {
            case FolderDuplicationLevel.None: return "Keine Duplikate";
            case FolderDuplicationLevel.ContainsDuplicates: return "Der Ordner enthält Duplikate";
            case FolderDuplicationLevel.EntireContentAreDuplicates: return "Der ganze Ordner-Inhalt ist ein Duplikat";
            }

            return string.Empty;
        }
    }

    private async void OnSelectedMirrorFolderChanged(object? sender, EventArgs e)
    {
        try
        {
            if (_projectManager.CurrentProject == null || !_projectManager.CurrentProject.IsReady)
            {
                return;
            }

            Duplicates.Clear();

            _selectedFolder = _selectedFolderService.SelectedMirrorFolder;

            if (_selectedFolder != null)
            {
                var connection = _projectManager.CurrentProject.Data.Connection;
                var folderRepository = _projectManager.CurrentProject.Data.FolderRepository;
                foreach (var duplicate in await folderRepository.EnumerateDuplicatesOfFolder(connection, _selectedFolder, DriveType.Working))
                {
                    var fullPath = await folderRepository.GetFullPathForFolderAsync(connection, duplicate);
                    Duplicates.Add(System.IO.Path.Join(fullPath.Select(f => f.Name).ToArray()));
                }
            }

            RaisePropertyChanged(string.Empty);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
        }
    }
}
