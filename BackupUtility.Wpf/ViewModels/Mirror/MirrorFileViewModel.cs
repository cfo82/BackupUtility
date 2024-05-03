namespace BackupUtilities.Wpf.ViewModels.Mirror;

using System.Collections.Generic;
using BackupUtilities.Data.Interfaces;
using Prism.Mvvm;

/// <summary>
/// The view model for a single duplicate file.
/// </summary>
public class MirrorFileViewModel : BindableBase
{
    private readonly OrphanedFile _file;
    private readonly List<File> _duplicates;

    /// <summary>
    /// Initializes a new instance of the <see cref="MirrorFileViewModel"/> class.
    /// </summary>
    /// <param name="file">The orphaned file instance.</param>
    /// <param name="duplicates">A list of copies of this file on the live drive.</param>
    public MirrorFileViewModel(
        OrphanedFile file,
        IEnumerable<File> duplicates)
    {
        _file = file;
        _duplicates = new List<File>(duplicates);
    }

    /// <summary>
    /// Gets the file that is represented by this viewmodel.
    /// </summary>
    public OrphanedFile File => _file;

    /// <summary>
    /// Gets the hash of the duplicate file.
    /// </summary>
    public string Hash => _file.Hash;

    /// <summary>
    /// Gets the name of the file.
    /// </summary>
    public string Name => _file.Name;

    /// <summary>
    /// Gets a value indicating whether this file has identical copies somewhere on the live drive.
    /// </summary>
    public bool HasCopiesOnLiveDrive => _duplicates.Count > 0;
}
