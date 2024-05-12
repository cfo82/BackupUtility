namespace BackupUtilities.Services.Services;

using System;
using System.Data;
using System.Threading.Tasks;
using BackupUtilities.Data.Interfaces;
using BackupUtilities.Services.Interfaces;

/// <summary>
/// Default implementation of <see cref="IScan"/>.
/// </summary>
public class Scan : IScan
{
    private readonly IScanRepository _scanRepository;
    private Data.Interfaces.Scan _scan;
    private Settings _settings;

    /// <summary>
    /// Initializes a new instance of the <see cref="Scan"/> class.
    /// </summary>
    /// <param name="scanRepository">The scan repository.</param>
    /// <param name="scan">The data value that this object encapsulates.</param>
    /// <param name="settings">The settings for the scan.</param>
    public Scan(
        IScanRepository scanRepository,
        Data.Interfaces.Scan scan,
        Settings settings)
    {
        _scanRepository = scanRepository;
        _scan = scan;
        _settings = settings;
    }

    /// <inheritdoc />
    public event EventHandler<EventArgs>? Changed;

    /// <inheritdoc />
    public Settings Settings => _settings;

    /// <inheritdoc />
    public Data.Interfaces.Scan Data => _scan;

    /// <inheritdoc />
    public async Task UpdateFullScanDataAsync(
        IDbConnection connection,
        DateTime? startDate,
        DateTime? finishedDate)
    {
        using var transaction = connection.BeginTransaction();

        _scan.StartDate = startDate;
        _scan.FinishedDate = finishedDate;

        await _scanRepository.SaveScanAsync(_scan);

        transaction.Commit();

        Changed?.Invoke(this, EventArgs.Empty);
    }

    /// <inheritdoc />
    public async Task UpdateFolderScanDataAsync(
        IDbConnection connection,
        bool finished,
        DateTime? startDate,
        DateTime? finishedDate)
    {
        using var transaction = connection.BeginTransaction();

        _scan.StageFolderScanFinished = finished;
        _scan.FolderScanStartDate = startDate;
        _scan.FolderScanFinishedDate = finishedDate;

        await _scanRepository.SaveScanAsync(_scan);

        transaction.Commit();

        Changed?.Invoke(this, EventArgs.Empty);
    }

    /// <inheritdoc />
    public async Task UpdateFileScanDataAsync(
        IDbConnection connection,
        bool initialized,
        bool finished,
        DateTime? startDate,
        DateTime? finishedDate)
    {
        using var transaction = connection.BeginTransaction();

        _scan.StageFileScanInitialized = initialized;
        _scan.StageFileScanFinished = finished;
        _scan.FileScanStartDate = startDate;
        _scan.FileScanFinishedDate = finishedDate;

        await _scanRepository.SaveScanAsync(_scan);

        transaction.Commit();

        Changed?.Invoke(this, EventArgs.Empty);
    }

    /// <inheritdoc />
    public async Task UpdateDuplicateFileAnalysisDataAsync(
        IDbConnection connection,
        bool finished,
        DateTime? startDate,
        DateTime? finishedDate)
    {
        using var transaction = connection.BeginTransaction();

        _scan.StageDuplicateFileAnalysisFinished = finished;
        _scan.DuplicateFileAnalysisStartDate = startDate;
        _scan.DuplicateFileAnalysisFinishedDate = finishedDate;

        await _scanRepository.SaveScanAsync(_scan);

        transaction.Commit();

        Changed?.Invoke(this, EventArgs.Empty);
    }

    /// <inheritdoc />
    public async Task UpdateOrphanedFilesScanDataAsync(
        IDbConnection connection,
        bool finished,
        DateTime? startDate,
        DateTime? finishedDate)
    {
        using var transaction = connection.BeginTransaction();

        _scan.StageOrphanedFileEnumerationFinished = finished;
        _scan.OrphanedFileEnumerationStartDate = startDate;
        _scan.OrphanedFileEnumerationFinishedDate = finishedDate;

        await _scanRepository.SaveScanAsync(_scan);

        transaction.Commit();

        Changed?.Invoke(this, EventArgs.Empty);
    }
}
