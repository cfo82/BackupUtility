namespace BackupUtilities.Data.Repositories;

using System.Collections.Generic;
using System.Data;
using BackupUtilities.Data.Interfaces;
using Dapper;

/// <summary>
/// Repository to read and write file information.
/// </summary>
public class FileRepository : IFileRepository
{
    private readonly DbContextData _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileRepository"/> class.
    /// </summary>
    /// <param name="context">The database conect to use for this repository.</param>
    public FileRepository(DbContextData context)
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
                    @"CREATE TABLE Files(
                            ParentId INTEGER NOT NULL,
                            Name TEXT NOT NULL,
                            IntroHash TEXT,
                            Hash TEXT,
                            LastWriteTime TEXT,
                            Touched INTEGER,
                            PRIMARY KEY (ParentId, Name)
                            FOREIGN KEY(ParentId) REFERENCES Folders(Id) ON DELETE CASCADE ON UPDATE NO ACTION
                        );");
                await connection.ExecuteAsync(@"CREATE INDEX Files_IntroHash ON Files(IntroHash);");
                await connection.ExecuteAsync(@"CREATE INDEX Files_Hash ON Files(Hash);");
                break;
            }

        case 1:
            {
                await connection.ExecuteAsync(@"CREATE INDEX Files_ParentId ON Files(ParentId);");
                await connection.ExecuteAsync(@"ALTER TABLE Files ADD IsDuplicate INTEGER NOT NULL DEFAULT 0;");
                break;
            }
        }
    }

    /// <inheritdoc />
    public async Task<File?> FindFileByNameAsync(IDbConnection connection, Folder parent, string name)
    {
        long parentId = parent.Id;
        var result = await connection.QuerySingleOrDefaultAsync<File>(
            @"SELECT
                ParentId,
                Name,
                IntroHash,
                Hash,
                LastWriteTime,
                Touched,
                IsDuplicate
            FROM
                Files
            WHERE
                ParentId = @parentId AND
                Name = @name;",
            new { parentId, name, });

        return result;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<File>> EnumerateFilesByFolderAsync(IDbConnection connection, Folder parent)
    {
        long parentId = parent.Id;
        var result = await connection.QueryAsync<File>(
            @"SELECT
                ParentId,
                Name,
                IntroHash,
                Hash,
                LastWriteTime,
                Touched,
                IsDuplicate
            FROM
                Files
            WHERE
                ParentId = @parentId",
            new { parentId, });

        return result;
    }

    /// <inheritdoc />
    public async Task SaveFileAsync(IDbConnection connection, File file)
    {
        var exists = await connection.ExecuteScalarAsync<bool>(
            @"SELECT 1 WHERE EXISTS(
                SELECT 1 FROM
                    Files
                WHERE
                    ParentId = @ParentId AND
                    Name = @Name);",
            file);
        if (exists)
        {
            await connection.ExecuteAsync(
                @"UPDATE FILES SET
                    IntroHash = @IntroHash,
                    Hash = @Hash,
                    LastWriteTime = @LastWriteTime,
                    Touched = @Touched,
                    IsDuplicate = @IsDuplicate
                WHERE
                    ParentId = @ParentId AND
                    Name = @Name;",
                file);
        }
        else
        {
            await connection.ExecuteAsync(
                @"INSERT INTO Files(
                    ParentId,
                    Name,
                    IntroHash,
                    Hash,
                    LastWriteTime,
                    Touched,
                    IsDuplicate
                ) VALUES (
                    @ParentId,
                    @Name,
                    @IntroHash,
                    @Hash,
                    @LastWriteTime,
                    @Touched,
                    @IsDuplicate
                );",
                file);
        }
    }

    /// <inheritdoc />
    public async Task MarkAllFilesAsUntouchedAsync(IDbConnection connection)
    {
        await connection.ExecuteAsync(
            @"UPDATE Files SET
                Touched = 0;");
    }

    /// <inheritdoc />
    public async Task TouchFileAsync(IDbConnection connection, File file)
    {
        await connection.ExecuteAsync(
            @"UPDATE Files SET
                Touched = 1
            WHERE
                ParentId = @ParentId AND
                Name = @Name;",
            file);
        file.Touched = 1;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<DuplicateFiles>> FindDuplicateFilesAsync(IDbConnection connection)
    {
        var duplicates = await connection.QueryAsync<File>(
                @"SELECT
	                a.ParentId,
	                a.Name,
	                a.IntroHash,
	                a.Hash,
	                a.LastWriteTime,
	                a.Touched,
                    a.IsDuplicate
                FROM
	                Files a
                WHERE 
	                a.Hash IN (
	                SELECT
		                Hash
	                FROM
		                Files
	                GROUP BY
		                Hash
	                HAVING
		                COUNT(*) > 1)
                ORDER BY
	                a.Hash;
                ");

        return duplicates
            .GroupBy(file => file.Hash)
            .Select(group => new DuplicateFiles { Hash = group.Key, Files = group });
    }

    /// <inheritdoc />
    public async Task<IEnumerable<string>> FindHashesOfDuplicateFilesAsync(IDbConnection connection)
    {
        return await connection.QueryAsync<string>(
            @"SELECT
                Hash
            FROM
                Files
            GROUP BY
                Hash
            HAVING
                COUNT(*) > 1;");
    }

    /// <inheritdoc />
    public async Task DeleteAllAsync(IDbConnection connection)
    {
        await connection.ExecuteAsync(
            @"DELETE FROM Files;");
    }

    /// <inheritdoc />
    public async Task RemoveAllDuplicateMarks(IDbConnection connection)
    {
        await connection.ExecuteAsync(
            @"UPDATE Files SET
                IsDuplicate = 0;");
    }

    /// <inheritdoc />
    public async Task MarkFileAsDuplicate(IDbConnection connection, File file)
    {
        await connection.ExecuteAsync(
            @"UPDATE Files SET
                IsDuplicate = 1
            WHERE
                ParentId = @ParentId AND
                Name = @Name;",
            file);
        file.IsDuplicate = 1;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<File>> EnumerateDuplicateFiles(IDbConnection connection)
    {
        return await connection.QueryAsync<File>(
            @"SELECT
                ParentId,
                Name,
                IntroHash,
                Hash,
                LastWriteTime,
                Touched,
                IsDuplicate
            FROM
                Files
            WHERE
                IsDuplicate = 1");
    }

    /// <inheritdoc />
    public async Task<IEnumerable<File>> EnumerateDuplicatesOfFile(IDbConnection connection, File file)
    {
        return await connection.QueryAsync<File>(
            @"SELECT
                ParentId,
                Name,
                IntroHash,
                Hash,
                LastWriteTime,
                Touched,
                IsDuplicate
            FROM
                Files
            WHERE
                Hash = @Hash AND
                (ParentId != @ParentId OR Name != @Name);",
            file);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<File>> EnumerateDuplicatesWithHash(IDbConnection connection, string hash)
    {
        return await connection.QueryAsync<File>(
            @"SELECT
                ParentId,
                Name,
                IntroHash,
                Hash,
                LastWriteTime,
                Touched,
                IsDuplicate
            FROM
                Files
            WHERE
                Hash = @hash",
            new { hash });
    }
}
