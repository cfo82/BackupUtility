namespace Data.IntegTest;

using System.Threading.Tasks;
using BackupUtilities.Data.Repositories;
using Microsoft.Data.Sqlite;

/// <summary>
/// Base class for integration tests that need the database.
/// </summary>
public class InMemoryDbIntegrationTestBase
{
    private DbContextData? _dbContext = null;

    /// <summary>
    /// Gets the current database context.
    /// </summary>
    public DbContextData DbContext
    {
        get
        {
            return _dbContext ?? throw new InvalidOperationException("_dbContext must be initialized at this point.");
        }
    }

    /// <summary>
    /// Initialize a test: Setup a new temporary database.
    /// </summary>
    /// <returns>A task for async programming.</returns>
    [TestInitialize]
    public async Task InitializeTemporaryDatabaseAsync()
    {
        _dbContext = new DbContextData(":memory:");
        await _dbContext.InitAsync();
        await InitializeDataAsync(_dbContext);
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
    }

    /// <summary>
    /// Virtual method that can be overriden by test classes to initialize test data.
    /// </summary>
    /// <param name="dbContext">The database context.</param>
    /// <returns>A task for async programming.</returns>
    protected virtual Task InitializeDataAsync(DbContextData dbContext)
    {
        return Task.CompletedTask;
    }
}
