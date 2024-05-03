namespace BackupUtilities.Data.Interfaces;

/// <summary>
/// A single directory that should be ignored when processing files.
/// </summary>
public class IgnoredFolder
{
    /// <summary>
    /// Gets or sets the absolute path to the directory that should be ignored.
    /// </summary>
    public string Path { get; set; } = string.Empty;
}
