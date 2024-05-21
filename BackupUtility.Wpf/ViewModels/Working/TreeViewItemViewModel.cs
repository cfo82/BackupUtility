namespace BackupUtilities.Wpf.ViewModels.Working;

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using BackupUtilities.Data.Interfaces;
using BackupUtilities.Services.Interfaces;
using BackupUtilities.Wpf.Contracts;
using Prism.Mvvm;

/// <summary>
/// Represents a single item within the folder tree view.
/// </summary>
public class TreeViewItemViewModel : BindableBase
{
    private readonly IErrorHandler _errorHandler;
    private readonly ISelectedFolderService _selectedFolderService;
    private readonly IDbContextData _dbContextData;
    private readonly Folder _folder;
    private readonly TreeViewItemViewModel? _parent;
    private TaskCompletionSource _isFilledCompletionSource;
    private bool _isSelected;
    private bool _isExpanded;

    /// <summary>
    /// Initializes a new instance of the <see cref="TreeViewItemViewModel"/> class.
    /// </summary>
    /// <param name="errorHandler">The error handler.</param>
    /// <param name="selectedFolderService">The service to manage the currently selected folder.</param>
    /// <param name="dbContextData">The data layer.</param>
    /// <param name="folder">The folder that is represented by this item.</param>
    /// <param name="parent">The parent item in the tree.</param>
    public TreeViewItemViewModel(
        IErrorHandler errorHandler,
        ISelectedFolderService selectedFolderService,
        IDbContextData dbContextData,
        Folder folder,
        TreeViewItemViewModel? parent)
    {
        _errorHandler = errorHandler;
        _selectedFolderService = selectedFolderService;
        _dbContextData = dbContextData;
        _isFilledCompletionSource = new TaskCompletionSource();
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
    /// Gets the id of the folder represented by this view model.
    /// </summary>
    public long Id => _folder.Id;

    /// <summary>
    /// Gets a value indicating whether the entire folder has another copy of it somewhere else..
    /// </summary>
    public bool IsHashIdentical => _folder.IsDuplicate == FolderDuplicationLevel.HashIdenticalToOtherFolder;

    /// <summary>
    /// Gets a value indicating whether the folder contains duplicates.
    /// </summary>
    public bool ContainsDuplicates => _folder.IsDuplicate == FolderDuplicationLevel.ContainsDuplicates ||
        _folder.IsDuplicate == FolderDuplicationLevel.EntireContentAreDuplicates;

    /// <summary>
    /// Gets a value indicating whether this folder does not contain any duplicate fiels.
    /// </summary>
    public bool IsUnique => !IsHashIdentical && !ContainsDuplicates;

    /// <summary>
    /// Gets the name of this item.
    /// </summary>
    public string Name => $"{_folder.Name} [{_folder.Id}]";

    /// <summary>
    /// Gets a <see cref="TaskCompletionSource"/> to wait for when you need to ensure the children are correctly loaded.
    /// </summary>
    public TaskCompletionSource IsFilledCompletionSource => _isFilledCompletionSource;

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
    /// Gets the folder duplication level of this folder.
    /// </summary>
    public FolderDuplicationLevel DuplicationLevel => _folder.IsDuplicate;

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
                _isFilledCompletionSource = new TaskCompletionSource();
                Children.Clear();
            }
        }
    }

    private async void Fill()
    {
        try
        {
            Children.Clear();

            var folderRepository = _dbContextData.FolderRepository;

            var subFolders = await folderRepository.GetSubFoldersAsync(_folder);
            var children = subFolders
                .Select(subFolder => new TreeViewItemViewModel(_errorHandler, _selectedFolderService, _dbContextData, subFolder, this))
                .OrderBy(subFolder => subFolder.Name);

            Children.AddRange(children);

            _isFilledCompletionSource.SetResult();
        }
        catch (Exception e)
        {
            _errorHandler.Error = e;
        }
    }
}
