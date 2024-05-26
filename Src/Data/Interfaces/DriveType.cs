namespace BackupUtilities.Data.Interfaces;

/// <summary>
/// Enumeration defining which drive a folder resides on.
/// </summary>
public enum DriveType
{
    /// <summary>
    /// The live working copy.
    /// </summary>
    Working = 0,

    /// <summary>
    /// The drive where the mirror files are.
    /// </summary>
    Mirror = 1,
}
