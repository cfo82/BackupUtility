namespace BackupUtilities.Wpf.ViewModels.Mirror;

using System;
using System.Diagnostics;
using BackupUtilities.Services.Interfaces;
using BackupUtilities.Wpf.Contracts;
using BackupUtilities.Wpf.ViewModels.Shared;
using BackupUtilities.Wpf.Views.Working;

/// <summary>
/// The view model for <see cref="FolderDetailsView"/>.
/// </summary>
public class MirrorFolderDetailsViewModel : FolderDetailsViewModelBase
{
    private readonly ISelectedFolderService _selectedFolderService;

    /// <summary>
    /// Initializes a new instance of the <see cref="MirrorFolderDetailsViewModel"/> class.
    /// </summary>
    /// <param name="selectedFolderService">The service to manage the currently selected folder.</param>
    /// <param name="projectManager">Manages the current project.</param>
    public MirrorFolderDetailsViewModel(
        ISelectedFolderService selectedFolderService,
        IProjectManager projectManager)
        : base(selectedFolderService, projectManager)
    {
        _selectedFolderService = selectedFolderService;

        _selectedFolderService.SelectedMirrorFolderChanged += OnSelectedMirrorFolderChanged;
    }

    /// <summary>
    /// Gets a value indicating whether to display the 'touched' fields on the UI. In this case dont.
    /// </summary>
    public bool HasTouchedAttribute => false;

    /// <summary>
    /// Gets a value indicating whether this view model should display folder size data.
    /// </summary>
    public bool HasFolderSizeData => false;

    private async void OnSelectedMirrorFolderChanged(object? sender, EventArgs e)
    {
        try
        {
            await OnSelectedFolderChanged(_selectedFolderService.SelectedMirrorFolder);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
        }
    }
}
