namespace BackupUtilities.Wpf.ViewModels.Shared;

using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using BackupUtilities.Data.Interfaces;
using BackupUtilities.Services.Interfaces;
using BackupUtilities.Wpf.Contracts;
using BackupUtilities.Wpf.Views.Shared;
using Prism.Commands;
using Prism.Mvvm;

/// <summary>
/// Base class for the view models related with <see cref="SharedFolderDetailsView"/>.
/// </summary>
public class FolderDetailsViewModelBase : BindableBase
{
    private readonly IProjectManager _projectManager;
    private Folder? _selectedFolder;

    /// <summary>
    /// Initializes a new instance of the <see cref="FolderDetailsViewModelBase"/> class.
    /// </summary>
    /// <param name="projectManager">Manages the current project.</param>
    public FolderDetailsViewModelBase(
        IProjectManager projectManager)
    {
        _projectManager = projectManager;

        Path = string.Empty;
        Duplicates = new();
        CopyPathToClipboardCommand = new DelegateCommand(OnCopyPathToClipboard);
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
    /// Gets the full path of the currently selected folder..
    /// </summary>
    public string Path { get; private set; }

    /// <summary>
    /// Gets a collection of pathes to duplicates of this folder.
    /// </summary>
    public ObservableCollection<DuplicateFolderViewModel> Duplicates { get; }

    /// <summary>
    /// Gets the command to copy path of the currently selected folder to the clipboard.
    /// </summary>
    public ICommand CopyPathToClipboardCommand { get; private set; }

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

    /// <summary>
    /// Method to be called by implementations of this view model when the respective selected folder has changed.
    /// </summary>
    /// <param name="selectedFolder">The newly selected folder.</param>
    /// <returns>A task for async programming.</returns>
    protected async Task OnSelectedFolderChanged(Folder? selectedFolder)
    {
        if (_projectManager.CurrentProject == null || !_projectManager.CurrentProject.IsReady)
        {
            return;
        }

        Duplicates.Clear();

        _selectedFolder = selectedFolder;

        if (_selectedFolder != null)
        {
            var connection = _projectManager.CurrentProject.Data.Connection;
            var folderRepository = _projectManager.CurrentProject.Data.FolderRepository;

            var fullPath = await folderRepository.GetFullPathForFolderAsync(connection, _selectedFolder);
            Path = System.IO.Path.Join(fullPath.Select(f => f.Name).ToArray());

            foreach (var duplicate in await folderRepository.EnumerateDuplicatesOfFolder(connection, _selectedFolder, DriveType.Working))
            {
                fullPath = await folderRepository.GetFullPathForFolderAsync(connection, duplicate);
                Duplicates.Add(new DuplicateFolderViewModel(System.IO.Path.Join(fullPath.Select(f => f.Name).ToArray())));
            }
        }

        RaisePropertyChanged(string.Empty);
    }

    private void OnCopyPathToClipboard()
    {
        System.Windows.Clipboard.SetText(Path);
    }
}
