namespace BackupUtilities.Data.Interfaces;

/// <summary>
/// A repository that stores orphaned files. Orphaned files are files that exist on the Mirror but
/// not on the live drive. That means they will be deleted during the next robocopy /MIR.
/// </summary>
public interface IOrphanedFileRepository : IRepository
{
    /// <summary>
    /// Delete all orphaned files from the repository.
    /// </summary>
    /// <returns>A task for async processing.</returns>
    Task DeleteAllAsync();

    /// <summary>
    /// Save the orphaned file inside the repository.
    /// </summary>
    /// <param name="orphanedFile">The orphaned file to save.</param>
    /// <returns>A task for async processing.</returns>
    Task SaveOrphanedFileAsync(OrphanedFile orphanedFile);

    /// <summary>
    /// Enumerate all orphaned files within the database.
    /// </summary>
    /// <returns>The list of all discovered orphaned files.</returns>
    Task<IEnumerable<OrphanedFile>> EnumerateAllOrphanedFiles();

    /// <summary>
    /// List all files from the given folder.
    /// </summary>
    /// <param name="parent">The parent folder.</param>
    /// <param name="loadWorkingCopies">Load the files from the working drive.</param>
    /// <returns>A list of files.</returns>
    Task<IEnumerable<OrphanedFile>> EnumerateOrphanedFilesByFolderAsync(
        Folder parent,
        bool loadWorkingCopies);
}
