namespace BackupUtilities.Services.Interfaces;

using System.Threading.Tasks;
using BackupUtilities.Data.Interfaces;

/// <summary>
/// An interface to enumerate and store folders.
/// </summary>
public interface IFolderEnumerator
{
    /// <summary>
    /// Enumerate all sub-folders inside the settings root folder and stores them inside the database
    /// using <see cref="IFolderRepository"/>.
    /// </summary>
    /// <returns>A task for async programming.</returns>
    Task EnumerateFoldersAsync();
}
