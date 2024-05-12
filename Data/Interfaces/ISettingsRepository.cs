namespace BackupUtilities.Data.Interfaces;

using System.Threading.Tasks;

/// <summary>
/// This interface represents a repository that stores the <see cref="Settings"/> for this database.
/// </summary>
public interface ISettingsRepository : IRepository
{
    /// <summary>
    /// Return the settings that are stored inside the database.
    /// </summary>
    /// <param name="scan">Get the settings for this scan. If its null it returns the currently valid settings.</param>
    /// <returns>The settings.</returns>
    Task<Settings> GetSettingsAsync(Scan? scan);

    /// <summary>
    /// Updates the settings inside the database.
    /// </summary>
    /// <param name="settings">The settings to be saved.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SaveSettingsAsync(Settings settings);

    /// <summary>
    /// Create a copy of the current settings (where ScanId == NULL) and save them with the scan.
    /// </summary>
    /// <returns>The new copy of the settings.</returns>
    Task<Settings> CreateCopyForScan();
}
