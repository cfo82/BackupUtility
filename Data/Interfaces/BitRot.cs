namespace BackupUtilities.Data.Interfaces;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// The backup utilities can detect bitrot. This means files that have a bit flipped on the disk. NTFS cannot detect bitrot by itself.
/// If the backup utilites detect bitrot an instance of this class is stored inside the database. Indicating which file has bitrot.
/// </summary>
public class BitRot
{
    /// <summary>
    /// Gets or sets the primary key of this entity.
    /// </summary>
    [Key]
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the id of the scan during which this entity was found.
    /// </summary>
    public long? ScanId { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="Folder.Id"/> of the folder containing the file with bitrot. This is part of the
    /// combined foreign key to <see cref="File"/> together with <see cref="FileName"/>.
    /// </summary>
    public long FolderId { get; set; }

    /// <summary>
    /// Gets or sets the name of the file that has bitrot. This is part of the
    /// combined foreign key to <see cref="File"/> together with <see cref="FolderId"/>.
    /// </summary>
    public string FileName { get; set; } = string.Empty;
}
