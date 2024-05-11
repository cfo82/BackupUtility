namespace BackupUtilities.Data.Repositories;

using System.Collections.Generic;
using System.Data;
using BackupUtilities.Data.Interfaces;
using Dapper;

/// <summary>
/// Repository to read and write folder information.
/// </summary>
public class FolderRepository : IFolderRepository
{
    private readonly DbContextData _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="FolderRepository"/> class.
    /// </summary>
    /// <param name="context">The database conect to use for this repository.</param>
    public FolderRepository(DbContextData context)
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
                    @"CREATE TABLE Folders(
                            Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                            ParentId INTEGER,
                            Name TEXT NOT NULL,
                            DriveType INTEGER NOT NULL DEFAULT 0,
                            Touched INTEGER NOT NULL DEFAULT 0,
                            Hash TEXT,
                            IsDuplicate INTEGER NOT NULL DEFAULT 0,
                            FOREIGN KEY(ParentId) REFERENCES Folders(Id) ON DELETE CASCADE ON UPDATE NO ACTION
                        );");
                await connection.ExecuteAsync(@"CREATE INDEX Folders_ParentId ON Folders(ParentId);");
                break;
            }
        }
    }

    /// <inheritdoc />
    public async Task SaveFolderAsync(IDbConnection connection, Folder folder)
    {
        folder.Id = await connection.QuerySingleAsync<long>(
            @"INSERT INTO Folders(
                ParentId,
                Name,
                Touched,
                Hash,
                IsDuplicate,
                DriveType
            )
            VALUES (
                @ParentId,
                @Name,
                @Touched,
                @Hash,
                @IsDuplicate,
                @DriveType
            );
            SELECT last_insert_rowid();",
            folder);
    }

    /// <inheritdoc />
    public async Task<Folder> SaveFullPathAsync(
        IDbConnection connection,
        string path,
        DriveType driveType)
    {
        if (!Path.IsPathRooted(path))
        {
            throw new ArgumentException($"The path '{path}' must be rooted.", nameof(path));
        }

        var existingFolder = await GetFolderAsync(connection, path);
        if (existingFolder != null)
        {
            return existingFolder;
        }

        if (Path.GetPathRoot(path) == path)
        {
            path = path.TrimEnd('\\');
            var folder = new Folder
            {
                Id = 0,
                ParentId = null,
                Name = path,
                DriveType = driveType,
            };
            await SaveFolderAsync(connection, folder);
            return folder;
        }
        else
        {
            var parentName = Path.GetDirectoryName(path);
            if (parentName == null)
            {
                throw new InvalidOperationException($"Unable to evaluate parent folder name for '{path}'.");
            }

            var parentFolder = await SaveFullPathAsync(connection, parentName, driveType);
            if (parentFolder == null)
            {
                throw new InvalidOperationException($"Unable to save parent folder '{path}'.");
            }

            var folder = new Folder
            {
                Id = 0,
                ParentId = parentFolder.Id,
                Name = Path.GetFileName(path),
                DriveType = driveType,
            };
            await SaveFolderAsync(connection, folder);
            return folder;
        }
    }

    /// <inheritdoc />
    public async Task MarkAllFoldersAsUntouchedAsync(IDbConnection connection)
    {
        await connection.ExecuteAsync(
            @"UPDATE Folders SET
                Touched = 0;");
    }

    /// <inheritdoc />
    public async Task<long> GetFolderCount(IDbConnection connection, DriveType driveType)
    {
        return await connection.QuerySingleAsync<long>(
            @"SELECT
                count(*)
            FROM
                Folders
            WHERE
                DriveType = @driveType",
            new { driveType });
    }

    /// <inheritdoc />
    public async Task TouchFolderAsync(IDbConnection connection, Folder folder)
    {
        await connection.ExecuteAsync(
            @"UPDATE Folders SET
                Touched = 1
            WHERE
                Id = @Id",
            folder);
        folder.Touched = 1;
    }

    /// <inheritdoc />
    public async Task<Folder?> GetFolderAsync(IDbConnection connection, long folderId)
    {
        return await connection.QuerySingleOrDefaultAsync<Folder>(
            @"SELECT
                    Id,
                    ParentId,
                    Name,
                    Touched,
                    Hash,
                    IsDuplicate,
                    DriveType
                FROM
                    Folders
                WHERE
                    Id = @folderId;",
            new { folderId });
    }

    /// <inheritdoc />
    public async Task<Folder?> GetFolderAsync(IDbConnection connection, string path)
    {
        if (!Path.IsPathRooted(path))
        {
            throw new ArgumentException($"The path '{path}' must be rooted.", nameof(path));
        }

        if (Path.GetPathRoot(path) == path)
        {
            path = path.TrimEnd('\\');
            return await connection.QuerySingleOrDefaultAsync<Folder>(
                @"SELECT
                    Id,
                    ParentId,
                    Name,
                    Touched,
                    Hash,
                    IsDuplicate,
                    DriveType
                FROM
                    Folders
                WHERE
                    Name = @path;",
                new { path });
        }
        else
        {
            var parentName = Path.GetDirectoryName(path);
            if (parentName == null)
            {
                throw new InvalidOperationException($"Unable to evaluate parent folder name for '{path}'.");
            }

            var parentFolder = await GetFolderAsync(connection, parentName);
            if (parentFolder == null)
            {
                return null;
            }

            return await GetFolderAsync(connection, parentFolder.Id, Path.GetFileName(path));
        }
    }

    /// <inheritdoc />
    public async Task<Folder?> GetFolderAsync(IDbConnection connection, long parentId, string name)
    {
        return await connection.QuerySingleOrDefaultAsync<Folder>(
            @"SELECT
                    Id,
                    ParentId,
                    Name,
                    Touched,
                    Hash,
                    IsDuplicate,
                    DriveType
                FROM
                    Folders
                WHERE
                    ParentId = @parentId AND
                    Name = @name;",
            new { parentId, name });
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Folder>> GetRootFolders(IDbConnection connection, DriveType driveType)
    {
        return await connection.QueryAsync<Folder>(
            @"SELECT
                Id,
                ParentId,
                Name,
                Touched,
                Hash,
                IsDuplicate,
                DriveType
            FROM
                Folders
            WHERE
                parentId IS NULL AND
                DriveType = @driveType;",
            new { driveType });
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Folder>> GetSubFoldersAsync(IDbConnection connection, Folder folder)
    {
        return await connection.QueryAsync<Folder>(
            @"SELECT
                Id,
                ParentId,
                Name,
                Touched,
                Hash,
                IsDuplicate,
                DriveType
            FROM
                Folders
            WHERE
                ParentId = @Id AND
                Touched = 1;",
            folder);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Folder>> GetFullPathForFolderAsync(IDbConnection connection, Folder folder)
    {
        return await GetFullPathForFolderAsync(connection, folder.Id);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Folder>> GetFullPathForFolderAsync(IDbConnection connection, long folderId)
    {
        return await connection.QueryAsync<Folder>(
            @"WITH RECURSIVE parent_folders(n) AS (
	            VALUES(@folderId)
	            UNION
	            SELECT ParentId FROM Folders, parent_folders
	            WHERE Folders.Id = parent_folders.n)
            SELECT 
	            Id,
                ParentId,
                Name,
                Touched,
                Hash,
                IsDuplicate,
                DriveType
            FROM
	            Folders
            WHERE
	            Folders.Id IN parent_folders;",
            new { folderId });
    }

    /// <inheritdoc />
    public async Task DeleteAllAsync(IDbConnection connection)
    {
        await connection.ExecuteAsync(
            @"DELETE FROM Folders;");
    }

    /// <inheritdoc />
    public async Task RemoveAllDuplicateMarks(IDbConnection connection, DriveType driveType)
    {
        await connection.ExecuteAsync(
            @"UPDATE Folders SET
                Hash = NULL
            WHERE
                DriveType = @driveType;",
            new { driveType });
        await connection.ExecuteAsync(
            @"UPDATE Folders SET
                IsDuplicate = 0
            WHERE
                DriveType = @driveType;",
            new { driveType });
    }

    /// <inheritdoc />
    public async Task MarkFolderAsDuplicate(IDbConnection connection, Folder folder, FolderDuplicationLevel duplicationLevel)
    {
        var folderId = folder.Id;
        await connection.ExecuteAsync(
            @"UPDATE Folders SET
                IsDuplicate = @duplicationLevel
            WHERE
                Id = @folderId",
            new { folderId, duplicationLevel });
        folder.IsDuplicate = duplicationLevel;
    }

    /// <inheritdoc />
    public async Task SaveFolderHashAsync(IDbConnection connection, Folder folder, string hash)
    {
        var folderId = folder.Id;
        await connection.ExecuteAsync(
            @"UPDATE Folders SET
                Hash = @hash
            WHERE
                Id = @folderId",
            new { folderId, hash });
        folder.Hash = hash;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Folder>> FindDuplicateFoldersAsync(IDbConnection connection, DriveType driveType)
    {
        return await connection.QueryAsync<Folder>(
                @"SELECT
                    a.Id,
                    a.ParentId,
                    a.Name,
                    a.Touched,
                    a.Hash,
                    a.IsDuplicate,
                    a.DriveType
                FROM
	                Folders a
                WHERE 
	                a.Hash IN (
	                SELECT
		                Hash
	                FROM
		                Folders
	                GROUP BY
		                Hash
	                HAVING
		                COUNT(*) > 1)
                ORDER BY
	                a.Hash;
                ");
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Folder>> EnumerateDuplicatesOfFolder(IDbConnection connection, Folder folder, DriveType driveType)
    {
        if (string.IsNullOrEmpty(folder.Hash))
        {
            return Enumerable.Empty<Folder>();
        }

        return await connection.QueryAsync<Folder>(
            @"SELECT
                Id,
                ParentId,
                Name,
                Touched,
                Hash,
                IsDuplicate,
                DriveType
            FROM
                Folders
            WHERE
                Hash = @Hash AND
                Id != @Id AND
                DriveType = @DriveType;",
            new
            {
                Hash = folder.Hash,
                Id = folder.Id,
                DriveType = driveType,
            });
    }
}
