namespace BackupUtilities.Wpf.ViewModels.Shared;

using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using BackupUtilities.Data.Interfaces;
using BackupUtilities.Services.Interfaces;
using BackupUtilities.Wpf.Contracts;
using BackupUtilities.Wpf.Views.Shared;
using Prism.Mvvm;

/// <summary>
/// The view model for <see cref="SharedFileDetailsView"/>.
/// </summary>
public class FileDetailsViewModelBase : BindableBase
{
    private readonly IProjectManager _projectManager;
    private BaseFile? _selectedFile;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileDetailsViewModelBase"/> class.
    /// </summary>
    /// <param name="projectManager">Manages the current project.</param>
    public FileDetailsViewModelBase(
        IProjectManager projectManager)
    {
        _projectManager = projectManager;
        Duplicates = new();
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
    /// Gets the hash of the selected file.
    /// </summary>
    public string Hash => _selectedFile?.Hash ?? string.Empty;

    /// <summary>
    /// Gets a collection of pathes to duplicates of this folder.
    /// </summary>
    public ObservableCollection<DuplicateFileViewModel> Duplicates { get; }

    /// <summary>
    /// Method to be called by implementations of this view model when the respective selected file has changed.
    /// </summary>
    /// <param name="selectedFile">The newly selected file.</param>
    /// <returns>A task for async programming.</returns>
    protected async Task OnSelectedFileChanged(BaseFile? selectedFile)
    {
        if (_projectManager.CurrentProject == null || !_projectManager.CurrentProject.IsReady)
        {
            return;
        }

        Duplicates.Clear();

        _selectedFile = selectedFile;

        if (_selectedFile != null)
        {
            var connection = _projectManager.CurrentProject.Data.Connection;
            var fileRepository = _projectManager.CurrentProject.Data.FileRepository;
            var folderRepository = _projectManager.CurrentProject.Data.FolderRepository;

            foreach (var duplicate in await fileRepository.EnumerateDuplicatesOfFile(connection, _selectedFile))
            {
                var folder = await folderRepository.GetFolderAsync(connection, duplicate.ParentId);
                if (folder == null)
                {
                    throw new InvalidOperationException("Unexpected error: folder is null.");
                }

                var fullPath = await folderRepository.GetFullPathForFolderAsync(connection, folder);
                Duplicates.Add(new DuplicateFileViewModel(
                    System.IO.Path.Join(fullPath.Select(f => f.Name).ToArray()),
                    duplicate.Name));
            }
        }

        RaisePropertyChanged(string.Empty);
    }
}
