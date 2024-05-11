namespace BackupUtilities.Data.Repositories;

using System.Data;
using System.Threading.Tasks;
using BackupUtilities.Data.Interfaces;
using Dapper;

/// <summary>
/// Repository to read and write bitrot information.
/// </summary>
public class BitRotRepository : IBitRotRepository
{
    private readonly DbContextData _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="BitRotRepository"/> class.
    /// </summary>
    /// <param name="context">The database conect to use for this repository.</param>
    public BitRotRepository(DbContextData context)
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
                    @"CREATE TABLE BitRot(
                            Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                            ScanId INTEGER DEFAULT NULL,
                            FolderId INTEGER NOT NULL,
                            FileName TEXT NOT NULL,
                            FOREIGN KEY(ScanId) REFERENCES Scans(Id) ON DELETE CASCADE ON UPDATE NO ACTION,
                            FOREIGN KEY(FolderId, FileName) REFERENCES Files(ParentId, Name) ON DELETE NO ACTION ON UPDATE NO ACTION
                        );");

                await connection.ExecuteAsync(@"CREATE INDEX BitRot_ScanId ON BitRot(ScanId);");
                break;
            }
        }
    }

    /// <inheritdoc />
    public async Task<BitRot> CreateBitRotAsync(Scan scan, File file)
    {
        long bitrotId = await _context.Connection.QuerySingleAsync<long>(
            @"INSERT INTO BitRot(
                ScanId,
                FolderId,
                FileName
            )
            VALUES (
                @ScanId,
                @ParentId,
                @Name
            );
            SELECT last_insert_rowid();",
            new { ScanId = scan.Id, ParentId = file.ParentId, Name = file.Name });

        return await _context.Connection.QuerySingleAsync<BitRot>(
            @"SELECT
                Id,
                ScanId,
                FolderId,
                FileName
            FROM
                BitRot
            WHERE
                Id = @bitrotId",
            new { bitrotId });
    }

    /// <inheritdoc />
    public async Task DeleteAllAsync(Scan scan)
    {
        await _context.Connection.ExecuteAsync(
            @"DELETE FROM BitRot WHERE ScanId = @ScanId;",
            new { ScanId = scan.Id });
    }
}
