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
    private const string _emptyFileHash = "cf83e1357eefb8bdf1542850d66d8007d620e4050b5715dc83f4a921d36ce9ce47d0d13c5d85f2b0ff8318d2877eec2f63b931bd47417a81a538327af927da3e";
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
    public async Task InitAsync(int version)
    {
        switch (version)
        {
        case 0:
            {
                await _context.Connection.ExecuteAsync(
                    @"CREATE TABLE Files(
                            ParentId INTEGER NOT NULL,
                            Name TEXT NOT NULL,
                            IntroHash TEXT NOT NULL,
                            Hash TEXT NOT NULL,
                            LastWriteTime TEXT NOT NULL,
                            Touched INTEGER NOT NULL DEFAULT 0,
                            IsDuplicate INTEGER NOT NULL DEFAULT 0,
                            PRIMARY KEY (ParentId, Name)
                            FOREIGN KEY(ParentId) REFERENCES Folders(Id) ON DELETE CASCADE ON UPDATE NO ACTION
                        );");
                await _context.Connection.ExecuteAsync(@"CREATE INDEX Files_IntroHash ON Files(IntroHash);");
                await _context.Connection.ExecuteAsync(@"CREATE INDEX Files_Hash ON Files(Hash);");
                await _context.Connection.ExecuteAsync(@"CREATE INDEX Files_ParentId ON Files(ParentId);");
                break;
            }
        }
    }

    /// <inheritdoc />
    public async Task<File?> FindFileByNameAsync(Folder parent, string name)
    {
        long parentId = parent.Id;
        var result = await _context.Connection.QuerySingleOrDefaultAsync<File>(
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
    public async Task<IEnumerable<File>> EnumerateFilesByFolderAsync(Folder parent)
    {
        long parentId = parent.Id;
        var result = await _context.Connection.QueryAsync<File>(
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
    public async Task SaveFileAsync(File file)
    {
        var exists = await _context.Connection.ExecuteScalarAsync<bool>(
            @"SELECT 1 WHERE EXISTS(
                SELECT 1 FROM
                    Files
                WHERE
                    ParentId = @ParentId AND
                    Name = @Name);",
            file);
        if (exists)
        {
            await _context.Connection.ExecuteAsync(
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
            await _context.Connection.ExecuteAsync(
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
    public async Task MarkAllFilesAsUntouchedAsync()
    {
        await _context.Connection.ExecuteAsync(
            @"UPDATE Files SET
                Touched = 0;");
    }

    /// <inheritdoc />
    public async Task TouchFileAsync(File file)
    {
        await _context.Connection.ExecuteAsync(
            @"UPDATE Files SET
                Touched = 1
            WHERE
                ParentId = @ParentId AND
                Name = @Name;",
            file);
        file.Touched = 1;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<DuplicateFiles>> FindDuplicateFilesAsync()
    {
        var duplicates = await _context.Connection.QueryAsync<File>(
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
    public async Task<IEnumerable<string>> FindHashesOfDuplicateFilesAsync()
    {
        return await _context.Connection.QueryAsync<string>(
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
    public async Task DeleteAllAsync()
    {
        await _context.Connection.ExecuteAsync(
            @"DELETE FROM Files;");
    }

    /// <inheritdoc />
    public async Task RemoveAllDuplicateMarks()
    {
        await _context.Connection.ExecuteAsync(
            @"UPDATE Files SET
                IsDuplicate = 0;");
    }

    /// <inheritdoc />
    public async Task MarkFileAsDuplicate(File file)
    {
        await _context.Connection.ExecuteAsync(
            @"UPDATE Files SET
                IsDuplicate = 1
            WHERE
                ParentId = @ParentId AND
                Name = @Name;",
            file);
        file.IsDuplicate = 1;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<File>> EnumerateDuplicateFiles()
    {
        return await _context.Connection.QueryAsync<File>(
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
    public async Task<IEnumerable<File>> EnumerateDuplicatesOfFile(BaseFile file)
    {
        if (string.Equals(file.Hash, _emptyFileHash))
        {
            return Enumerable.Empty<File>();
        }

        return await _context.Connection.QueryAsync<File>(
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
    public async Task<IEnumerable<File>> EnumerateDuplicatesWithHash(string hash)
    {
        if (string.Equals(hash, _emptyFileHash))
        {
            return Enumerable.Empty<File>();
        }

        return await _context.Connection.QueryAsync<File>(
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
