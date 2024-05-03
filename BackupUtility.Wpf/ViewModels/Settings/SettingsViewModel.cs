namespace BackupUtilities.Wpf.ViewModels.Settings;

using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Input;
using BackupUtilities.Data.Interfaces;
using BackupUtilities.Services.Interfaces;
using BackupUtilities.Wpf.Views.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Mvvm;

/// <summary>
/// The view model for <see cref="SettingsView"/>.
/// </summary>
public class SettingsViewModel : BindableBase
{
    private readonly ILogger<SettingsViewModel> _logger;
    private readonly IProjectManager _projectManager;
    private Settings? _settings;
    private string _rootPath;
    private string _mirrorPath;
    private IgnoredFolderViewModel? _selectedIgnoredFolder;
    private bool _changed;
    private bool _enabled;

    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsViewModel"/> class.
    /// </summary>
    /// <param name="logger">The lgoger to use.</param>
    /// <param name="projectManager">Manages the current project.</param>
    public SettingsViewModel(
        ILogger<SettingsViewModel> logger,
        IProjectManager projectManager)
    {
        _logger = logger;
        _projectManager = projectManager;

        _rootPath = string.Empty;
        _mirrorPath = string.Empty;
        _changed = false;

        SelectRootPathCommand = new DelegateCommand(OnSelectRootPath);
        SelectMirrorPathCommand = new DelegateCommand(OnSelectMirrorPath);
        AddIgnoredFolderCommand = new DelegateCommand(OnAddIgnoredFolder);
        RemoveSelectedIgnoredFolderCommand = new DelegateCommand(OnRemoveSelectedIgnoredFolder);
        SaveCommand = new DelegateCommand(OnSave);

        IgnoredFolders = new ObservableCollection<IgnoredFolderViewModel>();
        IgnoredFolders.CollectionChanged += OnIgnoredFoldersChanged;

        _projectManager.CurrentProjectChanged += OnCurrentProjectChanged;
        OnCurrentProjectChanged(null, EventArgs.Empty);
    }

    /// <summary>
    /// Gets the command to select the root path.
    /// </summary>
    public ICommand SelectRootPathCommand { get; }

    /// <summary>
    /// Gets the command to select the mirror path.
    /// </summary>
    public ICommand SelectMirrorPathCommand { get; }

    /// <summary>
    /// Gets the command to add a new folder to the ignored folder list.
    /// </summary>
    public ICommand AddIgnoredFolderCommand { get; }

    /// <summary>
    /// Gets the command to remove the selected ignored folder from the ignored folder list.
    /// </summary>
    public ICommand RemoveSelectedIgnoredFolderCommand { get; }

    /// <summary>
    /// Gets the command to save the settings.
    /// </summary>
    public ICommand SaveCommand { get; }

    /// <summary>
    /// Gets an observable collection of folders to ignore during the scan.
    /// </summary>
    public ObservableCollection<IgnoredFolderViewModel> IgnoredFolders { get; }

