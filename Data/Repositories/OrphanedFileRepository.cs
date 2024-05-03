namespace BackupUtilities.Data.Repositories;

using System.Data;
using BackupUtilities.Data.Interfaces;
using Dapper;

/// <summary>
/// The default implementation of <see cref="IOrphanedFileRepository"/>.
/// </summary>
public class OrphanedFileRepository : IOrphanedFileRepository
{
    private readonly DbContextData _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="OrphanedFileRepository"/> class.
    /// </summary>
    /// <param name="context">The db contect that owns this repository.</param>
    public OrphanedFileRepository(DbContextData context)
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
                await connection.ExecuteAsync("CREATE TABLE OrphanedFiles(FullPath TEXT NOT NULL PRIMARY KEY);");
                break;
            }

        case 1:
            {
                await connection.ExecuteAsync("DROP TABLE OrphanedFiles;");
                await connection.ExecuteAsync(
                    @"CREATE TABLE OrphanedFiles(
                            ParentId INTEGER NOT NULL,
                            Name TEXT NOT NULL,
                            Hash TEXT NOT NULL,
                            PRIMARY KEY (ParentId, Name)
                            FOREIGN KEY(ParentId) REFERENCES Folders(Id) ON DELETE CASCADE ON UPDATE NO ACTION
                        );");
                await connection.ExecuteAsync(@"CREATE INDEX OrphanedFiles_Hash ON OrphanedFiles(Hash);");
                break;
            }
        }
    }

    /// <inheritdoc />
    public async Task DeleteAllAsync(IDbConnection connection)
    {
        await connection.ExecuteAsync(
            @"DELETE FROM OrphanedFiles;");
    }

    /// <inheritdoc />
    public async Task SaveOrphanedFileAsync(IDbConnection connection, OrphanedFile orphanedFile)
    {
        await connection.ExecuteAsync(
            @"INSERT INTO OrphanedFiles(
                    ParentId,
                    Name,
                    Hash
                ) VALUES (
                    @ParentId,
                    @Name,
                    @Hash
                );",
            orphanedFile);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<OrphanedFile>> EnumerateAllOrphanedFiles(IDbConnection connection)
    {
        return await connection.QueryAsync<OrphanedFile>(
            @"SELECT
                    ParentId,
                    Name,
                    Hash,
                    (SELECT COUNT(*) FROM Files B WHERE B.Hash = A.Hash) as NumCopiesOnLiveDrive
                FROM
                    OrphanedFiles A;");
    }

    /// <inheritdoc />
    public async Task<IEnumerable<OrphanedFile>> EnumerateOrphanedFilesByFolderAsync(
        IDbConnection connection,
        Folder parent,
        bool loadWorkingCopies)
    {
        if (!loadWorkingCopies)
        {
            return await connection.QueryAsync<OrphanedFile>(
                @"SELECT
                    ParentId,
                    Name,
                    Hash,
                    (SELECT COUNT(*) FROM Files B WHERE B.Hash = A.Hash) as NumCopiesOnLiveDrive
                FROM
                    OrphanedFiles A
                WHERE
                    A.ParentId = @Id;",
                parent);
        }
        else
        {
            var sql = @"
                SELECT
                    of.ParentId,
                    of.Name,
                    of.Hash,
                    f.ParentId,
                    f.Name,
                    f.IntroHash,
                    f.Hash,
                    f.LastWriteTime,
                    f.Touched,
                    f.IsDuplicate
                FROM
                    OrphanedFiles of
                LEFT JOIN
                    Files f
                ON
                    of.Hash = f.Hash
                WHERE
                    of.ParentId = @Id;";

            var resultSet = await connection.QueryAsync<OrphanedFile, File, OrphanedFile>(
                sql,
                (orphanedFile, file) =>
                {
                    if (file != null)
                    {
                        orphanedFile.DuplicatesOnLifeDrive.Add(file);
                    }

                    return orphanedFile;
                },
                parent,
                splitOn: "ParentId,Name");

            return resultSet.Select(
                file =>
                {
                    file.NumCopiesOnLiveDrive = file.DuplicatesOnLifeDrive.Count;
                    return file;
                });
        }
    }
}
