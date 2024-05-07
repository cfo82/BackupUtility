using BackupUtilities.Console;
using BackupUtilities.Data.Interfaces;
using BackupUtilities.Services;
using Cocona;
using Microsoft.Extensions.Logging;

var builder = CoconaApp.CreateBuilder();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
#if DEBUG
builder.Logging.AddDebug();
#endif

var app = builder.Build();

const string InitializeCommandName = "Initialize";
const string ScanFoldersCommandName = "ScanFolders";
const string ScanFilesCommandName = "ScanFiles";
const string RunDuplicateFilesAnalysisCommandName = "DuplicateFilesAnalysis";
const string FindOrphanedFilesCommandName = "FindOrphanedFiles";
const string RunAllCommandName = "RunAll";

app.AddCommand(
    InitializeCommandName,
    async (
        ILoggerFactory loggerFactory,
        [Argument(Description = "Path to the hash database.")] string databasePath,
        [Argument(Description = "The path of the location to check.")] string rootPath,
        [Argument(Description = "Mirror with backup files.")] string mirrorPath,
        [Option('i', Description = "Ignore Directory.")] string[]? ignore,
        [Option('l', Description = "Logfile.")] string? logFile) =>
    {
        loggerFactory.WriteToLogFile(logFile);
        var logger = loggerFactory.CreateLogger(InitializeCommandName);

        logger.LogInformation("Run {Command}:", InitializeCommandName);
        Console.WriteLine($"Database: {databasePath}");
        Console.WriteLine($"Root:     {rootPath}");
        Console.WriteLine($"Mirror:   {mirrorPath}");
        Console.WriteLine($"Ignore:   {string.Join(", ", ignore ?? Array.Empty<string>())}");
        Console.WriteLine();

        var projectManager = new ProjectManager();
        var project = await projectManager.OpenProjectAsync(databasePath);
        if (project == null)
        {
            throw new InvalidOperationException($"Unable to open project {databasePath}");
        }

        project.Settings.RootPath = rootPath;
        project.Settings.MirrorPath = mirrorPath;
        project.Settings.IgnoredFolders.Clear();
        if (ignore != null)
        {
            project.Settings.IgnoredFolders.AddRange(ignore.Select(i => new IgnoredFolder { Path = i }));
        }

        logger.LogInformation("Write settings...");
        await project.SaveSettingsAsync(project.Settings);
    });

app.AddCommand(
    ScanFoldersCommandName,
    async (
        ILoggerFactory loggerFactory,
        [Argument(Description = "Path to the hash database.")] string databasePath,
        [Option('l', Description = "Logfile.")] string? logFile) =>
    {
        loggerFactory.WriteToLogFile(logFile);
        var logger = loggerFactory.CreateLogger(ScanFoldersCommandName);

        logger.LogInformation("Run {Command}:", ScanFoldersCommandName);
        Console.WriteLine($"Database: {databasePath}");
        Console.WriteLine();

        var projectManager = new ProjectManager();
        var project = await projectManager.OpenProjectAsync(databasePath);
        var dispatcherService = new ConsoleDispatcherService();
        var longRunningOperationManager = new LongRunningOperationManager(dispatcherService);
        var errorHandler = new ErrorHandler(loggerFactory.CreateLogger<ErrorHandler>());

        logger.LogInformation("Enumerate Folders...");
        var enumerateFolders = new FolderEnumerator(
            loggerFactory.CreateLogger<FolderEnumerator>(),
            projectManager,
            longRunningOperationManager,
            errorHandler);

        await enumerateFolders.EnumerateFoldersAsync();
    });

app.AddCommand(
    ScanFilesCommandName,
    async (
        ILoggerFactory loggerFactory,
        [Argument(Description = "Path to the hash database.")] string databasePath,
        [Option('l', Description = "Logfile.")] string? logFile) =>
    {
        loggerFactory.WriteToLogFile(logFile);
        var logger = loggerFactory.CreateLogger(ScanFilesCommandName);

        logger.LogInformation("Run {Command}:", ScanFilesCommandName);
        Console.WriteLine($"Database: {databasePath}");
        Console.WriteLine();

        var projectManager = new ProjectManager();
        var project = await projectManager.OpenProjectAsync(databasePath);
        var dispatcherService = new ConsoleDispatcherService();
        var longRunningOperationManager = new LongRunningOperationManager(dispatcherService);
        var errorHandler = new ErrorHandler(loggerFactory.CreateLogger<ErrorHandler>());

        logger.LogInformation("Enumerate Folders...");
        var enumerateFiles = new FileEnumerator(
            loggerFactory.CreateLogger<FileEnumerator>(),
            projectManager,
            longRunningOperationManager,
            errorHandler);

        await enumerateFiles.EnumerateFilesAsync(false);
    });

