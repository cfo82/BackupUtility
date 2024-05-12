namespace BackupUtilities.Services;

using System;
using System.Data;
using System.Globalization;
using System.Security.Cryptography;
using System.Threading.Tasks;
using BackupUtilities.Data.Interfaces;
using BackupUtilities.Services.Interfaces;
using Microsoft.Extensions.Logging;

/// <summary>
/// Default-Implementation of <see cref="IFileEnumerator"/>.
/// </summary>
public class FileEnumerator : IFileEnumerator
{
    private readonly ILogger<FileEnumerator> _logger;
    private readonly IProjectManager _projectManager;
    private readonly IScanStatus _scanStatus;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileEnumerator"/> class.
    /// </summary>
    /// <param name="logger">A new logger instance to be used.</param>
    /// <param name="projectManager">The project manager.</param>
    /// <param name="longRunningOperationManager">The long running operation manager.</param>
    public FileEnumerator(
        ILogger<FileEnumerator> logger,
        IProjectManager projectManager,
        IScanStatusManager longRunningOperationManager)
    {
        _logger = logger;
        _projectManager = projectManager;
        _scanStatus = longRunningOperationManager.FullScanStatus.FileScanStatus;
    }

    /// <inheritdoc />
    public async Task EnumerateFilesAsync(bool continueLastScan)
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
        var fullPathString = System.IO.Path.Combine(fullPath.Select(f => f.Name).ToArray());

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
                System.IO.Path.Join(path, subFolder.Name),
                folderCount,
                folderCounter,
                continueLastScan);
        }

        var files = Directory.GetFiles(path);
        if (files.Length > 0)
        {
            using var transaction = connection.BeginTransaction();

            foreach (var file in files)
            {
                status = $"Working on file {file}...";
                await _scanStatus.UpdateAsync(status, _scanStatus.Progress);

                await CheckAndSaveFileAsync(connection, fileRepository, bitRotRepository, scan, folder, file, continueLastScan);
            }

            transaction.Commit();
        }
    }

    private async Task CheckAndSaveFileAsync(
        IDbConnection connection,
        IFileRepository fileRepository,
        IBitRotRepository bitRotRepository,
        IScan scan,
        Folder parentFolder,
        string path,
        bool continueLastScan)
    {
        try
        {
            FileInfo fileInfo = new FileInfo(path);
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
        using var stream = new BufferedStream(System.IO.File.OpenRead(file), 1200000);
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
