namespace BackupUtilities.Wpf.ViewModels.Working;

using System;
using System.Diagnostics;
using BackupUtilities.Data.Interfaces;
using BackupUtilities.Services.Interfaces;
using BackupUtilities.Wpf.Contracts;
using BackupUtilities.Wpf.ViewModels.Shared;
using BackupUtilities.Wpf.Views.Working;

/// <summary>
/// The view model for <see cref="FileDetailsView"/>.
/// </summary>
public class FileDetailsViewModel : FileDetailsViewModelBase
{
    private readonly ISelectedFileService _selectedFileService;
    private File? _selectedFile;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileDetailsViewModel"/> class.
    /// </summary>
    /// <param name="selectedFileService">The service to manage the currently selected file.</param>
    /// <param name="projectManager">Manages the current project.</param>
    public FileDetailsViewModel(
        ISelectedFileService selectedFileService,
        IProjectManager projectManager)
        : base(projectManager)
    {
        _selectedFileService = selectedFileService;

        _selectedFileService.SelectedFileChanged += OnSelectedFileChanged;
    }

    /// <summary>
    /// Gets a value indicating whether this folder has been deleted in the meantime.
    /// </summary>
    public string Touched
    {
        get
        {
            if (_selectedFile == null)
            {
                return string.Empty;
            }

            return (_selectedFile.Touched == 1).ToString();
        }
    }

    /// <summary>
    /// Gets a value indicating whether to display the 'touched' fields on the UI. In this case yes.
    /// </summary>
    public bool HasTouchedAttribute => true;

    /// <summary>
    /// Gets a value indicating the duplication level of this folder.
    /// </summary>
    public string IsDuplicate => (_selectedFile?.IsDuplicate == 1).ToString();

    private async void OnSelectedFileChanged(object? sender, EventArgs e)
    {
        try
        {
            _selectedFile = _selectedFileService.SelectedFile;

            await OnSelectedFileChanged(_selectedFileService.SelectedFile);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
        }
    }
}
