namespace BackupUtilities.Data.Interfaces;

using System.Data;

/// <summary>
/// Repository to manage bitrot.
/// </summary>
public interface IBitRotRepository : IRepository
{
    /// <summary>
    /// Create a new bitrot entry.
    /// </summary>
    /// <param name="scan">The scan during that the bitrot was detected.</param>
    /// <param name="file">The file for which bitrot has been detected.</param>
    /// <returns>The new bitrot instance.</returns>
    Task<BitRot> CreateBitRotAsync(Scan scan, File file);

    /// <summary>
    /// Delete all bitrot elements from the repository.
    /// </summary>
    /// <param name="scan">The scan for which all bitrot instances should be cleared.</param>
    /// <returns>A task for async processing.</returns>
    Task DeleteAllAsync(Scan scan);
}
