namespace BackupUtilities.Wpf.ViewModels.Working;

using System;
using System.Diagnostics;
using BackupUtilities.Services.Interfaces;
using BackupUtilities.Wpf.Contracts;
using BackupUtilities.Wpf.ViewModels.Shared;
using BackupUtilities.Wpf.Views.Working;

/// <summary>
/// The view model for <see cref="FolderDetailsView"/>.
/// </summary>
public class FolderDetailsViewModel : FolderDetailsViewModelBase
{
    private readonly ISelectedFolderService _selectedFolderService;

    /// <summary>
    /// Initializes a new instance of the <see cref="FolderDetailsViewModel"/> class.
    /// </summary>
    /// <param name="selectedFolderService">The service to manage the currently selected folder.</param>
    /// <param name="projectManager">Manages the current project.</param>
    public FolderDetailsViewModel(
        ISelectedFolderService selectedFolderService,
        IProjectManager projectManager)
        : base(selectedFolderService, projectManager)
    {
        _selectedFolderService = selectedFolderService;

        _selectedFolderService.SelectedFolderChanged += OnSelectedFolderChanged;
    }

    /// <summary>
    /// Gets a value indicating whether to display the 'touched' fields on the UI. In this case yes.
    /// </summary>
    public bool HasTouchedAttribute => true;

    private async void OnSelectedFolderChanged(object? sender, EventArgs e)
    {
        try
        {
            await OnSelectedFolderChanged(_selectedFolderService.SelectedFolder);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
        }
    }
}
