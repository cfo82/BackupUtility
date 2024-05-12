namespace BackupUtilities.Data.Interfaces;

using System.Data;
using System.Threading.Tasks;

/// <summary>
/// This interface represents a repository that stores the <see cref="Scan"/> instances for this database.
/// </summary>
public interface IScanRepository : IRepository
{
    /// <summary>
    /// Get the current scan (the most recent one).
    /// </summary>
    /// <returns>The most recent scan entity.</returns>
    Task<Scan?> GetCurrentScan();

    /// <summary>
    /// Create a new empty scan.
    /// </summary>
    /// <returns>Thew new scan.</returns>
    Task<Scan> CreateScanAsync();

    /// <summary>
    /// Save the scan.
    /// </summary>
    /// <param name="scan">The scan to be saved.</param>
    /// <returns>A task for async programming.</returns>
    Task SaveScanAsync(Scan scan);
}
