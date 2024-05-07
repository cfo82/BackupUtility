namespace BackupUtilities.Wpf.ViewModels.Mirror;

using System.Collections.ObjectModel;
using BackupUtilities.Data.Interfaces;
using BackupUtilities.Wpf.Contracts;
using Prism.Mvvm;

/// <summary>
/// Represents a single item within the folder tree view for duplicate files.
/// </summary>
public class MirrorTreeViewItemViewModel : BindableBase
{
    private readonly ISelectedFolderService _selectedFolderService;
    private readonly Folder _folder;
    private bool _isSelected;
    private bool _isExpanded;

    /// <summary>
    /// Initializes a new instance of the <see cref="MirrorTreeViewItemViewModel"/> class.
    /// </summary>
    /// <param name="selectedFolderService">The service to manage the currently selected folder.</param>
    /// <param name="folder">The folder that is represented by this item.</param>
    public MirrorTreeViewItemViewModel(
        ISelectedFolderService selectedFolderService,
        Folder folder)
    {
        _selectedFolderService = selectedFolderService;
        _folder = folder;
        _isExpanded = true;

        Children = new ObservableCollection<MirrorTreeViewItemViewModel>();
    }

    /// <summary>
    /// Gets the folder that is represented by this item.
    /// </summary>
    public Folder Folder => _folder;

    /// <summary>
    /// Gets the name of this item.
    /// </summary>
    public string Name => _folder.Name;

    /// <summary>
    /// Gets or sets a value indicating whether this item is currently selected.
    /// </summary>
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (SetProperty(ref _isSelected, value))
            {
                _selectedFolderService.SelectedMirrorFolder = _folder;
            }
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether this item is currently expanded.
    /// </summary>
    public bool IsExpanded
    {
        get => _isExpanded;
        set { SetProperty(ref _isExpanded, value); }
    }

    /// <summary>
    /// Gets a value indicating whether all subfolders and files inside the folder are duplicates.
    /// </summary>
    public bool AllFilesAreDuplicates => _folder.IsDuplicate == FolderDuplicationLevel.HashIdenticalToOtherFolder;

    /// <summary>
    /// Gets a value indicating whether the folder contains duplicates.
    /// </summary>
    public bool ContainsDuplicates => _folder.IsDuplicate == FolderDuplicationLevel.ContainsDuplicates;

    /// <summary>
    /// Gets the children of this item.
    /// </summary>
    public ObservableCollection<MirrorTreeViewItemViewModel> Children { get; }
}
