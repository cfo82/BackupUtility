namespace BackupUtilities.Wpf;

using System;
using System.IO;
using System.Reflection;
using System.Windows;
using BackupUtilities.Services;
using BackupUtilities.Services.Interfaces;
using BackupUtilities.Wpf.Contracts;
using BackupUtilities.Wpf.Services;
using BackupUtilities.Wpf.ViewModels;
using BackupUtilities.Wpf.ViewModels.Mirror;
using BackupUtilities.Wpf.ViewModels.Scans;
using BackupUtilities.Wpf.ViewModels.Settings;
using BackupUtilities.Wpf.ViewModels.Working;
using BackupUtilities.Wpf.Views;
using BackupUtilities.Wpf.Views.Mirror;
using BackupUtilities.Wpf.Views.Scans;
using BackupUtilities.Wpf.Views.Settings;
using BackupUtilities.Wpf.Views.Working;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Prism.DryIoc;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using Serilog;

/// <summary>
/// Interaction logic for App.xaml.
/// </summary>
public partial class App : PrismApplication
{
    /// <inheritdoc />
    protected override void RegisterTypes(IContainerRegistry containerRegistry)
    {
        var assemblyDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ??
                        throw new InvalidOperationException("Unable to find application directory.");

        var configurationBuilder = new ConfigurationBuilder()
            .SetBasePath(assemblyDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

        var configuration = configurationBuilder.Build();

        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var logFolder = Path.Combine(localAppData, "BackupUtilities", "log");
        Directory.CreateDirectory(logFolder);

        var logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .WriteTo.File(Path.Combine(logFolder, "log.txt"))
            .CreateLogger();

        var loggerFactory = new LoggerFactory()
            .AddSerilog(logger);

        // Register Services
        containerRegistry.RegisterInstance<IConfiguration>(configuration);
        containerRegistry.RegisterInstance<ILoggerFactory>(loggerFactory);
        containerRegistry.RegisterSingleton<ISelectedFolderService, SelectedFolderService>();
        containerRegistry.RegisterSingleton<ISelectedFileService, SelectedFileService>();
        containerRegistry.RegisterSingleton<IProjectManager, ProjectManager>();
        containerRegistry.RegisterSingleton<IUiDispatcherService, UiDispatcherService>();
        containerRegistry.RegisterSingleton<IScanStatusManager, ScanStatusManager>();
        containerRegistry.RegisterSingleton<IErrorHandler, ErrorHandler>();
        containerRegistry.Register<IFolderEnumerator, FolderEnumerator>();
        containerRegistry.Register<IFileEnumerator, FileEnumerator>();
        containerRegistry.Register<IDuplicateFileAnalysis, DuplicateFileAnalysis>();
        containerRegistry.Register<IOrphanedFileEnumerator, OrphanedFileEnumerator>();
        containerRegistry.Register<ICompleteScan, CompleteScan>();
        containerRegistry.Register(typeof(ILogger<>), typeof(Logger<>));

        // Views - Generic

        // Views - Region Navigation
        containerRegistry.RegisterForNavigation<MainWindow, MainWindowViewModel>();
        containerRegistry.RegisterForNavigation<ToolBarView, ToolBarViewModel>();
        containerRegistry.RegisterForNavigation<SettingsView, SettingsViewModel>();

        containerRegistry.RegisterForNavigation<WorkingView, WorkingViewModel>();
        containerRegistry.RegisterForNavigation<FolderTreeView, FolderTreeViewModel>();
        containerRegistry.RegisterForNavigation<FolderContentView, FolderContentViewModel>();
        containerRegistry.RegisterForNavigation<FolderDetailsView, FolderDetailsViewModel>();
        containerRegistry.RegisterForNavigation<FileDetailsView, FileDetailsViewModel>();

        containerRegistry.RegisterForNavigation<MirrorView, MirrorViewModel>();
        containerRegistry.RegisterForNavigation<MirrorTreeView, MirrorTreeViewModel>();
        containerRegistry.RegisterForNavigation<MirrorContentView, MirrorContentViewModel>();
    }

    /// <inheritdoc />
    protected override void ConfigureRegionAdapterMappings(RegionAdapterMappings regionAdapterMappings)
    {
        base.ConfigureRegionAdapterMappings(regionAdapterMappings);
    }

    /// <inheritdoc />
    protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
    {
        base.ConfigureModuleCatalog(moduleCatalog);
    }

    /// <inheritdoc />
    protected override Window CreateShell()
    {
        return Container.Resolve<MainWindow>();
    }

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        var regionManager = Container.Resolve<IRegionManager>();
        regionManager.RegisterViewWithRegion("Region_MainWindow_ToolBar", typeof(ToolBarView));
        regionManager.RegisterViewWithRegion("Region_MainWindow_Settings", typeof(SettingsView));
        regionManager.RegisterViewWithRegion("Region_MainWindow_Scans", typeof(ScanView));
        regionManager.RegisterViewWithRegion("Region_MainWindow_Working", typeof(WorkingView));
        regionManager.RegisterViewWithRegion("Region_MainWindow_Mirror", typeof(MirrorView));

        regionManager.RegisterViewWithRegion("Region_Working_FolderTree", typeof(FolderTreeView));
        regionManager.RegisterViewWithRegion("Region_Working_FolderContent", typeof(FolderContentView));
        regionManager.RegisterViewWithRegion("Region_Working_FolderDetails", typeof(FolderDetailsView));
        regionManager.RegisterViewWithRegion("Region_Working_FileDetails", typeof(FileDetailsView));

        regionManager.RegisterViewWithRegion("Region_Mirror_Tree", typeof(MirrorTreeView));
        regionManager.RegisterViewWithRegion("Region_Mirror_Content", typeof(MirrorContentView));
        regionManager.RegisterViewWithRegion("Region_Mirror_FolderDetails", typeof(MirrorFolderDetailsView));
        regionManager.RegisterViewWithRegion("Region_Mirror_FileDetails", typeof(MirrorFileDetailsView));

        regionManager.RegisterViewWithRegion("Region_Scan_Settings", typeof(ScanSettingsView));
        regionManager.RegisterViewWithRegion("Region_Scan_Status", typeof(SimpleScanView));

        base.OnInitialized();
    }
}
