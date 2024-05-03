namespace Data.IntegTest;

using System.IO;
using BackupUtilities.Data.Repositories;
using Microsoft.Data.Sqlite;

/// <summary>
/// This class is a template for tests that need the database as a file.
/// </summary>
public class WithDbAsFileTest
{
    private string _databaseName = string.Empty;
    private DbContextData? _dbContext = null;

    /// <summary>
    /// Initialize a test: Setup a new temporary database.
    /// </summary>
    /// <returns>A task for async programming.</returns>
    [TestInitialize]
    public async Task InitializeTemporaryDatabaseAsync()
    {
        _databaseName = Path.GetTempFileName();
        _dbContext = new DbContextData(_databaseName);
        await _dbContext.InitAsync();
    }

    /// <summary>
    /// Deletes the temporary database that has been created for the test.
    /// </summary>
    [TestCleanup]
    public void CleanupTemporaryDatabase()
    {
        _dbContext?.Dispose();
        _dbContext = null;
        SqliteConnection.ClearAllPools();
        System.IO.File.Delete(_databaseName);
    }
}
