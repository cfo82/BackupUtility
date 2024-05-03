namespace BackupUtilities.Services.Interfaces;

using System.Threading.Tasks;

/// <summary>
/// An interface to find duplicate files and folders.
/// </summary>
public interface IDuplicateFileAnalysis
{
    /// <summary>
    /// Use the database an run an analysis to find duplicate files and folders.
    /// </summary>
    /// <returns>A task for async programming.</returns>
    Task RunDuplicateFileAnalysis();
}
