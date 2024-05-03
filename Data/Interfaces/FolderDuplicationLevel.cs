namespace BackupUtilities.Data.Interfaces;

/// <summary>
/// This enumeration describes the possible duplication levels of folders.
/// </summary>
public enum FolderDuplicationLevel
{
    /// <summary>
    /// This folder is unique. It does not contain any duplicates.
    /// </summary>
    None = 0,

    /// <summary>
    /// This folder contains duplicate files or folders.
    /// </summary>
    ContainsDuplicates = 1,

    /// <summary>
    /// All Files and Folders inside this folder are duplicates.
    /// </summary>
    EntireContentAreDuplicates = 2,

    /// <summary>
    /// The combined has of the folder content is identical to that of another folder. Meaning this
    /// folder is most likely a duplicate of another folder.
    /// </summary>
    HashIdenticalToOtherFolder = 3,
}
