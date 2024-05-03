namespace BackupUtilities.Data.Interfaces;

using System.Data;

/// <summary>
/// Repository to manage bitrot.
/// </summary>
public interface IBitRotRepository : IRepository
{
    /// <summary>
    /// Clear all bitrot entries.
    /// </summary>
    /// <param name="connection">The connection to the database.</param>
    /// <returns>A task for async processing.</returns>
    Task ClearAsync(IDbConnection connection);

    /// <summary>
    /// Create a new bitrot entry.
    /// </summary>
    /// <param name="connection">The connection to the database.</param>
    /// <param name="file">The file for which bitrot has been detected.</param>
    /// <returns>The new bitrot instance.</returns>
    Task<BitRot> CreateBitRotAsync(IDbConnection connection, File file);

    /// <summary>
    /// Delete all bitrot elements from the repository.
    /// </summary>
    /// <param name="connection">The connection to the database.</param>
    /// <returns>A task for async processing.</returns>
    Task DeleteAllAsync(IDbConnection connection);
}
