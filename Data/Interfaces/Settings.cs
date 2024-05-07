namespace BackupUtilities.Data.Interfaces;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Represents the settings for a database/file.
/// </summary>
public class Settings
{
    /// <summary>
    /// Gets or sets the primary key of the settings.
    /// </summary>
    [Key]
    public int SettingsId { get; set; }

    /// <summary>
    /// Gets or sets the id of the scan for which the settings have been valid.
    /// </summary>
    public long? ScanId { get; set; }

    /// <summary>
    /// Gets or sets the root path of the live working tree.
    /// </summary>
    public string RootPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the root path of where the mirror of the live data is stored.
    /// </summary>
    public string MirrorPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a list of ignored directories that are neither scanned nor processed.
    /// </summary>
    public List<IgnoredFolder> IgnoredFolders { get; set; } = [];
}
