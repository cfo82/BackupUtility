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
    /// <returns>The settings.</returns>
    Task<Settings> GetSettingsAsync(IDbConnection connection);

    /// <summary>
    /// Updates the settings inside the database.
    /// </summary>
    /// <param name="connection">The database connection representing the database.</param>
    /// <param name="settings">The settings to be saved.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task UpdateSettingsAsync(IDbConnection connection, Settings settings);
}
