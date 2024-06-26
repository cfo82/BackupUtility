namespace BackupUtilities.Services.Services.Scans;

using System;
using System.Data;
using System.Globalization;
using System.Security.Cryptography;
using System.Threading.Tasks;
using BackupUtilities.Data.Interfaces;
using BackupUtilities.Services.Interfaces;
using BackupUtilities.Services.Interfaces.Scans;
using BackupUtilities.Services.Interfaces.Status;
using BackupUtilities.Services.Services;
using Microsoft.Extensions.Logging;

/// <summary>
/// Default-Implementation of <see cref="IFileScan"/>.
/// </summary>
public class FileScan : ScanOperationBase, IFileScan
{
    private readonly ILogger<FileScan> _logger;
    private readonly IFileSystemService _fileSystemService;
    private readonly IFileScanStatus _scanStatus;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileScan"/> class.
    /// </summary>
    /// <param name="logger">A new logger instance to be used.</param>
    /// <param name="fileSystemService">The file system access.</param>
    /// <param name="projectManager">The project manager.</param>
    /// <param name="longRunningOperationManager">The long running operation manager.</param>
    public FileScan(
        ILogger<FileScan> logger,
        IFileSystemService fileSystemService,
        IProjectManager projectManager,
        IScanStatusManager longRunningOperationManager)
        : base(projectManager)
    {
        _logger = logger;
        _fileSystemService = fileSystemService;
        _scanStatus = longRunningOperationManager.FullScanStatus.FileScanStatus;
    }

    /// <inheritdoc />
    public async Task EnumerateFilesAsync(bool continueLastScan)
    {
        await SpawnAndFinishLongRunningTaskAsync(_scanStatus, async (currentProject, currentScan) =>
        {
            var connection = currentProject.Data.Connection;
            var settingsRepository = currentProject.Data.SettingsRepository;
            var folderRepository = currentProject.Data.FolderRepository;
            var fileRepository = currentProject.Data.FileRepository;
            var bitRotRepository = currentProject.Data.BitRotRepository;

            await currentScan.UpdateFileScanDataAsync(
                connection,
                continueLastScan ? currentScan.Data.StageFileScanInitialized : false,
                false,
                DateTime.Now,
                null);

            var settings = await settingsRepository.GetSettingsAsync(null);

            if (!continueLastScan)
            {
                using var transaction = connection.BeginTransaction();

                await _scanStatus.UpdateAsync("Mark all files in DB as untouched...", 0.0);
                await fileRepository.MarkAllFilesAsUntouchedAsync();

                await _scanStatus.UpdateAsync("Clear all existing bitrot from database...", 0.0);
                await bitRotRepository.DeleteAllAsync(currentScan.Data);

                transaction.Commit();

                await currentScan.UpdateFileScanDataAsync(connection, true, false, currentScan.Data.FileScanStartDate, null);
            }

            await ProcessTreeAsync(
                connection,
                folderRepository,
                fileRepository,
                bitRotRepository,
                settings,
                currentScan,
                continueLastScan);

            await currentScan.UpdateFileScanDataAsync(connection, true, true, currentScan.Data.FolderScanStartDate, DateTime.Now);
        });
    }

    private async Task ProcessTreeAsync(
        IDbConnection connection,
        IFolderRepository folderRepository,
        IFileRepository fileRepository,
        IBitRotRepository bitRotRepository,
        Settings settings,
        IScan scan,
        bool continueLastScan)
    {
        var rootFolder = await folderRepository.GetFolderAsync(settings.RootPath);
        if (rootFolder == null)
        {
            return;
        }

        var folderCount = await folderRepository.GetFolderCount(DriveType.Working);
        var fullPath = await folderRepository.GetFullPathForFolderAsync(rootFolder);
        var fullPathString = Path.Combine(fullPath.Select(f => f.Name).ToArray());

        await ProcessTreeRecursiveAsync(
            connection,
            folderRepository,
            fileRepository,
            bitRotRepository,
            scan,
            rootFolder,
            fullPathString,
            folderCount,
            new FolderCounter { Index = fullPath.Count() - 1 },
            continueLastScan);
    }

