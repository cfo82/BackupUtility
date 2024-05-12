namespace Data.IntegTest;

using System.Security.Cryptography;
using BackupUtilities.Data.Interfaces;
using BackupUtilities.Data.Repositories;
using Microsoft.Data.Sqlite;

/// <summary>
/// This class is used to test the <see cref="SettingsRepository"/> class.
/// </summary>
[TestClass]
public class SettingsRepositoryTest
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
    /// Test that empty settings are created when requested.
    /// </summary>
    /// <returns>A task for async programming.</returns>
    [TestMethod]
    public async Task Test_GetSettingsFromEmptyDb_ReturnsDefaultSettings()
    {
        // Arrange
        Assert.IsNotNull(_dbContext);
        var sut = new SettingsRepository(_dbContext);

        // Act
        var settings = await sut.GetSettingsAsync(null);

        // Assert
        Assert.AreEqual(settings.SettingsId, 1);
        Assert.AreEqual(string.Empty, settings.RootPath);
        Assert.AreEqual(string.Empty, settings.MirrorPath);
        Assert.AreEqual(0, settings.IgnoredFolders.Count);
    }

    /// <summary>
    /// Tests that settings are correctly stored inside the database.
    /// </summary>
    /// <returns>A task for async programming.</returns>
    [TestMethod]
    public async Task Test_UpdateSettingsAsync_Updated()
    {
        // Arrange
        Assert.IsNotNull(_dbContext);
        var sut = new SettingsRepository(_dbContext);
        var settings = new Settings()
        {
            SettingsId = 1,
            RootPath = "D:\\",
            MirrorPath = "E:\\",
            IgnoredFolders = new List<IgnoredFolder>()
            {
                new IgnoredFolder() { Path = "D:\\$RECYCLE.BIN", },
                new IgnoredFolder() { Path = "D:\\OneDriveTemp", },
            },
        };

        // Act
        await sut.SaveSettingsAsync(settings);
        var settingsRead = await sut.GetSettingsAsync(null);

        // Assert
        Assert.AreEqual(1, settingsRead.SettingsId);
        Assert.AreEqual("D:\\", settingsRead.RootPath);
        Assert.AreEqual("E:\\", settingsRead.MirrorPath);
        Assert.AreEqual(2, settingsRead.IgnoredFolders.Count);
        Assert.AreEqual("D:\\$RECYCLE.BIN", settingsRead.IgnoredFolders[0].Path);
        Assert.AreEqual("D:\\OneDriveTemp", settingsRead.IgnoredFolders[1].Path);
    }

    /// <summary>
    /// Verify that a hash can be created and converted back to the original value.
    /// </summary>
    [TestMethod]
    public void Test_Hash_Conversion()
    {
        var randomByteArray = new byte[512];
        Random.Shared.NextBytes(randomByteArray);

        using var sha = SHA512.Create();
        byte[] checksum = sha.ComputeHash(randomByteArray);
        var fullHash = BitConverter.ToString(checksum).Replace("-", string.Empty).ToLower();

        var convertedBack = Enumerable
                        .Range(0, fullHash.Length / 2)
                        .Select(i => fullHash.Substring(i * 2, 2))
                        .Select(s => Convert.ToByte(s, 16))
                        .ToArray();

        CollectionAssert.AreEqual(checksum, convertedBack);
    }
}
