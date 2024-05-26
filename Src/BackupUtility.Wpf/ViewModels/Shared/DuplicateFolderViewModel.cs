namespace BackupUtilities.Wpf.ViewModels.Shared;

using System;
using System.Diagnostics;
using System.Windows.Input;
using BackupUtilities.Data.Interfaces;
using BackupUtilities.Wpf.Contracts;
using Prism.Commands;
using Prism.Mvvm;

/// <summary>
/// View model that represents a single path for the duplicate folder list within the folder details view.
/// </summary>
public class DuplicateFolderViewModel : BindableBase
{
    private readonly ISelectedFolderService _selectedFolderService;
    private readonly Folder _folder;

    /// <summary>
    /// Initializes a new instance of the <see cref="DuplicateFolderViewModel"/> class.
    /// </summary>
    /// <param name="folder">The duplicate folder data item.</param>
    /// <param name="selectedFolderService">The service that manages the currently selected folder.</param>
    /// <param name="path">The path for the folder.</param>
    public DuplicateFolderViewModel(ISelectedFolderService selectedFolderService, Folder folder, string path)
    {
        _selectedFolderService = selectedFolderService;
        _folder = folder;

        Path = path;

        CopyPathToClipboardCommand = new DelegateCommand(OnCopyPathToClipboard);
        GoToCopyCommand = new DelegateCommand(OnGoToCopy);
        OpenFolderInExplorerCommand = new DelegateCommand(OnOpenFolderInExplorer);
    }

    /// <summary>
    /// Gets the path of this duplicate.
    /// </summary>
    public string Path { get; }

    /// <summary>
    /// Gets the command to copy path of the currently selected folder to the clipboard.
    /// </summary>
    public ICommand CopyPathToClipboardCommand { get; private set; }

    /// <summary>
    /// Gets a command to move the selection to this copy.
    /// </summary>
    public ICommand GoToCopyCommand { get; private set; }

    /// <summary>
    /// Gets a command to open the folder in explorer.
    /// </summary>
    public ICommand OpenFolderInExplorerCommand { get; private set; }

    private void OnCopyPathToClipboard()
    {
        System.Windows.Clipboard.SetText(Path);
    }

    private void OnGoToCopy()
    {
        _selectedFolderService.SelectedFolder = _folder;
    }

    private void OnOpenFolderInExplorer()
    {
        Process.Start("explorer.exe", Path);
    }
}
