namespace Data.IntegTest;

using BackupUtilities.Data.Interfaces;
using BackupUtilities.Data.Repositories;
using Microsoft.Data.Sqlite;

/// <summary>
/// This class is used to test the <see cref="FolderRepository"/> class.
/// </summary>
[TestClass]
public class FolderRepositoryTest
{
    private DbContextData? _dbContext = null;

    /// <summary>
    /// Initialize a test: Setup a new temporary database.
    /// </summary>
    /// <returns>A task for async programming.</returns>
    [TestInitialize]
    public async Task InitializeTemporaryDatabaseAsync()
    {
        _dbContext = new DbContextData(":memory:");
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
    }

    /// <summary>
    /// This test verifies that a path can be read from the database given its full path.
    /// </summary>
    /// <returns>A task for async programming.</returns>
    [TestMethod]
    public async Task Test_GetRootFolder_Async()
    {
        // Arrange
        Assert.IsNotNull(_dbContext);
        var sut = _dbContext.FolderRepository;
        await sut.SaveFullPathAsync(@"D:\Test\Child", DriveType.Working);

        // Act
        var result = await sut.GetFolderAsync(@"D:\Test\Child");

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(3, result.Id);
        Assert.AreEqual(2, result.ParentId);
        Assert.AreEqual("Child", result.Name);
    }

    /// <summary>
    /// This path verifies that the method <see cref="IFolderRepository.GetFullPathForFolderAsync(Folder)"/> returnes the
    /// correct absolute path for the given folder.
    /// </summary>
    /// <returns>A task for async programming.</returns>
    [TestMethod]
    public async Task Test_GetFullPathForFolderAsync_Async()
    {
        // Arrange
        Assert.IsNotNull(_dbContext);
        var sut = _dbContext.FolderRepository;
        await sut.SaveFullPathAsync(@"D:\Test\Child", DriveType.Working);
        var leaf = await sut.GetFolderAsync(@"D:\Test\Child");
        Assert.IsNotNull(leaf);

        // Act
        var fullPath = (await sut.GetFullPathForFolderAsync(leaf)).ToList();

        // Assert
        Assert.AreEqual(3, fullPath.Count());
        Assert.AreEqual("D:", fullPath[0].Name);
        Assert.AreEqual("Test", fullPath[1].Name);
        Assert.AreEqual("Child", fullPath[2].Name);
    }
}
