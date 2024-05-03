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
    /// Save new settings for this project.
    /// </summary>
    /// <param name="settings">The settings that are to be saved.</param>
    /// <returns>Returns the settings as they have been saved.</returns>
    Task<Settings> SaveSettingsAsync(Settings settings);
}
