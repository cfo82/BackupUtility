namespace BackupUtilities.Data.Repositories;

using System.Data;
using BackupUtilities.Data.Interfaces;
using Dapper;

/// <summary>
/// This repository manages the settings of the backup utilities.
/// </summary>
public class SettingsRepository : ISettingsRepository
{
    private readonly DbContextData _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsRepository"/> class.
    /// </summary>
    /// <param name="context">The database conect to use for this repository.</param>
    public SettingsRepository(DbContextData context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task InitAsync(IDbConnection connection, int version)
    {
        switch (version)
        {
        case 0:
            {
                await connection.ExecuteAsync(
                    @"CREATE TABLE Settings(
                        SettingsId INTEGER PRIMARY KEY AUTOINCREMENT,
                        RootPath TEXT NOT NULL,
                        MirrorPath TEXT NOT NULL);");

                await connection.ExecuteAsync(
                    @"CREATE TABLE IgnoredFolders(
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        SettingsId INTEGER NOT NULL,
                        Path TEXT NOT NULL,
                        FOREIGN KEY(SettingsId) REFERENCES Settings(SettingsId) ON DELETE CASCADE ON UPDATE NO ACTION
                    );");
                break;
            }
        }
    }

    /// <inheritdoc />
    public async Task<Settings> GetSettingsAsync(IDbConnection connection)
    {
        var settings = await connection.QuerySingleOrDefaultAsync<Settings>(
            @"SELECT
                SettingsId,
                RootPath,
                MirrorPath
            FROM
                Settings;");

        if (settings != null)
        {
            settings.IgnoredFolders = (await connection.QueryAsync<IgnoredFolder>(
                @"SELECT
                Path
            FROM
                IgnoredFolders
            WHERE
                SettingsId = @SettingsId;",
                settings)).ToList();
        }
        else
        {
            settings = new();
        }

        return settings;
    }

    /// <inheritdoc />
    public async Task UpdateSettingsAsync(IDbConnection connection, Settings settings)
    {
        using var transaction = connection.BeginTransaction();

        await connection.ExecuteAsync("DELETE FROM Settings;");
        await connection.ExecuteAsync("DELETE FROM IgnoredFolders;");

        int settingsId = await connection.QuerySingleAsync<int>(
            @"INSERT INTO Settings(
                RootPath,
                MirrorPath
            )
            VALUES (
                @RootPath,
                @MirrorPath
            );
            SELECT last_insert_rowid();",
            settings);
        foreach (var ignoredDirectory in settings.IgnoredFolders)
        {
            await connection.ExecuteAsync(@$"INSERT INTO IgnoredFolders(SettingsId, Path) VALUES ({settingsId}, @Path);", ignoredDirectory);
        }

        transaction.Commit();
    }
}
