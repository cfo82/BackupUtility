namespace BackupUtilities.Data.Repositories;

using System.Data;
using BackupUtilities.Data.Interfaces;
using Dapper;

/// <summary>
/// Default implementation of <see cref="IScanRepository"/>.
/// </summary>
public class ScanRepository : IScanRepository
{
    private readonly DbContextData _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScanRepository"/> class.
    /// </summary>
    /// <param name="context">The database conect to use for this repository.</param>
    public ScanRepository(DbContextData context)
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
                    @"CREATE TABLE Scans(
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        StartDate TEXT,
                        EndDate TEXT,
                        StageFolderScanFinished INTEGER NOT NULL DEFAULT 0,
                        StageFileScanInitialized INTEGER NOT NULL DEFAULT 0,
                        StageFileScanFinished INTEGER NOT NULL DEFAULT 0,
                        StageDuplicateFileAnalysisFinished INTEGER NOT NULL DEFAULT 0,
                        StageOrphanedFileEnumerationFinished INTEGER NOT NULL DEFAULT 0);");
                break;
            }
        }
    }
}