    /// <summary>
    /// Gets or sets the root path.
    /// </summary>
    public string RootPath
    {
        get
        {
            return _rootPath;
        }

        set
        {
            if (SetProperty(ref _rootPath, value))
            {
                CheckChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets the mirror path.
    /// </summary>
    public string MirrorPath
    {
        get
        {
            return _mirrorPath;
        }

        set
        {
            if (SetProperty(ref _mirrorPath, value))
            {
                CheckChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets the selected ignored folder view model.
    /// </summary>
    public IgnoredFolderViewModel? SelectedIgnoredFolder
    {
        get
        {
            return _selectedIgnoredFolder;
        }

        set
        {
            if (SetProperty(ref _selectedIgnoredFolder, value))
            {
                RaisePropertyChanged(nameof(IsRemoveSelectedIgnoredFolderEnabled));
            }
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the settings have changed.
    /// </summary>
    public bool HasChanged
    {
        get { return _changed; }
        set { SetProperty(ref _changed, value); }
    }

    /// <summary>
    /// Gets or sets a value indicating whether a project is loaded and thus controls should be enabled or disabled.
    /// </summary>
    public bool IsEnabled
    {
        get { return _enabled; }
        set { SetProperty(ref _enabled, value); }
    }

    /// <summary>
    /// Gets a value indicating whether an ignored folder is selected.
    /// </summary>
    public bool IsRemoveSelectedIgnoredFolderEnabled
    {
        get { return _selectedIgnoredFolder != null; }
    }

    private void CheckChanged()
    {
        if (_settings == null)
        {
            HasChanged = true;
        }
        else
        {
            var hasChanged = !string.Equals(RootPath, _settings.RootPath)
                || !string.Equals(MirrorPath, _settings.MirrorPath);

            hasChanged = hasChanged || !_settings.IgnoredFolders
                .Select(f => f.Path)
                .Order()
                .SequenceEqual(
                    IgnoredFolders
                        .Select(f => f.IgnoredFolder.Path)
                        .Order());

            HasChanged = hasChanged;
        }
    }

    private string? OpenFolder(string title, string initialPath)
    {
        var dialog = new OpenFolderDialog();

        dialog.Multiselect = false;
        dialog.Title = title;
        if (!string.IsNullOrEmpty(initialPath))
        {
            dialog.InitialDirectory = initialPath;
        }

        var result = dialog.ShowDialog();

        if (result ?? false)
        {
            return dialog.FolderName;
        }

        return null;
    }

    private void OnSelectRootPath()
    {
        var result = OpenFolder("Select the root folder of the live working tree", RootPath);
        if (result != null)
        {
            RootPath = result;
        }
    }

    private void OnSelectMirrorPath()
    {
        var result = OpenFolder("Select the root folder of the mirror tree", MirrorPath);
        if (result != null)
        {
            MirrorPath = result;
        }
    }

    private void OnAddIgnoredFolder()
    {
        var result = OpenFolder("Select the folder that should be ignored during scans", RootPath);
        if (result != null && !IgnoredFolders.Any(f => string.Equals(f.Path, result)))
        {
            IgnoredFolders.Add(new IgnoredFolderViewModel(new IgnoredFolder { Path = result, }));
        }
    }

    private void OnRemoveSelectedIgnoredFolder()
    {
        if (_selectedIgnoredFolder != null)
        {
            var index = IgnoredFolders.IndexOf(_selectedIgnoredFolder);
            IgnoredFolders.Remove(_selectedIgnoredFolder);
            if (index >= IgnoredFolders.Count && index > 0)
            {
                --index;
            }

            if (index < IgnoredFolders.Count)
            {
                SelectedIgnoredFolder = IgnoredFolders[index];
            }
            else
            {
                SelectedIgnoredFolder = null;
            }
        }
    }

    private async void OnSave()
    {
        var previousSettings = _settings;

        try
        {
            if (_projectManager.CurrentProject == null)
            {
                throw new InvalidOperationException("Project is not opened.");
            }

            var settingsRepository = _projectManager.CurrentProject.Data.SettingsRepository;

            _settings = new Settings()
            {
                RootPath = RootPath,
                MirrorPath = MirrorPath,
                IgnoredFolders = IgnoredFolders.Select(f => f.IgnoredFolder).ToList(),
            };

            _settings = await _projectManager.CurrentProject.SaveSettingsAsync(_settings);

            RootPath = _settings.RootPath;
            MirrorPath = _settings.MirrorPath;
            IgnoredFolders.Clear();
            IgnoredFolders.AddRange(_settings.IgnoredFolders.Select(f => new IgnoredFolderViewModel(f)));
            HasChanged = false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error has occured during initialization.");
            _settings = previousSettings;
        }
    }

    private async void OnCurrentProjectChanged(object? sender, EventArgs e)
    {
        try
        {
            if (_projectManager.CurrentProject == null)
            {
                RootPath = string.Empty;
                MirrorPath = string.Empty;
                IgnoredFolders.Clear();
                HasChanged = false;
                IsEnabled = false;
                return;
            }

            var connection = _projectManager.CurrentProject.Data.Connection;
            var settingsRepository = _projectManager.CurrentProject.Data.SettingsRepository;

            _settings = await settingsRepository.GetSettingsAsync(connection);
            RootPath = _settings.RootPath;
            MirrorPath = _settings.MirrorPath;
            IgnoredFolders.Clear();
            IgnoredFolders.AddRange(_settings.IgnoredFolders.Select(f => new IgnoredFolderViewModel(f)));
            HasChanged = false;
            IsEnabled = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error has occured during initialization.");
        }
    }

    private void OnIgnoredFoldersChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        CheckChanged();
    }
}
