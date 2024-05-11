namespace BackupUtilities.Data.Interfaces;

using System.Data;
using System.Threading.Tasks;

/// <summary>
/// This interface represents a repository that stores the <see cref="Settings"/> for this database.
/// </summary>
public interface ISettingsRepository : IRepository
{
    /// <summary>
    /// Return the settings that are stored inside the database.
    /// </summary>
    /// <param name="connection">The database connection representing the database.</param>
    /// <param name="scan">Get the settings for this scan. If its null it returns the currently valid settings.</param>
    /// <returns>The settings.</returns>
    Task<Settings> GetSettingsAsync(IDbConnection connection, Scan? scan);

    /// <summary>
    /// Updates the settings inside the database.
    /// </summary>
    /// <param name="connection">The database connection representing the database.</param>
    /// <param name="settings">The settings to be saved.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SaveSettingsAsync(IDbConnection connection, Settings settings);

    /// <summary>
    /// Create a copy of the current settings (where ScanId == NULL) and save them with the scan.
    /// </summary>
    /// <param name="connection">The database connection representing the database.</param>
    /// <returns>The new copy of the settings.</returns>
    Task<Settings> CreateCopyForScan(IDbConnection connection);
}
