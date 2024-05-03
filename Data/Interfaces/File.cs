namespace BackupUtilities.Data.Interfaces;

/// <summary>
/// The disk utility tool replicates internally the file/directory table of the file system that
/// is being scanned. Beginning with the configured root directory. This class represents a single
/// file that exists on disk.
/// </summary>
public class File
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
    /// Gets or sets the intro hash. It is a hash over a certain number of bytes for each file. It is being used to detect
    /// similar files (files that begin with the same data but have different data in the end.
    /// </summary>
    public string IntroHash { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the hash of the file. The hash is calculated over the entire content of the file and is being used
    /// to detect bitrot.
    /// </summary>
    public string Hash { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the last time that the file has been written. This is used to detect if a file has changed since the last run
    /// such that both hashes need to be updated.
    /// </summary>
    public string LastWriteTime { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether this file has already been scanned. When a new file scan is started
    /// the Touched flag of all files is set to 0. During the file scan this value is returned to one for files that exist.
    /// If this value remains at zero that means the file has been deleted or been moved on the disk.
    /// </summary>
    public short Touched { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this file is a duplicate. This flag is set during duplicate file analysis.
    /// </summary>
    public short IsDuplicate { get; set; }
}
