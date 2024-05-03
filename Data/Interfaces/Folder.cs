namespace BackupUtilities.Data.Interfaces;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// The disk utility tool replicates internally the file/directory table of the file system that
/// is being scanned. Beginning with the configured root directory. This class represents a single
/// folder that exists on disk.
/// </summary>
public class Folder
{
    /// <summary>
    /// Gets or sets the primary key for the folder inside the database.
    /// </summary>
    [Key]
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="Id"/> of the parent directory. This is a foreign key to Id inside the database.
    /// </summary>
    public long? ParentId { get; set; }

    /// <summary>
    /// Gets or sets the name of the directory.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value defining if the folder is on the working drive or the mirror drive.
    /// </summary>
    public DriveType DriveType { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this directory has already been scanned. When a new directory scan is started
    /// the Touched flag of all folders is set to 0. During the folder scan this value is returned to one for folders that exist.
    /// If this value remains at zero that means the folder has been deleted or been moved on the disk.
    /// </summary>
    public short Touched { get; set; } = 0;

    /// <summary>
    /// Gets or sets the folders hash value. The hash value is computed as a combined hash of its content (files and folders).
    /// </summary>
    public string Hash { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether this folder is a duplicate. This flag is set during duplicate file analysis.
    /// </summary>
    public FolderDuplicationLevel IsDuplicate { get; set; }
}
