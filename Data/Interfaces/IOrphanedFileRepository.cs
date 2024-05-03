namespace BackupUtilities.Data.Interfaces;

using System.Data;

/// <summary>
/// A repository that stores orphaned files. Orphaned files are files that exist on the Mirror but
/// not on the live drive. That means they will be deleted during the next robocopy /MIR.
/// </summary>
public interface IOrphanedFileRepository : IRepository
{
    /// <summary>
    /// Delete all orphaned files from the repository.
    /// </summary>
    /// <param name="connection">The connection to the database.</param>
    /// <returns>A task for async processing.</returns>
    Task DeleteAllAsync(IDbConnection connection);

    /// <summary>
    /// Save the orphaned file inside the repository.
    /// </summary>
    /// /// <param name="connection">The connection to the database.</param>
    /// <param name="orphanedFile">The orphaned file to save.</param>
    /// <returns>A task for async processing.</returns>
    Task SaveOrphanedFileAsync(IDbConnection connection, OrphanedFile orphanedFile);

    /// <summary>
    /// Enumerate all orphaned files within the database.
    /// </summary>
    /// <param name="connection">The connection to the database.</param>
    /// <returns>The list of all discovered orphaned files.</returns>
    Task<IEnumerable<OrphanedFile>> EnumerateAllOrphanedFiles(IDbConnection connection);

    /// <summary>
    /// List all files from the given folder.
    /// </summary>
    /// <param name="connection">The connection to the database.</param>
    /// <param name="parent">The parent folder.</param>
    /// <param name="loadWorkingCopies">Load the files from the working drive.</param>
    /// <returns>A list of files.</returns>
    Task<IEnumerable<OrphanedFile>> EnumerateOrphanedFilesByFolderAsync(
        IDbConnection connection,
        Folder parent,
        bool loadWorkingCopies);
}
