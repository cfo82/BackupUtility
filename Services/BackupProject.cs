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
    private IScan? _currentScan;
    private bool _disposedValue;

    /// <summary>
    /// Initializes a new instance of the <see cref="BackupProject"/> class.
    /// </summary>
    /// <param name="data">The database of this project file.</param>
    /// <param name="settings">The settings of the project.</param>
    /// <param name="currentScan">The current scan for this project.</param>
    private BackupProject(
        DbContextData data,
        Settings settings,
        IScan? currentScan)
    {
        _data = data;
        _settings = settings;
        _currentScan = currentScan;

        UpdateIsReady();
    }

    /// <inheritdoc />
    public event EventHandler<EventArgs>? IsReadyChanged;

    /// <inheritdoc />
    public event EventHandler<EventArgs>? CurrentScanChanged;

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

    /// <inheritdoc />
    public IScan? CurrentScan => _currentScan;

    /// <summary>
    /// Construction of a <see cref="BackupProject"/> instance needs async code to read the settings. Therefore
    /// you need to use this method to create an instance.
    /// </summary>
    /// <param name="data">The database of the project file.</param>
    /// <returns>The newly created project.</returns>
    public static async Task<BackupProject> CreateBackupProjectAsync(DbContextData data)
    {
        var settings = await data.SettingsRepository.GetSettingsAsync(null);
        var scanData = await data.ScanRepository.GetCurrentScan();
        IScan? scan = null;
        if (scanData != null)
        {
            var scanSettings = await data.SettingsRepository.GetSettingsAsync(scanData);
            scan = new Scan(data.ScanRepository, scanData, scanSettings);
        }

        return new BackupProject(data, settings, scan);
    }

    /// <inheritdoc />
    public async Task<Settings> SaveSettingsAsync(Settings settings)
    {
        await _data.SettingsRepository.SaveSettingsAsync(settings);
        _settings = await _data.SettingsRepository.GetSettingsAsync(null);

        UpdateIsReady();

        return _settings;
    }

    /// <inheritdoc />
    public async Task<IScan> CreateScanAsync()
    {
        if (!IsReady)
        {
            throw new InvalidOperationException("Project is not opened or not ready.");
        }

        var scanRepository = Data.ScanRepository;
        var settingsRepository = Data.SettingsRepository;

        var scan = await scanRepository.CreateScanAsync();
        var settings = await settingsRepository.GetSettingsAsync(scan);

        _currentScan = new Scan(scanRepository, scan, settings);

        CurrentScanChanged?.Invoke(this, EventArgs.Empty);

        return _currentScan;
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
