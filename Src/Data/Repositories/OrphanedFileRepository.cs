namespace BackupUtilities.Data.Repositories;

using BackupUtilities.Data.Interfaces;
using Dapper;

/// <summary>
/// The default implementation of <see cref="IOrphanedFileRepository"/>.
/// </summary>
public class OrphanedFileRepository : IOrphanedFileRepository
{
    private readonly DbContextData _context;
    private readonly IFileRepository _fileRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="OrphanedFileRepository"/> class.
    /// </summary>
    /// <param name="context">The db contect that owns this repository.</param>
    /// <param name="fileRepository">The file repository of the current database.</param>
    public OrphanedFileRepository(DbContextData context, IFileRepository fileRepository)
    {
        _context = context;
        _fileRepository = fileRepository;
    }

    /// <inheritdoc />
    public async Task InitAsync(int version)
    {
        switch (version)
        {
        case 0:
            {
                await _context.Connection.ExecuteAsync(
                    @"CREATE TABLE OrphanedFiles(
                            ParentId INTEGER NOT NULL,
                            Name TEXT NOT NULL,
                            Size INTEGER NOT NULL DEFAULT 0,
                            Hash TEXT NOT NULL,
                            PRIMARY KEY (ParentId, Name)
                            FOREIGN KEY(ParentId) REFERENCES Folders(Id) ON DELETE CASCADE ON UPDATE NO ACTION
                        );");
                await _context.Connection.ExecuteAsync(@"CREATE INDEX OrphanedFiles_Hash ON OrphanedFiles(Hash);");
                break;
            }
        }
    }

    /// <inheritdoc />
    public async Task DeleteAllAsync()
    {
        await _context.Connection.ExecuteAsync(
            @"DELETE FROM OrphanedFiles;");
    }

    /// <inheritdoc />
    public async Task SaveOrphanedFileAsync(OrphanedFile orphanedFile)
    {
        await _context.Connection.ExecuteAsync(
            @"INSERT INTO OrphanedFiles(
                    ParentId,
                    Name,
                    Size,
                    Hash
                ) VALUES (
                    @ParentId,
                    @Name,
                    @Size,
                    @Hash
                );",
            orphanedFile);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<OrphanedFile>> EnumerateAllOrphanedFiles()
    {
        return await _context.Connection.QueryAsync<OrphanedFile>(
            @"SELECT
                    ParentId,
                    Name,
                    Size,
                    Hash,
                    (SELECT COUNT(*) FROM Files B WHERE B.Hash = A.Hash) as NumCopiesOnLiveDrive
                FROM
                    OrphanedFiles A;");
    }

    /// <inheritdoc />
    public async Task<IEnumerable<OrphanedFile>> EnumerateOrphanedFilesByFolderAsync(
        Folder parent,
        bool loadWorkingCopies)
    {
        if (!loadWorkingCopies)
        {
            return await _context.Connection.QueryAsync<OrphanedFile>(
                @"SELECT
                    ParentId,
                    Name,
                    Size,
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
            var orphanedFiles = await _context.Connection.QueryAsync<OrphanedFile>(
                @"SELECT
                    ParentId,
                    Name,
                    Size,
                    Hash
                FROM
                    OrphanedFiles
                WHERE
                    ParentId = @Id",
                parent);

            foreach (var file in orphanedFiles)
            {
                var duplicates = await _fileRepository.EnumerateDuplicatesWithHash(file.Hash);
                file.DuplicatesOnLifeDrive.AddRange(duplicates);
                file.NumCopiesOnLiveDrive = file.DuplicatesOnLifeDrive.Count;
            }

            return orphanedFiles;
        }
    }
}
