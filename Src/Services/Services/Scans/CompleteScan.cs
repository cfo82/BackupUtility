namespace BackupUtilities.Services.Services.Scans;

using System;
using System.Threading.Tasks;
using BackupUtilities.Services.Interfaces;
using BackupUtilities.Services.Interfaces.Scans;
using BackupUtilities.Services.Interfaces.Status;
using Microsoft.Extensions.Logging;

/// <summary>
/// Default implementation of <see cref="ICompleteScan"/>.
/// </summary>
public class CompleteScan : ICompleteScan
{
    private readonly ILogger<CompleteScan> _logger;
    private readonly IProjectManager _projectManager;
    private readonly IScanStatusManager _scanStatusManager;
    private readonly IFolderScan _folderEnumerator;
    private readonly IFileScan _fileEnumerator;
    private readonly IDuplicateFileAnalysis _duplicateFileAnalysis;
    private readonly IOrphanedFileScan _orphanedFileEnumerator;
    private readonly IScanStatus _scanStatus;

    /// <summary>
    /// Initializes a new instance of the <see cref="CompleteScan"/> class.
    /// </summary>
    /// <param name="logger">The logger to use.</param>
    /// <param name="projectManager">The project manager.</param>
    /// <param name="scanStatusManager">The scan status manager.</param>
    /// <param name="folderEnumerator">The folder enumerator.</param>
    /// <param name="fileEnumerator">The file enumerator.</param>
    /// <param name="duplicateFileAnalysis">The duplicate file analysis.</param>
    /// <param name="orphanedFileEnumerator">The orphaned files enumerator.</param>
    public CompleteScan(
        ILogger<CompleteScan> logger,
        IProjectManager projectManager,
        IScanStatusManager scanStatusManager,
        IFolderScan folderEnumerator,
        IFileScan fileEnumerator,
        IDuplicateFileAnalysis duplicateFileAnalysis,
        IOrphanedFileScan orphanedFileEnumerator)
    {
        _logger = logger;
        _projectManager = projectManager;
        _scanStatusManager = scanStatusManager;
        _folderEnumerator = folderEnumerator;
        _fileEnumerator = fileEnumerator;
        _duplicateFileAnalysis = duplicateFileAnalysis;
        _orphanedFileEnumerator = orphanedFileEnumerator;
        _scanStatus = scanStatusManager.FullScanStatus;
    }

    /// <inheritdoc />
    public async Task RunAsync()
    {
        try
        {
            await _scanStatus.BeginAsync();

            var cs = new TaskCompletionSource();

            await Task.Factory.StartNew(
                async () =>
                {
                    try
                    {
                        var currentProject = _projectManager.CurrentProject;
                        var currentScan = currentProject?.CurrentScan;

                        if (currentProject == null || !currentProject.IsReady)
                        {
                            throw new InvalidOperationException("Project is not opened or not ready.");
                        }

                        if (currentScan == null)
                        {
                            throw new InvalidOperationException("No current scan is available.");
                        }

                        var connection = currentProject.Data.Connection;

                        await currentScan.UpdateFullScanDataAsync(connection, DateTime.Now, null);

                        await _scanStatusManager.FullScanStatus.FolderScanStatus.ResetAsync();
                        await _scanStatusManager.FullScanStatus.FileScanStatus.ResetAsync();
                        await _scanStatusManager.FullScanStatus.DuplicateFileAnalysisStatus.ResetAsync();
                        await _scanStatusManager.FullScanStatus.OrphanedFileScanStatus.ResetAsync();

                        await _scanStatus.UpdateAsync(0.0);

                        await _folderEnumerator.EnumerateFoldersAsync();

                        await _scanStatus.UpdateAsync(0.25);

                        await _fileEnumerator.EnumerateFilesAsync(false);

                        await _scanStatus.UpdateAsync(0.5);

                        await _duplicateFileAnalysis.RunDuplicateFileAnalysis();

                        await _scanStatus.UpdateAsync(0.75);

                        await _orphanedFileEnumerator.EnumerateOrphanedFilesAsync();

                        await _scanStatus.UpdateAsync(1.0);

                        await currentScan.UpdateFullScanDataAsync(connection, currentScan.Data.StartDate, DateTime.Now);
                    }
                    catch (Exception ex)
                    {
                        cs.SetException(ex);
                    }

                    cs.SetResult();
                },
                TaskCreationOptions.LongRunning);
            await cs.Task;
        }
        finally
        {
            await _scanStatus.EndAsync();
        }
    }
}
