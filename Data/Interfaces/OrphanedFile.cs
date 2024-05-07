namespace BackupUtilities.Data.Interfaces;

/// <summary>
/// Represents a single orphaned file. Orphaned files are files that exist on the Mirror but
/// not on the live drive. That means they will be deleted during the next robocopy /MIR.
/// </summary>
public class OrphanedFile : BaseFile
{
    /// <summary>
    /// Gets or sets the number of copies of this file on the live drive. This is a computed value. Its based on
    /// <see cref="BaseFile.Hash"/> and counts the number of files in <see cref="IFileRepository"/> with the exact same hash.
    /// </summary>
    public int NumCopiesOnLiveDrive { get; set; }

    /// <summary>
    /// Gets or sets the copies of this file on the live drive. This is a computed value and is filled only on request.
    /// Its based on <see cref="BaseFile.Hash"/> and retrieves the files from <see cref="IFileRepository"/> with the exact
    /// same hash.
    /// </summary>
    public List<File> DuplicatesOnLifeDrive { get; set; } = new List<File>();
}
