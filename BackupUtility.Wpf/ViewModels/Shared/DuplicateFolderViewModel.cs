namespace BackupUtilities.Wpf.ViewModels.Shared;

using System.Windows.Input;
using Prism.Commands;
using Prism.Mvvm;

/// <summary>
/// View model that represents a single path for the duplicate folder list within the folder details view.
/// </summary>
public class DuplicateFolderViewModel : BindableBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DuplicateFolderViewModel"/> class.
    /// </summary>
    /// <param name="path">The path for the folder.</param>
    public DuplicateFolderViewModel(string path)
    {
        Path = path;
        CopyPathToClipboardCommand = new DelegateCommand(OnCopyPathToClipboard);
    }

    /// <summary>
    /// Gets the path of this duplicate.
    /// </summary>
    public string Path { get; }

    /// <summary>
    /// Gets the command to copy path of the currently selected folder to the clipboard.
    /// </summary>
    public ICommand CopyPathToClipboardCommand { get; private set; }

    private void OnCopyPathToClipboard()
    {
        System.Windows.Clipboard.SetText(Path);
    }
}
