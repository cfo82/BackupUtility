namespace BackupUtilities.Wpf.ViewModels.Working;

using BackupUtilities.Data.Interfaces;
using Prism.Mvvm;

/// <summary>
/// The view model for a single file.
/// </summary>
public class FileViewModel : BindableBase
{
    private readonly File _file;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileViewModel"/> class.
    /// </summary>
    /// <param name="file">The file that is represented by this view model.</param>
    public FileViewModel(File file)
    {
        _file = file;
    }

    /// <summary>
    /// Gets the file that is represented by this view model.
    /// </summary>
    public File File => _file;

    /// <summary>
    /// Gets the name of the file.
    /// </summary>
    public string Name => _file.Name;

    /// <summary>
    /// Gets the intro hash of the file.
    /// </summary>
    public string IntroHash => _file.IntroHash;

    /// <summary>
    /// Gets the hash of the file.
    /// </summary>
    public string Hash => _file.Hash;

    /// <summary>
    /// Gets the last written timestamp of the file.
    /// </summary>
    public string LastWriteTime => _file.LastWriteTime;

    /// <summary>
    /// Gets a value indicating whether this file is a duplicate.
    /// </summary>
    public bool IsDuplicate => _file.IsDuplicate > 0;
}
