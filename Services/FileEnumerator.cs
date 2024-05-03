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
    private readonly ILongRunningOperationManager _longRunningOperationManager;
    private readonly IErrorHandler _errorHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileEnumerator"/> class.
    /// </summary>
    /// <param name="logger">A new logger instance to be used.</param>
    /// <param name="projectManager">The project manager.</param>
    /// <param name="longRunningOperationManager">The long running operation manager.</param>
    /// <param name="errorHandler">The error handler to use.</param>
    public FileEnumerator(
        ILogger<FileEnumerator> logger,
        IProjectManager projectManager,
        ILongRunningOperationManager longRunningOperationManager,
        IErrorHandler errorHandler)
    {
        _logger = logger;
        _projectManager = projectManager;
        _longRunningOperationManager = longRunningOperationManager;
        _errorHandler = errorHandler;
    }

    /// <inheritdoc />
    public async Task EnumerateFilesAsync(bool continueLastScan)
    {
        try
        {
            await _longRunningOperationManager.BeginOperationAsync("File Enumeration");

            var cs = new TaskCompletionSource();

            await Task.Factory.StartNew(
                async () =>
                {
                    try
                    {
                        if (_projectManager.CurrentProject == null || !_projectManager.CurrentProject.IsReady)
                        {
                            throw new InvalidOperationException("Project is not opened or not ready.");
                        }

                        var connection = _projectManager.CurrentProject.Data.Connection;
                        var settingsRepository = _projectManager.CurrentProject.Data.SettingsRepository;
                        var folderRepository = _projectManager.CurrentProject.Data.FolderRepository;
                        var fileRepository = _projectManager.CurrentProject.Data.FileRepository;
                        var bitRotRepository = _projectManager.CurrentProject.Data.BitRotRepository;

                        var settings = await settingsRepository.GetSettingsAsync(connection);

                        if (!continueLastScan)
                        {
                            await fileRepository.MarkAllFilesAsUntouchedAsync(connection);
                            await bitRotRepository.ClearAsync(connection);
                        }

                        await EnumerateFilesRecursiveAsync(connection, folderRepository, fileRepository, bitRotRepository, null, settings.RootPath, settings, continueLastScan);
                        await fileRepository.FindDuplicateFilesAsync(connection);
                    }
                    catch (Exception ex)
                    {
                        _errorHandler.Error = ex;
                    }

                    cs.SetResult();
                },
                TaskCreationOptions.LongRunning);
            await cs.Task;
        }
        finally
        {
            await _longRunningOperationManager.EndOperationAsync();
        }
    }

    private static (string IntroHash, string FullHash) ComputeChecksum(string file)
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

    private async Task EnumerateFilesRecursiveAsync(
        IDbConnection connection,
        IFolderRepository folderRepository,
        IFileRepository fileRepository,
        IBitRotRepository bitRotRepository,
        Folder? parentFolder,
        string path,
        Settings settings,
        bool continueLastScan)
    {
        if (settings.IgnoredFolders.Any(d => string.Equals(d.Path, path)))
        {
            return;
        }

        var status = $"Enumerating Files For Directory '{path}'...";
        _logger.LogInformation(status);
        await _longRunningOperationManager.UpdateOperationAsync(status, null);

        Folder currentFolder = await FindFolderAsync(connection, folderRepository, parentFolder, path);

        foreach (var subDirectory in Directory.GetDirectories(path))
        {
            await EnumerateFilesRecursiveAsync(connection, folderRepository, fileRepository, bitRotRepository, currentFolder, subDirectory, settings, continueLastScan);
        }

        var files = Directory.GetFiles(path);
        if (files.Length > 0)
        {
            using var transaction = connection.BeginTransaction();

            foreach (var file in files)
            {
                status = $"Working on file {file}...";
                await _longRunningOperationManager.UpdateOperationAsync(status, null);

                await CheckAndSaveFileAsync(connection, fileRepository, bitRotRepository, currentFolder, file, continueLastScan);
            }

            transaction.Commit();
        }
    }

    private async Task<Folder> FindFolderAsync(
        IDbConnection connection,
        IFolderRepository folderRepository,
        Folder? parentFolder,
        string path)
    {
        Folder? folder;
        if (parentFolder != null)
        {
            var folderName = Path.GetFileName(path);
            folder = await folderRepository.GetFolderAsync(connection, parentFolder.Id, folderName);
        }
        else
        {
            folder = await folderRepository.GetFolderAsync(connection, path);
        }

        if (folder == null || folder.Touched == 0)
        {
            throw new InvalidOperationException($"The parent folder '{path}' can't be found or is not touched.");
        }

        return folder;
    }

    private async Task CheckAndSaveFileAsync(
        IDbConnection connection,
        IFileRepository fileRepository,
        IBitRotRepository bitRotRepository,
        Folder parentFolder,
        string path,
        bool continueLastScan)
    {
        try
        {
            FileInfo fileInfo = new FileInfo(path);
            var lastWriteTime = fileInfo.LastWriteTimeUtc;

            string name = Path.GetFileName(path);
            var file = await fileRepository.FindFileByNameAsync(connection, parentFolder, name);
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
                await fileRepository.SaveFileAsync(connection, file);
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
                await fileRepository.SaveFileAsync(connection, file);
            }

            // file was not written to and hashes dont match: bitrot
            else if (!string.Equals(file.Hash, checksum.FullHash))
            {
                _logger.LogError($"*** Error: Bitrot detected on file '{path}'.");
                await bitRotRepository.CreateBitRotAsync(connection, file);
            }

            await fileRepository.TouchFileAsync(connection, file);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while checking file.");
        }
    }
}
