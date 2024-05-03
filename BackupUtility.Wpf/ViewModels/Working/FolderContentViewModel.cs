namespace BackupUtilities.Wpf.ViewModels.Working;

using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using BackupUtilities.Services.Interfaces;
using BackupUtilities.Wpf.Contracts;
using BackupUtilities.Wpf.Views;
using Prism.Mvvm;

/// <summary>
/// The view model for <see cref="FolderContentView"/>.
/// </summary>
public class FolderContentViewModel : BindableBase
{
    private readonly ISelectedFolderService _selectedFolderService;
    private readonly ISelectedFileService _selectedFileService;
    private readonly IProjectManager _projectManager;
    private FileViewModel? _selectedFile;

    /// <summary>
    /// Initializes a new instance of the <see cref="FolderContentViewModel"/> class.
    /// </summary>
    /// <param name="selectedFolderService">The service to manage the currently selected folder.</param>
    /// <param name="selectedFileService">The service to manage the currently selected file.</param>
    /// <param name="projectManager">Manages the current project.</param>
    public FolderContentViewModel(
        ISelectedFolderService selectedFolderService,
        ISelectedFileService selectedFileService,
        IProjectManager projectManager)
    {
        _selectedFolderService = selectedFolderService;
        _selectedFileService = selectedFileService;
        _projectManager = projectManager;
        Files = new ObservableCollection<FileViewModel>();

        _selectedFolderService.SelectedFolderChanged += OnSelectedFolderChanged;
    }

    /// <summary>
    /// Gets a list of all files within the folder.
    /// </summary>
    public ObservableCollection<FileViewModel> Files { get; }

    /// <summary>
    /// Gets or sets the currently selected file.
    /// </summary>
    public FileViewModel? SelectedFile
    {
        get
        {
            return _selectedFile;
        }

        set
        {
            if (SetProperty(ref _selectedFile, value))
            {
                _selectedFileService.SelectedFile = _selectedFile?.File;
            }
        }
    }

    private async void OnSelectedFolderChanged(object? sender, EventArgs e)
    {
        try
        {
            if (_projectManager.CurrentProject == null || !_projectManager.CurrentProject.IsReady)
            {
                return;
            }

            var connection = _projectManager.CurrentProject.Data.Connection;
            var fileRepository = _projectManager.CurrentProject.Data.FileRepository;

            Files.Clear();

            var selectedFolder = _selectedFolderService.SelectedFolder;
            if (selectedFolder != null)
            {
                var files = await fileRepository.EnumerateFilesByFolderAsync(connection, selectedFolder);
                Files.AddRange(files.Select(f => new FileViewModel(f)));
            }

            _selectedFile = Files.FirstOrDefault(f => f.File.ParentId == _selectedFileService.SelectedFile?.ParentId && string.Equals(f.File.Name, _selectedFileService.SelectedFile?.Name));
            RaisePropertyChanged(nameof(SelectedFile));
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
        }
    }
}
