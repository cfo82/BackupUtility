namespace BackupUtilities.Wpf.ViewModels.Working;

using System.Collections.ObjectModel;
using System.Linq;
using BackupUtilities.Data.Interfaces;
using BackupUtilities.Wpf.Contracts;
using Prism.Mvvm;

/// <summary>
/// Represents a single item within the folder tree view.
/// </summary>
public class TreeViewItemViewModel : BindableBase
{
    private readonly ISelectedFolderService _selectedFolderService;
    private readonly IDbContextData _dbContextData;
    private readonly Folder _folder;
    private readonly TreeViewItemViewModel? _parent;
    private bool _isSelected;
    private bool _isExpanded;

    /// <summary>
    /// Initializes a new instance of the <see cref="TreeViewItemViewModel"/> class.
    /// </summary>
    /// <param name="selectedFolderService">The service to manage the currently selected folder.</param>
    /// <param name="dbContextData">The data layer.</param>
    /// <param name="folder">The folder that is represented by this item.</param>
    /// <param name="parent">The parent item in the tree.</param>
    public TreeViewItemViewModel(
        ISelectedFolderService selectedFolderService,
        IDbContextData dbContextData,
        Folder folder,
        TreeViewItemViewModel? parent)
    {
        _selectedFolderService = selectedFolderService;
        _dbContextData = dbContextData;
        _folder = folder;
        _parent = parent;

        Children = new ObservableCollection<TreeViewItemViewModel>();

        if (parent != null)
        {
            parent.PropertyChanged += Parent_PropertyChanged;
        }

        if (parent == null)
        {
            Fill();
        }
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
    /// Gets the name of this item.
    /// </summary>
    public string Name => $"{_folder.Name} [{_folder.Id}]";

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
                _selectedFolderService.SelectedFolder = _folder;
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
    /// Gets the children of this item.
    /// </summary>
    public ObservableCollection<TreeViewItemViewModel> Children { get; }

    private void Parent_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (string.Equals(nameof(IsExpanded), e.PropertyName))
        {
            if (_parent != null && _parent.IsExpanded)
            {
                Fill();
            }
            else
            {
                Children.Clear();
            }
        }
    }

    private async void Fill()
    {
        Children.Clear();

        var connection = _dbContextData.Connection;
        var folderRepository = _dbContextData.FolderRepository;

        var subFolders = await folderRepository.GetSubFoldersAsync(connection, _folder);
        var children = subFolders
            .Select(subFolder => new TreeViewItemViewModel(_selectedFolderService, _dbContextData, subFolder, this))
            .OrderBy(subFolder => subFolder.Name);

        Children.AddRange(children);
    }
}
