namespace BackupUtilities.Wpf.ViewModels.Shared;

using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using BackupUtilities.Data.Interfaces;
using BackupUtilities.Services.Interfaces;
using BackupUtilities.Wpf.Contracts;
using BackupUtilities.Wpf.Views.Shared;
using LiveChartsCore;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using Prism.Commands;
using Prism.Mvvm;
using SkiaSharp;

/// <summary>
/// Base class for the view models related with <see cref="SharedFolderDetailsView"/>.
/// </summary>
public class FolderDetailsViewModelBase : BindableBase
{
    private readonly IProjectManager _projectManager;
    private readonly ISelectedFolderService _selectedFolderService;
    private Folder? _selectedFolder;
    private ISeries[] _folderSizeSeries;
    private LegendPosition _folderSizeLegendPosition;

    /// <summary>
    /// Initializes a new instance of the <see cref="FolderDetailsViewModelBase"/> class.
    /// </summary>
    /// <param name="selectedFolderService">The service to manage the currently selected folder.</param>
    /// <param name="projectManager">Manages the current project.</param>
    public FolderDetailsViewModelBase(
        ISelectedFolderService selectedFolderService,
        IProjectManager projectManager)
    {
        _selectedFolderService = selectedFolderService;
        _projectManager = projectManager;
        _folderSizeSeries = new ISeries[0];
        _folderSizeLegendPosition = LegendPosition.Hidden;

        Path = string.Empty;
        Duplicates = new();
        CopyPathToClipboardCommand = new DelegateCommand(OnCopyPathToClipboard);
        OpenFolderInExplorerCommand = new DelegateCommand(OnOpenFolderInExplorer);
    }

    /// <summary>
    /// Gets the data for the folder size pie chart.
    /// </summary>
    public ISeries[] FolderSizeSeries => _folderSizeSeries;

    /// <summary>
    /// Gets the position where the legend for the folder size pie chart should be positioned.
    /// </summary>
    public LegendPosition FolderSizeLegendPosition => _folderSizeLegendPosition;

    /// <summary>
    /// Gets the id of the currently selected folder.
    /// </summary>
    public string FolderId => _selectedFolder?.Id.ToString() ?? string.Empty;

    /// <summary>
    /// Gets the name of the currently selected folder.
    /// </summary>
    public string Name => _selectedFolder?.Name ?? string.Empty;

    /// <summary>
    /// Gets the size of the currently selected folder.
    /// </summary>
    public string Size => _selectedFolder?.Size.ToFileSizeString() ?? string.Empty;

    /// <summary>
    /// Gets the full path of the currently selected folder..
    /// </summary>
    public string Path { get; private set; }

    /// <summary>
    /// Gets a collection of pathes to duplicates of this folder.
    /// </summary>
    public ObservableCollection<DuplicateFolderViewModel> Duplicates { get; }

    /// <summary>
    /// Gets the command to copy path of the currently selected folder to the clipboard.
    /// </summary>
    public ICommand CopyPathToClipboardCommand { get; private set; }

    /// <summary>
    /// Gets a command to open the folder in explorer.
    /// </summary>
    public ICommand OpenFolderInExplorerCommand { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this folder has been deleted in the meantime.
    /// </summary>
    public string Touched
    {
        get
        {
            if (_selectedFolder == null)
            {
                return string.Empty;
            }

            return (_selectedFolder.Touched == 1).ToString();
        }
    }

    /// <summary>
    /// Gets a value indicating the duplication level of this folder.
    /// </summary>
    public string IsDuplicate
    {
        get
        {
            if (_selectedFolder == null)
            {
                return string.Empty;
            }

            switch (_selectedFolder.IsDuplicate)
            {
            case FolderDuplicationLevel.None: return "Keine Duplikate";
            case FolderDuplicationLevel.ContainsDuplicates: return "Der Ordner enthält Duplikate";
            case FolderDuplicationLevel.EntireContentAreDuplicates: return "Der ganze Ordner-Inhalt ist ein Duplikat";
            }

            return string.Empty;
        }
    }

    /// <summary>
    /// Method to be called by implementations of this view model when the respective selected folder has changed.
    /// </summary>
    /// <param name="selectedFolder">The newly selected folder.</param>
    /// <returns>A task for async programming.</returns>
    protected async Task OnSelectedFolderChanged(Folder? selectedFolder)
    {
        if (_projectManager.CurrentProject == null || !_projectManager.CurrentProject.IsReady)
        {
            return;
        }

        Duplicates.Clear();

        _selectedFolder = selectedFolder;
        _folderSizeSeries = new ISeries[0];

        if (_selectedFolder != null)
        {
            var folderRepository = _projectManager.CurrentProject.Data.FolderRepository;

            var fullPath = await folderRepository.GetFullPathForFolderAsync(_selectedFolder);
            Path = System.IO.Path.Join(fullPath.Select(f => f.Name).ToArray());

            foreach (var duplicate in await folderRepository.EnumerateDuplicatesOfFolder(_selectedFolder, DriveType.Working))
            {
                fullPath = await folderRepository.GetFullPathForFolderAsync(duplicate);
                Duplicates.Add(new DuplicateFolderViewModel(_selectedFolderService, duplicate, System.IO.Path.Join(fullPath.Select(f => f.Name).ToArray())));
            }

            var subFolders = await folderRepository.GetSubFoldersAsync(_selectedFolder);
            var totalSize = subFolders.Sum(f => f.Size);
            _folderSizeSeries = subFolders.Select(s => CreatePieSeries(s, totalSize)).ToArray();
            _folderSizeLegendPosition = subFolders.Count() <= 10 ? LegendPosition.Right : LegendPosition.Hidden;
        }

        RaisePropertyChanged(string.Empty);
    }

    private PieSeries<double> CreatePieSeries(Folder folder, long totalSize)
    {
        var pieSeries = new PieSeries<double>()
        {
            Values = new double[] { folder.Size },
            Name = folder.Name,
            HoverPushout = 12,
        };

        if ((double)folder.Size / totalSize > 0.15)
        {
            pieSeries.DataLabelsPaint = new SolidColorPaint(SKColors.Black);
            pieSeries.DataLabelsSize = 12;
            pieSeries.DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle;
            pieSeries.DataLabelsFormatter = point => folder.Name;
        }

        pieSeries.ToolTipLabelFormatter = point => folder.Size.ToShortFileSizeString();

        return pieSeries;
    }

    private void OnCopyPathToClipboard()
    {
        System.Windows.Clipboard.SetText(Path);
    }

    private void OnOpenFolderInExplorer()
    {
        Process.Start("explorer.exe", Path);
    }
}
