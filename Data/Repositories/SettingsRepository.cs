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
                        ScanId INTEGER DEFAULT NULL,
                        RootPath TEXT NOT NULL,
                        MirrorPath TEXT NOT NULL,
                        FOREIGN KEY(ScanId) REFERENCES Scans(Id) ON DELETE CASCADE ON UPDATE NO ACTION);");

                await connection.ExecuteAsync(
                    @"CREATE TABLE IgnoredFolders(
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        SettingsId INTEGER NOT NULL,
                        Path TEXT NOT NULL,
                        FOREIGN KEY(SettingsId) REFERENCES Settings(SettingsId) ON DELETE CASCADE ON UPDATE NO ACTION
                    );");

                await connection.ExecuteAsync(@"CREATE INDEX Settings_ScanId ON Settings(ScanId);");
                break;
            }
        }
    }

    /// <inheritdoc />
    public async Task<Settings> GetSettingsAsync(IDbConnection connection, Scan? scan)
    {
        var equalsTo = scan == null ? "IS NULL" : "= @ScanId";
        var settings = await connection.QuerySingleOrDefaultAsync<Settings>(
            @$"SELECT
                SettingsId,
                ScanId,
                RootPath,
                MirrorPath
            FROM
                Settings
            WHERE
                ScanId {equalsTo};",
            new { ScanId = scan?.Id });

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
    public async Task SaveSettingsAsync(IDbConnection connection, Settings settings)
    {
        using var transaction = connection.BeginTransaction();

        var exists = await connection.ExecuteScalarAsync<bool>(
            @"SELECT 1 WHERE EXISTS(
            SELECT 1 FROM
                Settings
            WHERE
                SettingsId = @SettingsId);",
            settings);
        if (exists)
        {
            await connection.ExecuteAsync(
                @"UPDATE Settings SET
                    ScanId = @ScanId,
                    RootPath = @RootPath,
                    MirrorPath = @MirrorPath
                WHERE
                    SettingsId = @SettingsId",
                settings);
        }
        else
        {
            int settingsId = await connection.QuerySingleAsync<int>(
                @"INSERT INTO Settings(
                    ScanId,
                    RootPath,
                    MirrorPath
                )
                VALUES (
                    @ScanId,
                    @RootPath,
                    @MirrorPath
                );
                SELECT last_insert_rowid();",
                settings);
            settings.SettingsId = settingsId;
        }

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
}
