namespace BackupUtilities.Wpf.ViewModels;

using System;
using BackupUtilities.Services.Interfaces;
using BackupUtilities.Wpf.Views;
using Prism.Mvvm;

/// <summary>
/// The view model for <see cref="LongRunningOperationStatusView"/>.
/// </summary>
public class LongRunningOperationStatusViewModel : BindableBase
{
    private readonly ILongRunningOperationManager _longRunningOperationManager;
    private string _title;
    private string _text;
    private double _progress;
    private bool _hasProgress;

    /// <summary>
    /// Initializes a new instance of the <see cref="LongRunningOperationStatusViewModel"/> class.
    /// </summary>
    /// <param name="longRunningOperationManager">The long running operation manager.</param>
    public LongRunningOperationStatusViewModel(
        ILongRunningOperationManager longRunningOperationManager)
    {
        _longRunningOperationManager = longRunningOperationManager;
        _title = string.Empty;
        _text = string.Empty;

        _longRunningOperationManager.OperationChanged += OnOperationChanged;
    }

    /// <summary>
    /// Gets or sets the title of the current long running operation.
    /// </summary>
    public string Title
    {
        get { return _title; }
        set { SetProperty(ref _title, value); }
    }

    /// <summary>
    /// Gets or sets the text of the current long running operation.
    /// </summary>
    public string Text
    {
        get { return _text; }
        set { SetProperty(ref _text, value); }
    }

    /// <summary>
    /// Gets or sets the current progress of the long running operation.
    /// </summary>
    public double Progress
    {
        get { return _progress; }
        set { SetProperty(ref _progress, value); }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the current operation has a progress indication.
    /// </summary>
    public bool HasProgress
    {
        get { return _hasProgress; }
        set { SetProperty(ref _hasProgress, value); }
    }

    private void OnOperationChanged(object? sender, EventArgs e)
    {
        if (!_longRunningOperationManager.IsRunning)
        {
            Title = string.Empty;
            Text = string.Empty;
            HasProgress = false;
            Progress = 0;
        }
        else
        {
            Title = _longRunningOperationManager.Title;
            Text = _longRunningOperationManager.Text;
            HasProgress = _longRunningOperationManager.Percentage.HasValue;
            Progress = _longRunningOperationManager.Percentage.HasValue ? _longRunningOperationManager.Percentage.Value : 0;
        }
    }
}
