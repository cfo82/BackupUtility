namespace BackupUtilities.Wpf.ViewModels;

using System;
using System.Windows.Input;
using BackupUtilities.Services.Interfaces;
using BackupUtilities.Wpf.Views;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Mvvm;

/// <summary>
/// The view model for <see cref="ToolBarView"/>.
/// </summary>
public class ToolBarViewModel : BindableBase
{
    private readonly ILogger<ToolBarViewModel> _logger;
    private readonly IProjectManager _projectManager;
    private IBackupProject? _currentProject;
    private bool _isReady = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="ToolBarViewModel"/> class.
    /// </summary>
    /// <param name="logger">The logger to use.</param>
    /// <param name="projectManager">The project manager.</param>
    public ToolBarViewModel(
        ILogger<ToolBarViewModel> logger,
        IProjectManager projectManager)
    {
        _logger = logger;
        _projectManager = projectManager;

        OpenCommand = new DelegateCommand(OnOpen);
        CreateCommand = new DelegateCommand(OnCreate);

        _projectManager.CurrentProjectChanged += OnCurrentProjectChanged;
        OnCurrentProjectChanged(null, EventArgs.Empty);
    }

    /// <summary>
    /// Gets a command to open an existing backup project.
    /// </summary>
    public ICommand OpenCommand { get; private set; }

    /// <summary>
    /// Gets a command to create a new backup project.
    /// </summary>
    public ICommand CreateCommand { get; private set; }

    /// <summary>
    /// Gets or sets a value indicating whether the project is ready to be run.
    /// </summary>
    public bool IsReady
    {
        get { return _isReady; }
        set { SetProperty(ref _isReady, value); }
    }

    private void OnCurrentProjectChanged(object? sender, EventArgs e)
    {
        if (_currentProject != null)
        {
            _currentProject.IsReadyChanged -= OnIsReadyChanged;
        }

        _currentProject = _projectManager.CurrentProject;

        if (_currentProject != null)
        {
            _currentProject.IsReadyChanged += OnIsReadyChanged;
        }

        OnIsReadyChanged(null, EventArgs.Empty);
    }

    private void OnIsReadyChanged(object? sender, EventArgs e)
    {
        IsReady = _currentProject != null && _currentProject.IsReady;
    }

    private async void OnOpen()
    {
        try
        {
            var dialog = new OpenFileDialog();
            dialog.FileName = string.Empty;
            dialog.DefaultExt = ".bproj";
            dialog.Filter = "Backup projects (.bproj)|*.bproj";

            var result = dialog.ShowDialog();

            if (result ?? false)
            {
                await _projectManager.OpenProjectAsync(dialog.FileName);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while opening the database.");
        }
    }

    private async void OnCreate()
    {
        try
        {
            var dialog = new SaveFileDialog();
            dialog.FileName = string.Empty;
            dialog.DefaultExt = ".bproj";
            dialog.Filter = "Backup projects (.bproj)|*.bproj";

            var result = dialog.ShowDialog();

            if (result ?? false)
            {
                await _projectManager.CreateProjectAsync(dialog.FileName);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while creating the database.");
        }
    }
}