app.AddCommand(
    RunDuplicateFilesAnalysisCommandName,
    async (
        ILoggerFactory loggerFactory,
        [Argument(Description = "Path to the hash database.")] string databasePath,
        [Option('l', Description = "Logfile.")] string? logFile) =>
    {
        loggerFactory.WriteToLogFile(logFile);
        var logger = loggerFactory.CreateLogger(RunDuplicateFilesAnalysisCommandName);

        logger.LogInformation("Run {Command}:", RunDuplicateFilesAnalysisCommandName);
        Console.WriteLine($"Database: {databasePath}");
        Console.WriteLine();

        var projectManager = new ProjectManager();
        var project = await projectManager.OpenProjectAsync(databasePath);
        var dispatcherService = new ConsoleDispatcherService();
        var longRunningOperationManager = new LongRunningOperationManager(dispatcherService);
        var errorHandler = new ErrorHandler(loggerFactory.CreateLogger<ErrorHandler>());

        logger.LogInformation("Find duplicate files and folders...");
        var duplicateFileAnalysis = new DuplicateFileAnalysis(
            loggerFactory.CreateLogger<DuplicateFileAnalysis>(),
            projectManager,
            longRunningOperationManager,
            errorHandler);

        await duplicateFileAnalysis.RunDuplicateFileAnalysis();
    });

app.AddCommand(
    FindOrphanedFilesCommandName,
    async (
        ILoggerFactory loggerFactory,
        [Argument(Description = "Path to the hash database.")] string databasePath,
        [Option('l', Description = "Logfile.")] string? logFile) =>
    {
        loggerFactory.WriteToLogFile(logFile);
        var logger = loggerFactory.CreateLogger(FindOrphanedFilesCommandName);

        logger.LogInformation("Run {Command}:", FindOrphanedFilesCommandName);
        Console.WriteLine($"Database: {databasePath}");
        Console.WriteLine();

        var projectManager = new ProjectManager();
        var project = await projectManager.OpenProjectAsync(databasePath);
        var dispatcherService = new ConsoleDispatcherService();
        var longRunningOperationManager = new LongRunningOperationManager(dispatcherService);
        var errorHandler = new ErrorHandler(loggerFactory.CreateLogger<ErrorHandler>());

        logger.LogInformation("Find orphaned files...");
        var orphanedFileEnumerator = new OrphanedFileEnumerator(
            loggerFactory.CreateLogger<OrphanedFileEnumerator>(),
            projectManager,
            longRunningOperationManager,
            errorHandler);

        await orphanedFileEnumerator.EnumerateOrphanedFilesAsync();
    });

app.AddCommand(
    RunAllCommandName,
    async (
        ILoggerFactory loggerFactory,
        [Argument(Description = "Path to the hash database.")] string databasePath,
        [Argument(Description = "The path of the location to check.")] string rootPath,
        [Argument(Description = "Mirror with backup files.")] string mirrorPath,
        [Option('i', Description = "Ignore Directory.")] string[]? ignore,
        [Option('l', Description = "Logfile.")] string? logFile) =>
    {
        loggerFactory.WriteToLogFile(logFile);
        var logger = loggerFactory.CreateLogger(RunAllCommandName);

        logger.LogInformation("Run {Command}:", RunAllCommandName);
        Console.WriteLine($"Database: {databasePath}");
        Console.WriteLine($"Root:     {rootPath}");
        Console.WriteLine($"Mirror:   {mirrorPath}");
        Console.WriteLine($"Ignore:   {string.Join(", ", ignore ?? Array.Empty<string>())}");
        Console.WriteLine();

        var projectManager = new ProjectManager();
        var project = await projectManager.OpenProjectAsync(databasePath);
        var dispatcherService = new ConsoleDispatcherService();
        var longRunningOperationManager = new LongRunningOperationManager(dispatcherService);
        var errorHandler = new ErrorHandler(loggerFactory.CreateLogger<ErrorHandler>());
        if (project == null)
        {
            throw new InvalidOperationException($"Unable to open project {databasePath}");
        }

        project.Settings.RootPath = rootPath;
        project.Settings.MirrorPath = mirrorPath;
        project.Settings.IgnoredFolders.Clear();
        if (ignore != null)
        {
            project.Settings.IgnoredFolders.AddRange(ignore.Select(i => new IgnoredFolder { Path = i }));
        }

        logger.LogInformation("Write settings...");
        await project.SaveSettingsAsync(project.Settings);

        logger.LogInformation("Enumerate Folders...");
        var enumerateFolders = new FolderEnumerator(
            loggerFactory.CreateLogger<FolderEnumerator>(),
            projectManager,
            longRunningOperationManager,
            errorHandler);

        await enumerateFolders.EnumerateFoldersAsync();

        logger.LogInformation("Enumerate Folders...");
        var enumerateFiles = new FileEnumerator(
            loggerFactory.CreateLogger<FileEnumerator>(),
            projectManager,
            longRunningOperationManager,
            errorHandler);

        await enumerateFiles.EnumerateFilesAsync(false);

        logger.LogInformation("Find duplicate files and folders...");
        var duplicateFileAnalysis = new DuplicateFileAnalysis(
            loggerFactory.CreateLogger<DuplicateFileAnalysis>(),
            projectManager,
            longRunningOperationManager,
            errorHandler);

        await duplicateFileAnalysis.RunDuplicateFileAnalysis();

        logger.LogInformation("Find orphaned files...");
        var orphanedFileEnumerator = new OrphanedFileEnumerator(
            loggerFactory.CreateLogger<OrphanedFileEnumerator>(),
            projectManager,
            longRunningOperationManager,
            errorHandler);

        await orphanedFileEnumerator.EnumerateOrphanedFilesAsync();
    });

app.Run();
