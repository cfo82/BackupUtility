namespace BackupUtilities.Wpf.ViewModels.Scans;

using System;
using System.Globalization;
using BackupUtilities.Services.Interfaces;
using BackupUtilities.Wpf.Views.Scans;
using Prism.Mvvm;

/// <summary>
/// View model for <see cref="SimpleScanView"/>.
/// </summary>
public class ScanSettingsViewModel : BindableBase
{
    private readonly IProjectManager _projectManager;
    private IBackupProject? _currentProject;
    private string _scanTitle;
    private string _settingsWorkingDrive;
    private string _settingsMirrorDrive;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScanSettingsViewModel"/> class.
    /// </summary>
    /// <param name="projectManager">The project manager.</param>
    public ScanSettingsViewModel(
        IProjectManager projectManager)
    {
        _projectManager = projectManager;

        _currentProject = null;
        _scanTitle = string.Empty;
        _settingsWorkingDrive = string.Empty;
        _settingsMirrorDrive = string.Empty;

        _projectManager.CurrentProjectChanged += OnCurrentProjectChanged;

        OnCurrentProjectChanged(_projectManager, EventArgs.Empty);
    }

    /// <summary>
    /// Gets or sets the current scan title.
    /// </summary>
    public string ScanTitle
    {
        get { return _scanTitle; }
        set { SetProperty(ref _scanTitle, value); }
    }

    /// <summary>
    /// Gets or sets the working drive settings.
    /// </summary>
    public string SettingsWorkingDrive
    {
        get { return _settingsWorkingDrive; }
        set { SetProperty(ref _settingsWorkingDrive, value); }
    }

    /// <summary>
    /// Gets or sets the mirror drive settings.
    /// </summary>
    public string SettingsMirrorDrive
    {
        get { return _settingsMirrorDrive; }
        set { SetProperty(ref _settingsMirrorDrive, value); }
    }

    private void OnCurrentProjectChanged(object? sender, EventArgs e)
    {
        if (_currentProject != null)
        {
            _currentProject.CurrentScanChanged -= OnCurrentScanChanged;
        }

        _currentProject = _projectManager.CurrentProject;

        if (_currentProject != null)
        {
            _currentProject.CurrentScanChanged += OnCurrentScanChanged;
        }

        OnCurrentScanChanged(null, EventArgs.Empty);
    }

    private void OnCurrentScanChanged(object? sender, EventArgs e)
    {
        var currentScan = _currentProject?.CurrentScan;

        if (currentScan == null)
        {
            ScanTitle = "No scan available.";
            SettingsWorkingDrive = string.Empty;
            SettingsMirrorDrive = string.Empty;
        }
        else
        {
            var date = currentScan.Data.CreatedDate;
            date = date.ToLocalTime();
            var dateString = date.ToString(CultureInfo.CurrentUICulture.DateTimeFormat.ShortDatePattern, CultureInfo.CurrentUICulture);
            var timeString = date.ToString(CultureInfo.CurrentUICulture.DateTimeFormat.ShortTimePattern, CultureInfo.CurrentUICulture);
            ScanTitle = $"Scan {dateString} {timeString}";
            SettingsWorkingDrive = currentScan.Settings.RootPath;
            SettingsMirrorDrive = currentScan.Settings.MirrorPath;
        }
    }
}
