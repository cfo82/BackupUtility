namespace BackupUtilities.Wpf.ViewModels.Mirror;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using BackupUtilities.Services.Interfaces;
using BackupUtilities.Wpf.Contracts;
using BackupUtilities.Wpf.Views.Mirror;
using Prism.Mvvm;

/// <summary>
/// View model for <see cref="MirrorContentView"/>.
/// </summary>
public class MirrorContentViewModel : BindableBase
{
    private readonly IErrorHandler _errorHandler;
    private readonly IProjectManager _projectManager;
    private readonly ISelectedFolderService _selectedFolderService;
    private readonly ISelectedFileService _selectedFileService;
    private MirrorFileViewModel? _selectedFile;

    /// <summary>
    /// Initializes a new instance of the <see cref="MirrorContentViewModel"/> class.
    /// </summary>
    /// <param name="errorHandler">The error handler.</param>
    /// <param name="projectManager">Manages the current project.</param>
    /// <param name="selectedFolderService">The service to manage the currently selected folder.</param>
    /// <param name="selectedFileService">The service to manage the currently selected file.</param>
    public MirrorContentViewModel(
        IErrorHandler errorHandler,
        IProjectManager projectManager,
        ISelectedFolderService selectedFolderService,
        ISelectedFileService selectedFileService)
    {
        _errorHandler = errorHandler;
        _projectManager = projectManager;
        _selectedFolderService = selectedFolderService;
        _selectedFileService = selectedFileService;
        OrphanedFiles = new ObservableCollection<MirrorFileViewModel>();

        _selectedFolderService.SelectedMirrorFolderChanged += OnSelectedMirrorFolderChanged;
        OnSelectedMirrorFolderChanged(null, EventArgs.Empty);
    }

    /// <summary>
    /// Gets the top level items of the folder tree.
    /// </summary>
    public ObservableCollection<MirrorFileViewModel> OrphanedFiles { get; }

    /// <summary>
    /// Gets or sets the currently selected file.
    /// </summary>
    public MirrorFileViewModel? SelectedFile
    {
        get
        {
            return _selectedFile;
        }

        set
        {
            if (SetProperty(ref _selectedFile, value))
            {
                _selectedFileService.SelectedMirrorFile = _selectedFile?.File;
            }
        }
    }

    private async void OnSelectedMirrorFolderChanged(object? sender, EventArgs e)
    {
        try
        {
            OrphanedFiles.Clear();

            var selectedFolder = _selectedFolderService.SelectedMirrorFolder;
            if (selectedFolder != null)
            {
                var currentProject = _projectManager.CurrentProject;
                if (currentProject == null)
                {
                    throw new InvalidOperationException("Project should be opened at this stage.");
                }

                var connection = currentProject.Data.Connection;
                var fileRepository = currentProject.Data.FileRepository;
                var orphanedFilesRepository = currentProject.Data.OrphanedFileRepository;

                var orphanedFiles = await orphanedFilesRepository.EnumerateOrphanedFilesByFolderAsync(connection, selectedFolder, true);

                OrphanedFiles.AddRange(orphanedFiles.Select(f => new MirrorFileViewModel(f, f.DuplicatesOnLifeDrive)));

                _selectedFile = OrphanedFiles.FirstOrDefault(f => f.File.ParentId == _selectedFileService.SelectedMirrorFile?.ParentId && string.Equals(f.File.Name, _selectedFileService.SelectedMirrorFile?.Name));
                RaisePropertyChanged(nameof(SelectedFile));
            }
        }
        catch (Exception ex)
        {
            _errorHandler.Error = ex;
        }
    }
}
