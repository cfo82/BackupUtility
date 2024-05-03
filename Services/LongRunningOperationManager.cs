namespace BackupUtilities.Services;

using System;
using System.Threading.Tasks;
using BackupUtilities.Services.Interfaces;

/// <summary>
/// Default implementation of <see cref="ILongRunningOperationManager"/>.
/// </summary>
public class LongRunningOperationManager : ILongRunningOperationManager
{
    private readonly IUiDispatcherService _uiDispatcherService;
    private bool _isRunning;
    private string _title;
    private string _text;
    private double? _progress;

    /// <summary>
    /// Initializes a new instance of the <see cref="LongRunningOperationManager"/> class.
    /// </summary>
    /// <param name="uiDispatcherService">The UI Dispatcher Service.</param>
    public LongRunningOperationManager(
        IUiDispatcherService uiDispatcherService)
    {
        _isRunning = false;
        _title = string.Empty;
        _text = string.Empty;
        _progress = null;
        _uiDispatcherService = uiDispatcherService;
    }

    /// <inheritdoc />
    public event EventHandler<EventArgs>? OperationChanged;

    /// <inheritdoc />
    public bool IsRunning => _isRunning;

    /// <inheritdoc />
    public string Title => _title;

    /// <inheritdoc />
    public string Text => _text;

    /// <inheritdoc />
    public double? Percentage => _progress;

    /// <inheritdoc />
    public async Task BeginOperationAsync(string title)
    {
        var completionSource = new TaskCompletionSource();
        _uiDispatcherService.Post(() =>
        {
            _isRunning = true;
            _title = title;
            OperationChanged?.Invoke(this, EventArgs.Empty);
            completionSource.SetResult();
        });

        await completionSource.Task;
    }

    /// <inheritdoc />
    public async Task UpdateOperationAsync(string text, double? percentage)
    {
        var completionSource = new TaskCompletionSource();
        _uiDispatcherService.Post(() =>
        {
            _text = text;
            _progress = percentage;
            OperationChanged?.Invoke(this, EventArgs.Empty);
            completionSource.SetResult();
        });

        await completionSource.Task;
    }

    /// <inheritdoc />
    public async Task UpdateOperationAsync(double? percentage)
    {
        await UpdateOperationAsync(_text, percentage);
    }

    /// <inheritdoc />
    public async Task EndOperationAsync()
    {
        var completionSource = new TaskCompletionSource();
        _uiDispatcherService.Post(() =>
        {
            _isRunning = false;
            OperationChanged?.Invoke(this, EventArgs.Empty);
            completionSource.SetResult();
        });

        await completionSource.Task;
    }
}
