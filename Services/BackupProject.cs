namespace BackupUtilities.Services;

using System;
using BackupUtilities.Data.Interfaces;
using BackupUtilities.Data.Repositories;
using BackupUtilities.Services.Interfaces;

/// <summary>
/// Default implementation of <see cref="IBackupProject"/>.
/// </summary>
public class BackupProject : IBackupProject
{
    private DbContextData _data;
    private Settings _settings;
    private bool _isReady;
    private bool _disposedValue;

    /// <summary>
    /// Initializes a new instance of the <see cref="BackupProject"/> class.
    /// </summary>
    /// <param name="data">The database of this project file.</param>
    /// <param name="settings">The settings of the project.</param>
    private BackupProject(DbContextData data, Settings settings)
    {
        _data = data;
        _settings = settings;
        UpdateIsReady();
    }

    /// <inheritdoc />
    public event EventHandler<EventArgs>? IsReadyChanged;

    /// <inheritdoc />
    public IDbContextData Data => _data;

    /// <inheritdoc />
    public Settings Settings => _settings;

    /// <inheritdoc />
    public bool IsReady
    {
        get
        {
            return _isReady;
        }

        set
        {
            if (_isReady != value)
            {
                _isReady = value;
                IsReadyChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    /// <summary>
    /// Construction of a <see cref="BackupProject"/> instance needs async code to read the settings. Therefore
    /// you need to use this method to create an instance.
    /// </summary>
    /// <param name="data">The database of the project file.</param>
    /// <returns>The newly created project.</returns>
    public static async Task<BackupProject> CreateBackupProjectAsync(DbContextData data)
    {
        var settings = await data.SettingsRepository.GetSettingsAsync(data.Connection, null);
        return new BackupProject(data, settings);
    }

    /// <inheritdoc />
    public async Task<Settings> SaveSettingsAsync(Settings settings)
    {
        await _data.SettingsRepository.SaveSettingsAsync(_data.Connection, settings);
        _settings = await _data.SettingsRepository.GetSettingsAsync(_data.Connection, null);

        UpdateIsReady();
        return _settings;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Dispose this object.
    /// </summary>
    /// <param name="disposing">Flag indicating whether this method was called from <see cref="Dispose()"/>.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _data.Dispose();
            }

            _disposedValue = true;
        }
    }

    private void UpdateIsReady()
    {
        IsReady = System.IO.Directory.Exists(_settings.RootPath)
            && System.IO.Directory.Exists(_settings.MirrorPath);
    }
}
