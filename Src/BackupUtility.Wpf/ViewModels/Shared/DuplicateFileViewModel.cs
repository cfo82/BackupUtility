namespace BackupUtilities.Wpf.ViewModels.Shared;

using System.Windows.Input;
using Prism.Commands;
using Prism.Mvvm;

/// <summary>
/// Represents a single duplicate file within the duplication list inside the <see cref="FileDetailsViewModelBase"/>.
/// </summary>
public class DuplicateFileViewModel : BindableBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DuplicateFileViewModel"/> class.
    /// </summary>
    /// <param name="folderPath">The path for the folder.</param>
    /// <param name="fileName">The name of the file.</param>
    public DuplicateFileViewModel(string folderPath, string fileName)
    {
        FolderPath = folderPath;
        FileName = fileName;

        CopyFolderPathToClipboardCommand = new DelegateCommand(OnCopyFolderPathToClipboard);
        CopyFilePathToClipboardCommand = new DelegateCommand(OnCopyFilePathToClipboard);
    }

    /// <summary>
    /// Gets the path to the folder that contains this duplicate.
    /// </summary>
    public string FolderPath { get; }

    /// <summary>
    /// Gets the name of this duplicate.
    /// </summary>
    public string FileName { get; }

    /// <summary>
    /// Gets the full path of the file.
    /// </summary>
    public string FilePath => System.IO.Path.Combine(FolderPath, FileName);

    /// <summary>
    /// Gets the command to copy path of the folder to the clipboard.
    /// </summary>
    public ICommand CopyFolderPathToClipboardCommand { get; private set; }

    /// <summary>
    /// Gets the command to copy the full path of the file to the clipboard.
    /// </summary>
    public ICommand CopyFilePathToClipboardCommand { get; private set; }

    private void OnCopyFolderPathToClipboard()
    {
        System.Windows.Clipboard.SetText(FolderPath);
    }

    private void OnCopyFilePathToClipboard()
    {
        System.Windows.Clipboard.SetText(FilePath);
    }
}
