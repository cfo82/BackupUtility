namespace BackupUtilities.Wpf.ViewModels.Mirror;

using System;
using System.Diagnostics;
using BackupUtilities.Services.Interfaces;
using BackupUtilities.Wpf.Contracts;
using BackupUtilities.Wpf.ViewModels.Shared;
using BackupUtilities.Wpf.Views.Mirror;

/// <summary>
/// The view model for <see cref="MirrorFileDetailsView"/>.
/// </summary>
public class MirrorFileDetailsViewModel : FileDetailsViewModelBase
{
    private readonly ISelectedFileService _selectedFileService;

    /// <summary>
    /// Initializes a new instance of the <see cref="MirrorFileDetailsViewModel"/> class.
    /// </summary>
    /// <param name="selectedFileService">The service to manage the currently selected file.</param>
    /// <param name="projectManager">Manages the current project.</param>
    public MirrorFileDetailsViewModel(
        ISelectedFileService selectedFileService,
        IProjectManager projectManager)
        : base(projectManager)
    {
        _selectedFileService = selectedFileService;

        _selectedFileService.SelectedMirrorFileChanged += OnSelectedMirrorFileChanged;
    }

    /// <summary>
    /// Gets a value indicating whether this folder has been deleted in the meantime.
    /// </summary>
    public string Touched => string.Empty;

    /// <summary>
    /// Gets a value indicating whether to display the 'touched' fields on the UI. In this case dont.
    /// </summary>
    public bool HasTouchedAttribute => false;

    /// <summary>
    /// Gets a value indicating the duplication level of this folder.
    /// </summary>
    public string IsDuplicate => (Duplicates.Count > 0).ToString();

    private async void OnSelectedMirrorFileChanged(object? sender, EventArgs e)
    {
        try
        {
            await OnSelectedFileChanged(_selectedFileService.SelectedMirrorFile);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
        }
    }
}
