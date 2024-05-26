namespace BackupUtilities.Data.Repositories;

using System.Data;
using BackupUtilities.Data.Interfaces;
using Dapper;
using Microsoft.Data.Sqlite;

/// <summary>
/// Represents a single database.
/// </summary>
public class DbContextData : IDbContextData
{
    private SqliteConnection _connection;
    private bool _disposedValue;

    static DbContextData()
    {
        SqlMapper.RemoveTypeMap(typeof(DateTime));
        SqlMapper.AddTypeHandler(new SqliteDateTimeHandler());
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DbContextData"/> class.
    /// </summary>
    /// <param name="dbPath">The absolute path to the database.</param>
    public DbContextData(string dbPath)
    {
        _connection = new SqliteConnection($"Data Source={dbPath}");

        SettingsRepository = new SettingsRepository(this);
        ScanRepository = new ScanRepository(this, SettingsRepository);
        FolderRepository = new FolderRepository(this);
        FileRepository = new FileRepository(this);
        BitRotRepository = new BitRotRepository(this);
        OrphanedFileRepository = new OrphanedFileRepository(this, FileRepository);
    }

    /// <inheritdoc />
    public IDbConnection Connection => _connection;

    /// <inheritdoc />
    public IScanRepository ScanRepository { get; }

    /// <inheritdoc />
    public ISettingsRepository SettingsRepository { get; }

    /// <inheritdoc />
    public IFolderRepository FolderRepository { get; }

    /// <inheritdoc />
    public IFileRepository FileRepository { get; }

    /// <inheritdoc />
    public IBitRotRepository BitRotRepository { get; }

    /// <inheritdoc />
    public IOrphanedFileRepository OrphanedFileRepository { get; }

    /// <summary>
    /// Initialize the database. This applies all necessary migrations.
    /// </summary>
    /// <returns>A task for async processing.</returns>
    public async Task InitAsync()
    {
        await _connection.OpenAsync();

        while (true)
        {
            var version = await Connection.QuerySingleAsync<int>("PRAGMA schema_version;");

            if (version < 2)
            {
                using var transaction = _connection.BeginTransaction();

                await ScanRepository.InitAsync(version);
                await SettingsRepository.InitAsync(version);
                await FolderRepository.InitAsync(version);
                await FileRepository.InitAsync(version);
                await BitRotRepository.InitAsync(version);
                await OrphanedFileRepository.InitAsync(version);

                await Connection.ExecuteAsync($"PRAGMA schema_version = {version + 1};");

                transaction.Commit();
            }
            else
            {
                break;
            }
        }
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
                Connection.Dispose();
            }

            _disposedValue = true;
        }
    }
}
