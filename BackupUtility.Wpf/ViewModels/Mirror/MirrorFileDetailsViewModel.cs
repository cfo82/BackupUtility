namespace BackupUtilities.Wpf.ViewModels.Mirror;

using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using BackupUtilities.Data.Interfaces;
using BackupUtilities.Services.Interfaces;
using BackupUtilities.Wpf.Contracts;
using BackupUtilities.Wpf.Views.Mirror;
using Prism.Mvvm;

/// <summary>
/// The view model for <see cref="MirrorFileDetailsView"/>.
/// </summary>
public class MirrorFileDetailsViewModel : BindableBase
{
    private readonly ISelectedFileService _selectedFileService;
    private readonly IProjectManager _projectManager;
    private OrphanedFile? _selectedFile;

    /// <summary>
    /// Initializes a new instance of the <see cref="MirrorFileDetailsViewModel"/> class.
    /// </summary>
    /// <param name="selectedFileService">The service to manage the currently selected file.</param>
    /// <param name="projectManager">Manages the current project.</param>
    public MirrorFileDetailsViewModel(
        ISelectedFileService selectedFileService,
        IProjectManager projectManager)
    {
        _selectedFileService = selectedFileService;
        _projectManager = projectManager;
        Duplicates = new ObservableCollection<string>();

        _selectedFileService.SelectedMirrorFileChanged += OnSelectedMirrorFileChanged;
    }

    /// <summary>
    /// Gets the id of the currently selected folder.
    /// </summary>
    public string ParentId => _selectedFile?.ParentId.ToString() ?? string.Empty;

    /// <summary>
    /// Gets the name of the currently selected folder.
    /// </summary>
    public string Name => _selectedFile?.Name ?? string.Empty;

    /// <summary>
    /// Gets a collection of pathes to duplicates of this folder.
    /// </summary>
    public ObservableCollection<string> Duplicates { get; }

    /// <summary>
    /// Gets a value indicating whether this folder has been deleted in the meantime.
    /// </summary>
    public string Touched => string.Empty;

    /// <summary>
    /// Gets a value indicating the duplication level of this folder.
    /// </summary>
    public string IsDuplicate => (Duplicates.Count > 0).ToString();

    private async void OnSelectedMirrorFileChanged(object? sender, EventArgs e)
    {
        try
        {
            if (_projectManager.CurrentProject == null || !_projectManager.CurrentProject.IsReady)
            {
                return;
            }

            Duplicates.Clear();

            _selectedFile = _selectedFileService.SelectedMirrorFile;

            if (_selectedFile != null)
            {
                var connection = _projectManager.CurrentProject.Data.Connection;
                var fileRepository = _projectManager.CurrentProject.Data.FileRepository;
                var folderRepository = _projectManager.CurrentProject.Data.FolderRepository;

                foreach (var duplicate in await fileRepository.EnumerateDuplicatesWithHash(connection, _selectedFile.Hash))
                {
                    var folder = await folderRepository.GetFolderAsync(connection, duplicate.ParentId);
                    if (folder == null)
                    {
                        throw new InvalidOperationException("Unexpected error: folder is null.");
                    }

                    var fullPath = await folderRepository.GetFullPathForFolderAsync(connection, folder);
                    Duplicates.Add(System.IO.Path.Join(fullPath.Select(f => f.Name).Append(duplicate.Name).ToArray()));
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
