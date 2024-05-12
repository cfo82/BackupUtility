namespace BackupUtilities.Services.Interfaces.Scans;

using System.Threading.Tasks;
using BackupUtilities.Data.Interfaces;

/// <summary>
/// An interface to search orphaned files. That means files on the Mirror drive that have been deleted on the
/// original drive and are therefore going to be deleted during the next robocopy /MIR.
/// </summary>
public interface IOrphanedFileEnumerator
{
    /// <summary>
    /// Enumerate all orphaned files on the mirror drive and its subfolders and store them inside the database
    /// using <see cref="IOrphanedFileRepository"/>.
    /// </summary>
    /// <returns>A task for async programming.</returns>
    Task EnumerateOrphanedFilesAsync();
}
