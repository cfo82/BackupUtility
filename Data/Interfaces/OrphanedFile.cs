namespace BackupUtilities.Data.Interfaces;

/// <summary>
/// Represents a single orphaned file. Orphaned files are files that exist on the Mirror but
/// not on the live drive. That means they will be deleted during the next robocopy /MIR.
/// </summary>
public class OrphanedFile
{
    /// <summary>
    /// Gets or sets the <see cref="Folder.Id"/> of the parent folder. This is part of the combined
    /// priamry key of this entity together with <see cref="Name"/>.
    /// </summary>
    public long ParentId { get; set; }

    /// <summary>
    /// Gets or sets the name of the file. This is part of the combined primary key of this entity
    /// together with <see cref="ParentId"/>.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the hash of the file. The hash is calculated over the entire content of the file and is being used
    /// to detect files on the working drive that still match the deleted orphaned file.
    /// </summary>
    public string Hash { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the number of copies of this file on the live drive. This is a computed value. Its based on
    /// <see cref="Hash"/> and counts the number of files in <see cref="IFileRepository"/> with the exact same hash.
    /// </summary>
    public int NumCopiesOnLiveDrive { get; set; }

    /// <summary>
    /// Gets or sets the copies of this file on the live drive. This is a computed value and is filled only on request.
    /// Its based on <see cref="Hash"/> and retrieves the files from <see cref="IFileRepository"/> with the exact
    /// same hash.
    /// </summary>
    public List<File> DuplicatesOnLifeDrive { get; set; } = new List<File>();
}
