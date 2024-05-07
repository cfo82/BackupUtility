namespace BackupUtilities.Data.Interfaces;

/// <summary>
/// Base class for the two file entities in the database.
/// </summary>
public class BaseFile
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
    /// to detect bitrot.
    /// </summary>
    public string Hash { get; set; } = string.Empty;
}
