namespace BackupUtilities.Services;

using System;
using BackupUtilities.Services.Interfaces;

/// <summary>
/// Default implementation of <see cref="IScanStatusManager"/>.
/// </summary>
public class ScanStatusManager : IScanStatusManager
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ScanStatusManager"/> class.
    /// </summary>
    /// <param name="uiDispatcherService">The UI Dispatcher Service.</param>
    public ScanStatusManager(IUiDispatcherService uiDispatcherService)
    {
        FullScanStatus = new FullScanStatus(uiDispatcherService, "Full Scan");

        FullScanStatus.Changed += OnAnyScanChanged;
        FullScanStatus.FolderScanStatus.Changed += OnAnyScanChanged;
        FullScanStatus.FileScanStatus.Changed += OnAnyScanChanged;
        FullScanStatus.DuplicateFileAnalysisStatus.Changed += OnAnyScanChanged;
        FullScanStatus.OrphanedFileScanStatus.Changed += OnAnyScanChanged;
    }

    /// <inheritdoc />
    public event EventHandler<EventArgs>? Changed;

    /// <inheritdoc />
    public bool IsRunning { get; private set; }

    /// <inheritdoc />
    public IFullScanStatus FullScanStatus { get; }

    private void OnAnyScanChanged(object? sender, EventArgs e)
    {
        IsRunning = FullScanStatus.IsRunning
            || FullScanStatus.FolderScanStatus.IsRunning
            || FullScanStatus.FileScanStatus.IsRunning
            || FullScanStatus.DuplicateFileAnalysisStatus.IsRunning
            || FullScanStatus.OrphanedFileScanStatus.IsRunning;

        Changed?.Invoke(this, e);
    }
}
