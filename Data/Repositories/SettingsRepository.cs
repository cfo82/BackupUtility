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
    public async Task InitAsync(int version)
    {
        switch (version)
        {
        case 0:
            {
                await _context.Connection.ExecuteAsync(
                    @"CREATE TABLE Settings(
                        SettingsId INTEGER PRIMARY KEY AUTOINCREMENT,
                        RootPath TEXT NOT NULL,
                        MirrorPath TEXT NOT NULL);");

                await _context.Connection.ExecuteAsync(
                    @"CREATE TABLE IgnoredFolders(
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        SettingsId INTEGER NOT NULL,
                        Path TEXT NOT NULL,
                        FOREIGN KEY(SettingsId) REFERENCES Settings(SettingsId) ON DELETE CASCADE ON UPDATE NO ACTION
                    );");

                await _context.Connection.ExecuteAsync(@"INSERT INTO Settings(RootPath, MirrorPath) VALUES ('', '');");
                break;
            }
        }
    }

    /// <inheritdoc />
    public async Task<Settings> GetSettingsAsync(Scan? scan)
    {
        var settings = await _context.Connection.QuerySingleAsync<Settings>(
            @$"SELECT
                SettingsId,
                RootPath,
                MirrorPath
            FROM
                Settings
            WHERE
                SettingsId = @SettingsId;",
            new { SettingsId = scan != null ? scan.SettingsId : 1 });

        settings.IgnoredFolders = (await _context.Connection.QueryAsync<IgnoredFolder>(
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
    public async Task SaveSettingsAsync(Settings settings)
    {
        using var transaction = _context.Connection.BeginTransaction();

        await _context.Connection.ExecuteAsync(
            @"UPDATE Settings SET
                RootPath = @RootPath,
                MirrorPath = @MirrorPath
            WHERE
                SettingsId = @SettingsId",
            settings);

        await _context.Connection.ExecuteAsync("DELETE FROM IgnoredFolders WHERE SettingsId = @SettingsId;", settings);

        foreach (var ignoredDirectory in settings.IgnoredFolders)
        {
            await _context.Connection.ExecuteAsync(
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
    public async Task<Settings> CreateCopyForScan()
    {
        var settings = await GetSettingsAsync(null);

        settings.SettingsId = await _context.Connection.QuerySingleAsync<int>(
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
            await _context.Connection.ExecuteAsync(
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
