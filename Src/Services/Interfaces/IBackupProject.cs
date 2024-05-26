namespace BackupUtilities.Services.Interfaces;

using BackupUtilities.Data.Interfaces;

/// <summary>
/// Represents a backup utilities project.
/// </summary>
public interface IBackupProject : IDisposable
{
    /// <summary>
    /// Event that is fired when the value for IsReady has changed.
    /// </summary>
    event EventHandler<EventArgs>? IsReadyChanged;

    /// <summary>
    /// This event is fired whenever the current scan changed.
    /// </summary>
    event EventHandler<EventArgs>? CurrentScanChanged;

    /// <summary>
    /// Gets acess to the project data.
    /// </summary>
    IDbContextData Data { get; }

    /// <summary>
    /// Gets the settings of this project.
    /// </summary>
    Settings Settings { get; }

    /// <summary>
    /// Gets a value indicating whether the settings are complete and the project
    /// can run scans.
    /// </summary>
    bool IsReady { get; }

    /// <summary>
    /// Gets the current scan.
    /// </summary>
    IScan? CurrentScan { get; }

    /// <summary>
    /// Save new settings for this project.
    /// </summary>
    /// <param name="settings">The settings that are to be saved.</param>
    /// <returns>Returns the settings as they have been saved.</returns>
    Task<Settings> SaveSettingsAsync(Settings settings);

    /// <summary>
    /// Create a new scan. There is only one current scan at a time. So when creating a new scan
    /// the current scan is closed and the new scan becomes the current scan.
    /// </summary>
    /// <returns>Reference to the new scan.</returns>
    Task<IScan> CreateScanAsync();
}
