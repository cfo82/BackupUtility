namespace BackupUtilities.Data.Interfaces;

/// <summary>
/// Used to group duplicate files by their hash.
/// </summary>
public class DuplicateFiles
{
    /// <summary>
    /// Gets or sets the hash that identifies the files.
    /// </summary>
    public string Hash { get; set; } = string.Empty;

    /// <summary>
    /// Gets an <see cref="IEnumerable{File}"/> of all files that share the same hash.
    /// </summary>
    public IEnumerable<File> Files { get; init; } = Enumerable.Empty<File>();
}
