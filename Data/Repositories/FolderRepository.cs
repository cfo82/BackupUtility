namespace BackupUtilities.Data.Repositories;

using System.Collections.Generic;
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
    public async Task InitAsync(int version)
    {
        switch (version)
        {
        case 0:
            {
                await _context.Connection.ExecuteAsync(
                    @"CREATE TABLE Folders(
                            Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                            ParentId INTEGER,
                            Name TEXT NOT NULL,
                            Size INTEGER NOT NULL DEFAULT 0,
                            DriveType INTEGER NOT NULL DEFAULT 0,
                            Touched INTEGER NOT NULL DEFAULT 0,
                            Hash TEXT,
                            IsDuplicate INTEGER NOT NULL DEFAULT 0,
                            FOREIGN KEY(ParentId) REFERENCES Folders(Id) ON DELETE CASCADE ON UPDATE NO ACTION
                        );");
                await _context.Connection.ExecuteAsync(@"CREATE INDEX Folders_ParentId ON Folders(ParentId);");
                break;
            }
        }
    }

    /// <inheritdoc />
    public async Task SaveFolderAsync(Folder folder)
    {
        folder.Id = await _context.Connection.QuerySingleAsync<long>(
            @"INSERT INTO Folders(
                ParentId,
                Name,
                Size,
                Touched,
                Hash,
                IsDuplicate,
                DriveType
            )
            VALUES (
                @ParentId,
                @Name,
                @Size,
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
        string path,
        DriveType driveType)
    {
        if (!Path.IsPathRooted(path))
        {
            throw new ArgumentException($"The path '{path}' must be rooted.", nameof(path));
        }

        var existingFolder = await GetFolderAsync(path);
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
            await SaveFolderAsync(folder);
            return folder;
        }
        else
        {
            var parentName = Path.GetDirectoryName(path);
            if (parentName == null)
            {
                throw new InvalidOperationException($"Unable to evaluate parent folder name for '{path}'.");
            }

            var parentFolder = await SaveFullPathAsync(parentName, driveType);
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
            await SaveFolderAsync(folder);
            return folder;
        }
    }

    /// <inheritdoc />
    public async Task MarkAllFoldersAsUntouchedAsync()
    {
        await _context.Connection.ExecuteAsync(
            @"UPDATE Folders SET
                Touched = 0;");
    }

    /// <inheritdoc />
    public async Task<long> GetFolderCount(DriveType driveType)
    {
        return await _context.Connection.QuerySingleAsync<long>(
            @"SELECT
                count(*)
            FROM
                Folders
            WHERE
                DriveType = @driveType",
            new { driveType });
    }

    /// <inheritdoc />
    public async Task TouchFolderAsync(Folder folder)
    {
        await _context.Connection.ExecuteAsync(
            @"UPDATE Folders SET
                Touched = 1
            WHERE
                Id = @Id",
            folder);
        folder.Touched = 1;
    }

    /// <inheritdoc />
    public async Task<Folder?> GetFolderAsync(long folderId)
    {
        return await _context.Connection.QuerySingleOrDefaultAsync<Folder>(
            @"SELECT
                Id,
                ParentId,
                Name,
                Size,
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
    public async Task<Folder?> GetFolderAsync(string path)
    {
        if (!Path.IsPathRooted(path))
        {
            throw new ArgumentException($"The path '{path}' must be rooted.", nameof(path));
        }

        if (Path.GetPathRoot(path) == path)
        {
            path = path.TrimEnd('\\');
            return await _context.Connection.QuerySingleOrDefaultAsync<Folder>(
                @"SELECT
                    Id,
                    ParentId,
                    Name,
                    Size,
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

            var parentFolder = await GetFolderAsync(parentName);
            if (parentFolder == null)
            {
                return null;
            }

            return await GetFolderAsync(parentFolder.Id, Path.GetFileName(path));
        }
    }

    /// <inheritdoc />
    public async Task<Folder?> GetFolderAsync(long parentId, string name)
    {
        return await _context.Connection.QuerySingleOrDefaultAsync<Folder>(
            @"SELECT
                Id,
                ParentId,
                Name,
                Size,
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
    public async Task<IEnumerable<Folder>> GetRootFolders(DriveType driveType)
    {
        return await _context.Connection.QueryAsync<Folder>(
            @"SELECT
                Id,
                ParentId,
                Name,
                Size,
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
    public async Task<IEnumerable<Folder>> GetSubFoldersAsync(Folder folder)
    {
        return await _context.Connection.QueryAsync<Folder>(
            @"SELECT
                Id,
                ParentId,
                Name,
                Size,
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
    public async Task<IEnumerable<Folder>> GetFullPathForFolderAsync(Folder folder)
    {
        return await GetFullPathForFolderAsync(folder.Id);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Folder>> GetFullPathForFolderAsync(long folderId)
    {
        return await _context.Connection.QueryAsync<Folder>(
            @"WITH RECURSIVE parent_folders(n) AS (
	            VALUES(@folderId)
	            UNION
	            SELECT ParentId FROM Folders, parent_folders
	            WHERE Folders.Id = parent_folders.n)
            SELECT 
	            Id,
                ParentId,
                Name,
                Size,
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
    public async Task DeleteAllAsync()
    {
        await _context.Connection.ExecuteAsync(
            @"DELETE FROM Folders;");
    }

    /// <inheritdoc />
    public async Task RemoveAllDuplicateMarks(DriveType driveType)
    {
        await _context.Connection.ExecuteAsync(
            @"UPDATE Folders SET
                Hash = NULL
            WHERE
                DriveType = @driveType;",
            new { driveType });
        await _context.Connection.ExecuteAsync(
            @"UPDATE Folders SET
                IsDuplicate = 0
            WHERE
                DriveType = @driveType;",
            new { driveType });
    }

    /// <inheritdoc />
    public async Task MarkFolderAsDuplicate(Folder folder, FolderDuplicationLevel duplicationLevel)
    {
        await _context.Connection.ExecuteAsync(
            @"UPDATE Folders SET
                IsDuplicate = @duplicationLevel
            WHERE
                Id = @folderId",
            new { folderId = folder.Id, duplicationLevel });
        folder.IsDuplicate = duplicationLevel;
    }

    /// <inheritdoc />
    public async Task SaveFolderHashAsync(Folder folder, string hash)
    {
        await _context.Connection.ExecuteAsync(
            @"UPDATE Folders SET
                Hash = @hash
            WHERE
                Id = @folderId",
            new { folderId = folder.Id, hash });
        folder.Hash = hash;
    }

    /// <inheritdoc />
    public async Task SaveFolderSizeAsync(Folder folder, long size)
    {
        await _context.Connection.ExecuteAsync(
            @"UPDATE Folders SET
                Size = @size
            WHERE
                Id = @folderId",
            new { folderId = folder.Id, size });
        folder.Size = size;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Folder>> FindDuplicateFoldersAsync(DriveType driveType)
    {
        return await _context.Connection.QueryAsync<Folder>(
                @"SELECT
                    a.Id,
                    a.ParentId,
                    a.Name,
                    a.Size,
                    a.Touched,
                    a.Hash,
                    a.IsDuplicate,
                    a.DriveType
                FROM
	                Folders a
                WHERE
                    DriveType = @DriveType AND
	                a.Hash IN (
	                SELECT
		                Hash
	                FROM
		                Folders
                    WHERE
                        DriveType = @DriveType
	                GROUP BY
		                Hash
	                HAVING
		                COUNT(*) > 1)
                ORDER BY
	                a.Hash;",
                new { DriveType = driveType });
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Folder>> EnumerateDuplicatesOfFolder(Folder folder, DriveType driveType)
    {
        if (string.IsNullOrEmpty(folder.Hash))
        {
            return Enumerable.Empty<Folder>();
        }

        return await _context.Connection.QueryAsync<Folder>(
            @"SELECT
                Id,
                ParentId,
                Name,
                Size,
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
