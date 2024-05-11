namespace BackupUtilities.Services.Interfaces;

using System.Threading.Tasks;

/// <summary>
/// Run a complete scan which includes the folder scan, the file scan, the duplicate file analysis and finally the orphaned file scan.
/// </summary>
public interface ICompleteScan
{
    /// <summary>
    /// Run the analysis.
    /// </summary>
    /// <returns>A task for async programming.</returns>
    Task RunAsync();
}
