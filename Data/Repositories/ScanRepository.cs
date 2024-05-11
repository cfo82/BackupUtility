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
    private readonly ISettingsRepository _settingsRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScanRepository"/> class.
    /// </summary>
    /// <param name="context">The database conect to use for this repository.</param>
    /// <param name="settingsRepository">The repository to store settings.</param>
    public ScanRepository(DbContextData context, ISettingsRepository settingsRepository)
    {
        _context = context;
        _settingsRepository = settingsRepository;
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
                        SettingsId INTEGER NOT NULL,
                        CreatedDate TEXT NOT NULL,
                        StartDate TEXT,
                        FinishedDate TEXT,
                        StageFolderScanFinished INTEGER NOT NULL DEFAULT 0,
                        FolderScanStartDate TEXT,
                        FolderScanFinishedDate TEXT,
                        StageFileScanInitialized INTEGER NOT NULL DEFAULT 0,
                        StageFileScanFinished INTEGER NOT NULL DEFAULT 0,
                        FileScanStartDate TEXT,
                        FileScanFinishedDate TEXT,
                        StageDuplicateFileAnalysisFinished INTEGER NOT NULL DEFAULT 0,
                        DuplicateFileAnalysisStartDate TEXT,
                        DuplicateFileAnalysisFinishedDate TEXT,
                        StageOrphanedFileEnumerationFinished INTEGER NOT NULL DEFAULT 0,
                        OrphanedFileEnumerationStartDate TEXT,
                        OrphanedFileEnumerationFinishedDate TEXT,
                        FOREIGN KEY(SettingsId) REFERENCES Settings(SettingsID) ON DELETE CASCADE ON UPDATE NO ACTION);");

                await connection.ExecuteAsync(@"CREATE INDEX Scans_SettingsID ON Scans(SettingsID);");
                break;
            }
        }
    }

    /// <inheritdoc />
    public Task<Scan?> GetCurrentScan(IDbConnection connection)
    {
        return connection.QueryFirstOrDefaultAsync<Scan>(
            @"SELECT
                Id,
                SettingsId,
                CreatedDate,
                StartDate,
                FinishedDate,
                StageFolderScanFinished,
                FolderScanStartDate,
                FolderScanFinishedDate,
                StageFileScanInitialized,
                StageFileScanFinished,
                FileScanStartDate,
                FileScanFinishedDate,
                StageDuplicateFileAnalysisFinished,
                DuplicateFileAnalysisStartDate,
                DuplicateFileAnalysisFinishedDate,
                StageOrphanedFileEnumerationFinished,
                OrphanedFileEnumerationStartDate,
                OrphanedFileEnumerationFinishedDate
            FROM
                Scans
            ORDER BY
                CreatedDate DESC
            LIMIT 1");
    }

    /// <inheritdoc />
    public async Task<Scan> CreateScanAsync(IDbConnection connection)
    {
        using var transaction = connection.BeginTransaction();

        var settings = await _settingsRepository.CreateCopyForScan(connection);

        var scan = new Scan()
        {
            SettingsId = settings.SettingsId,
            CreatedDate = DateTime.Now,
            StartDate = null,
            FinishedDate = null,
            StageFolderScanFinished = false,
            FolderScanStartDate = null,
            FolderScanFinishedDate = null,
            StageFileScanInitialized = false,
            StageFileScanFinished = false,
            FileScanStartDate = null,
            FileScanFinishedDate = null,
            StageDuplicateFileAnalysisFinished = false,
            DuplicateFileAnalysisStartDate = null,
            DuplicateFileAnalysisFinishedDate = null,
            StageOrphanedFileEnumerationFinished = false,
            OrphanedFileEnumerationStartDate = null,
            OrphanedFileEnumerationFinishedDate = null,
        };

        scan.Id = await connection.QuerySingleAsync<long>(
            @"INSERT INTO Scans(
                SettingsId,
                CreatedDate,
                StartDate,
                FinishedDate,
                StageFolderScanFinished,
                FolderScanStartDate,
                FolderScanFinishedDate,
                StageFileScanInitialized,
                StageFileScanFinished,
                FileScanStartDate,
                FileScanFinishedDate,
                StageDuplicateFileAnalysisFinished,
                DuplicateFileAnalysisStartDate,
                DuplicateFileAnalysisFinishedDate,
                StageOrphanedFileEnumerationFinished,
                OrphanedFileEnumerationStartDate,
                OrphanedFileEnumerationFinishedDate
            )
            VALUES (
                @SettingsId,
                @CreatedDate,
                @StartDate,
                @FinishedDate,
                @StageFolderScanFinished,
                @FolderScanStartDate,
                @FolderScanFinishedDate,
                @StageFileScanInitialized,
                @StageFileScanFinished,
                @FileScanStartDate,
                @FileScanFinishedDate,
                @StageDuplicateFileAnalysisFinished,
                @DuplicateFileAnalysisStartDate,
                @DuplicateFileAnalysisFinishedDate,
                @StageOrphanedFileEnumerationFinished,
                @OrphanedFileEnumerationStartDate,
                @OrphanedFileEnumerationFinishedDate
            );
            SELECT last_insert_rowid();",
            scan);

        transaction.Commit();

        return scan;
    }

    /// <inheritdoc />
    public async Task SaveScanAsync(IDbConnection connection, Scan scan)
    {
        await connection.ExecuteAsync(
            @"UPDATE Scans SET
                SettingsId = @SettingsId,
                CreatedDate = @CreatedDate,
                StartDate = @StartDate,
                FinishedDate = @FinishedDate,
                StageFolderScanFinished = @StageFolderScanFinished,
                FolderScanStartDate = @FolderScanStartDate,
                FolderScanFinishedDate = @FolderScanFinishedDate,
                StageFileScanInitialized = @StageFileScanInitialized,
                StageFileScanFinished = @StageFileScanFinished,
                FileScanStartDate = @FileScanStartDate,
                FileScanFinishedDate = @FileScanFinishedDate,
                StageDuplicateFileAnalysisFinished = @StageDuplicateFileAnalysisFinished,
                DuplicateFileAnalysisStartDate = @DuplicateFileAnalysisStartDate,
                DuplicateFileAnalysisFinishedDate = @DuplicateFileAnalysisFinishedDate,
                StageOrphanedFileEnumerationFinished = @StageOrphanedFileEnumerationFinished,
                OrphanedFileEnumerationStartDate = @OrphanedFileEnumerationStartDate,
                OrphanedFileEnumerationFinishedDate = @OrphanedFileEnumerationFinishedDate
            WHERE
                Id = @Id",
            scan);
    }
}
