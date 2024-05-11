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

                await connection.ExecuteAsync(@"INSERT INTO Settings(RootPath, MirrorPath) VALUES ('', '');");
                break;
            }
        }
    }

    /// <inheritdoc />
    public async Task<Settings> GetSettingsAsync(IDbConnection connection, Scan? scan)
    {
        var settings = await connection.QuerySingleAsync<Settings>(
            @$"SELECT
                SettingsId,
                RootPath,
                MirrorPath
            FROM
                Settings
            WHERE
                SettingsId = @SettingsId;",
            new { SettingsId = scan != null ? scan.SettingsId : 1 });

        settings.IgnoredFolders = (await connection.QueryAsync<IgnoredFolder>(
            @"SELECT
                Path
            FROM
                IgnoredFolders
            WHERE
                SettingsId = @SettingsId;",
            settings)).ToList();

        return settings;
    }

    /// <inheritdoc />
    public async Task SaveSettingsAsync(IDbConnection connection, Settings settings)
    {
        using var transaction = connection.BeginTransaction();

        await connection.ExecuteAsync(
            @"UPDATE Settings SET
                RootPath = @RootPath,
                MirrorPath = @MirrorPath
            WHERE
                SettingsId = @SettingsId",
            settings);

        await connection.ExecuteAsync("DELETE FROM IgnoredFolders WHERE SettingsId = @SettingsId;", settings);

        foreach (var ignoredDirectory in settings.IgnoredFolders)
        {
            await connection.ExecuteAsync(
                @"INSERT INTO IgnoredFolders(
                    SettingsId,
                    Path
                )
                VALUES (
                    @SettingsId,
                    @Path
                );",
                new { SettingsId = settings.SettingsId, Path = ignoredDirectory.Path });
        }

        transaction.Commit();
    }

    /// <inheritdoc />
    public async Task<Settings> CreateCopyForScan(IDbConnection connection)
    {
        var settings = await GetSettingsAsync(connection, null);

        settings.SettingsId = await connection.QuerySingleAsync<int>(
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
            await connection.ExecuteAsync(
                @"INSERT INTO IgnoredFolders(
                    SettingsId,
                    Path
                )
                VALUES (
                    @SettingsId,
                    @Path
                );",
                new { SettingsId = settings.SettingsId, Path = ignoredDirectory.Path });
        }

        return settings;
    }
}
