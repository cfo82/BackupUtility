namespace BackupUtilities.Services.Interfaces.Scans;

using System.Threading.Tasks;
using BackupUtilities.Data.Interfaces;

/// <summary>
/// An interface to enumerate and store files. Precondition is that the folder enumerator has already enumerated all folders.
/// </summary>
public interface IFileScan
{
    /// <summary>
    /// Enumerate all files inside the settings root folder and its subfolders and stores them inside the database
    /// using <see cref="IFileRepository"/>.
    /// </summary>
    /// <param name="continueLastScan">A value indicating if the last scan should be continued.</param>
    /// <returns>A task for async programming.</returns>
    Task EnumerateFilesAsync(bool continueLastScan);
}
