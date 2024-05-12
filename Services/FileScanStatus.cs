namespace BackupUtilities.Services;

using System;
using BackupUtilities.Services.Interfaces;

/// <summary>
/// Default implementation of <see cref="IFileScanStatus"/>.
/// </summary>
public class FileScanStatus : ScanStatus, IFileScanStatus
{
    private string _folderEnumerationText;
    private double _folderEnumerationProgress;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileScanStatus"/> class.
    /// </summary>
    /// <param name="uiDispatcherService">The UI Dispatcher Service.</param>
    /// <param name="title">Title of this status object.</param>
    public FileScanStatus(
        IUiDispatcherService uiDispatcherService,
        string title)
        : base(uiDispatcherService, title)
    {
        _folderEnumerationText = string.Empty;
        _folderEnumerationProgress = 0;
    }

    /// <inheritdoc />
    public string FolderEnumerationText => _folderEnumerationText;

    /// <inheritdoc />
    public double FolderEnumerationProgress => _folderEnumerationProgress;

    /// <inheritdoc />
    public async Task UpdateFolderEnumerationStatusAsync(string text, double percentage)
    {
        await RunSynchronizedAsync(() =>
        {
            _folderEnumerationText = text;
            _folderEnumerationProgress = percentage;
        });

        await RaiseChangedEventAsync();
    }
}