    private async Task ProcessTreeRecursiveAsync(
        IDbConnection connection,
        IFolderRepository folderRepository,
        IFileRepository fileRepository,
        IBitRotRepository bitRotRepository,
        IScan scan,
        Folder folder,
        string path,
        long folderCount,
        FolderCounter folderCounter,
        bool continueLastScan)
    {
        double percentage = folderCounter.Index / (double)(folderCount - 1);
        ++folderCounter.Index;
        var status = $"{folderCounter.Index} / {folderCount}: Enumerating Files For Directory '{path}'...";
        await _scanStatus.UpdateAsync(status, percentage);
        _logger.LogInformation(status);

        var subFolders = await folderRepository.GetSubFoldersAsync(folder);
        foreach (var subFolder in subFolders)
        {
            await ProcessTreeRecursiveAsync(
                connection,
                folderRepository,
                fileRepository,
                bitRotRepository,
                scan,
                subFolder,
                Path.Join(path, subFolder.Name),
                folderCount,
                folderCounter,
                continueLastScan);
        }

        var files = _fileSystemService.GetFiles(path).ToArray();
        if (files.Length > 0)
        {
            using var transaction = connection.BeginTransaction();

            for (int i = 0; i < files.Length; ++i)
            {
                double progress = (double)i / (files.Length - 1);
                await _scanStatus.UpdateFolderEnumerationStatusAsync(Path.GetFileName(files[i]), progress);

                await CheckAndSaveFileAsync(fileRepository, bitRotRepository, scan, folder, files[i], continueLastScan);
            }

            await _scanStatus.UpdateFolderEnumerationStatusAsync(string.Empty, 1.0);

            transaction.Commit();
        }
    }

    private async Task CheckAndSaveFileAsync(
        IFileRepository fileRepository,
        IBitRotRepository bitRotRepository,
        IScan scan,
        Folder parentFolder,
        string path,
        bool continueLastScan)
    {
        try
        {
            var fileInfo = new FileInfo(path);
            var lastWriteTime = fileInfo.LastWriteTimeUtc;

            string name = Path.GetFileName(path);
            var file = await fileRepository.FindFileByNameAsync(parentFolder, name);
            if (continueLastScan && file != null && file.Touched == 1)
            {
                return;
            }

            var checksum = ComputeChecksum(path);

            if (file == null)
            {
                file = new File
                {
                    ParentId = parentFolder.Id,
                    Name = name,
                    IntroHash = checksum.IntroHash,
                    Hash = checksum.FullHash,
                    LastWriteTime = lastWriteTime.ToString("dd/MM/yyyy HH:mm:ss:fffffff", CultureInfo.InvariantCulture),
                    Size = fileInfo.Length,
                    Touched = 1,
                };
                await fileRepository.SaveFileAsync(file);
            }

            var parsedLastWriteTime = DateTime.SpecifyKind(
                DateTime.ParseExact(new string(file.LastWriteTime), "dd/MM/yyyy HH:mm:ss:fffffff", CultureInfo.InvariantCulture),
                DateTimeKind.Utc);

            // file was written to: update hash in db
            if (!DateTime.Equals(lastWriteTime, parsedLastWriteTime))
            {
                file.IntroHash = checksum.IntroHash;
                file.Hash = checksum.FullHash;
                file.LastWriteTime = lastWriteTime.ToString("dd/MM/yyyy HH:mm:ss:fffffff", CultureInfo.InvariantCulture);
                file.Size = fileInfo.Length;
                file.Touched = 1;
                await fileRepository.SaveFileAsync(file);
            }

            // file was not written to and hashes dont match: bitrot
            else if (!string.Equals(file.Hash, checksum.FullHash))
            {
                _logger.LogError($"*** Error: Bitrot detected on file '{path}'.");
                await bitRotRepository.CreateBitRotAsync(scan.Data, file);
            }

            await fileRepository.TouchFileAsync(file);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while checking file.");
        }
    }

    private (string IntroHash, string FullHash) ComputeChecksum(string file)
    {
        using var stream = new BufferedStream(_fileSystemService.OpenFileToRead(file), 1200000);
        byte[] introBuffer = new byte[100 * 1024];
        int lengthRead = stream.Read(introBuffer, 0, introBuffer.Length);
        string introHash = string.Empty;
        if (lengthRead > 0)
        {
            using var introSha = SHA512.Create();
            byte[] introChecksum = introSha.ComputeHash(introBuffer, 0, lengthRead);
            introHash = BitConverter.ToString(introChecksum).Replace("-", string.Empty).ToLower();
        }

        stream.Seek(0, SeekOrigin.Begin);
        using var sha = SHA512.Create();
        byte[] checksum = sha.ComputeHash(stream);
        var fullHash = BitConverter.ToString(checksum).Replace("-", string.Empty).ToLower();

        return (introHash, fullHash);
    }

    private class FolderCounter
    {
        public long Index { get; set; }
    }
}
