namespace BackupUtilities.Data.Interfaces;

using System.Data;
using System.Threading.Tasks;

/// <summary>
/// Repository to manage folders.
/// </summary>
public interface IFolderRepository : IRepository
{
    /// <summary>
    /// Create a folder.
    /// </summary>
    /// <param name="folder">The folder to be stored in the database.</param>
    /// <returns>A task for async processing.</returns>
    Task SaveFolderAsync(Folder folder);

    /// <summary>
    /// Given a long path create and save folders for the entire path recursively. If a
    /// folder already exists it returns the existing folder.
    /// </summary>
    /// <param name="path">The full absolute path.</param>
    /// <param name="driveType">Deciding if this folder is on the working drive or the mirror drive.</param>
    /// <returns>The newly created folder.</returns>
    Task<Folder> SaveFullPathAsync(string path, DriveType driveType);

    /// <summary>
    /// Mark all folders in the repository as untouched.
    /// </summary>
    /// <returns>A task for async processing.</returns>
    Task MarkAllFoldersAsUntouchedAsync();

    /// <summary>
    /// Count the folders on one of the drives.
    /// </summary>
    /// <param name="driveType">The drive for which the folders should be counted.</param>
    /// <returns>The number of folders available for that drive.</returns>
    Task<long> GetFolderCount(DriveType driveType);

    /// <summary>
    /// Mark the given folder as touched.
    /// </summary>
    /// <param name="folder">The folder to be marked.</param>
    /// <returns>A task for async processing.</returns>
    Task TouchFolderAsync(Folder folder);

    /// <summary>
    /// Search a folder in the database.
    /// </summary>
    /// <param name="folderId">The id of the folder.</param>
    /// <returns>The folder instance if found.</returns>
    Task<Folder?> GetFolderAsync(long folderId);

    /// <summary>
    /// Search a folder in the database.
    /// </summary>
    /// <param name="path">The full absolute path to the folder.</param>
    /// <returns>The folder instance if found.</returns>
    Task<Folder?> GetFolderAsync(string path);

    /// <summary>
    /// Load a subfolder.
    /// </summary>
    /// <param name="parentId">The id of the parent folder.</param>
    /// <param name="name">The name of the folder.</param>
    /// <returns>The folder instance if found.</returns>
    Task<Folder?> GetFolderAsync(long parentId, string name);

    /// <summary>
    /// Get the root folders for the given drive type.
    /// </summary>
    /// <param name="driveType">The drive type to enumerate.</param>
    /// <returns>An enumerable of all root folders.</returns>
    Task<IEnumerable<Folder>> GetRootFolders(DriveType driveType);

    /// <summary>
    /// Return all subfolders of a given folder.
    /// </summary>
    /// <param name="folder">The folder to be queried.</param>
    /// <returns>An enumerable of all subfolders.</returns>
    Task<IEnumerable<Folder>> GetSubFoldersAsync(Folder folder);

    /// <summary>
    /// Select the full path for the given folder. This returns a <see cref="IEnumerable{T}"/> of folders
    /// beginning with the root up to the folder that was passed as argument to this method.
    /// </summary>
    /// <param name="folder">The folder for which the full path should be evaluated.</param>
    /// <returns>An enumerable of parent folders. Sorted from root to the folder passed to this method.</returns>
    Task<IEnumerable<Folder>> GetFullPathForFolderAsync(Folder folder);

    /// <summary>
    /// Select the full path for the given folder. This returns a <see cref="IEnumerable{T}"/> of folders
    /// beginning with the root up to the folder that was passed as argument to this method.
    /// </summary>
    /// <param name="folderId">The folder for which the full path should be evaluated.</param>
    /// <returns>An enumerable of parent folders. Sorted from root to the folder passed to this method.</returns>
    Task<IEnumerable<Folder>> GetFullPathForFolderAsync(long folderId);

    /// <summary>
    /// Delete all folders from the repository.
    /// </summary>
    /// <returns>A task for async processing.</returns>
    Task DeleteAllAsync();

    /// <summary>
    /// Remove the duplicate flag from all folders.
    /// </summary>
    /// <param name="driveType">The drive on which flags are to be reset.</param>
    /// <returns>A task for async processing.</returns>
    Task RemoveAllDuplicateMarks(DriveType driveType);

    /// <summary>
    /// Marks the given folder as duplicate.
    /// </summary>
    /// <param name="folder">The folder that should be marked as duplicate.</param>
    /// <param name="duplicationLevel">The level of duplication. 0 == no duplicates, 1 == contains duplicates, 2 == entire folder contains only duplicates.</param>
    /// <returns>A task for async programming.</returns>
    Task MarkFolderAsDuplicate(Folder folder, FolderDuplicationLevel duplicationLevel);

    /// <summary>
    /// Save the given hash for the folder.
    /// </summary>
    /// <param name="folder">The folder for which the hash should be saved.</param>
    /// <param name="hash">The folders hash value.</param>
    /// <returns>A task for async programming.</returns>
    Task SaveFolderHashAsync(Folder folder, string hash);

    /// <summary>
    /// Find folders with the same hash.
    /// </summary>
    /// <param name="driveType">The drive on which to search for duplicates.</param>
    /// <returns>A list of folders that have duplicates.</returns>
    Task<IEnumerable<Folder>> FindDuplicateFoldersAsync(DriveType driveType);

    /// <summary>
    /// Find duplicates of this folder using its hash.
    /// </summary>
    /// <param name="folder">The folder for which duplicates are to be searched.</param>
    /// <param name="driveType">The drive on which to search for duplicates with the same hash.</param>
    /// <returns>The list of duplicates.</returns>
    Task<IEnumerable<Folder>> EnumerateDuplicatesOfFolder(Folder folder, DriveType driveType);
}
