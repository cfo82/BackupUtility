namespace BackupUtilities.Services.Services.Status;

using System;
using System.Threading.Tasks;
using BackupUtilities.Services.Interfaces;
using BackupUtilities.Services.Interfaces.Status;

/// <summary>
/// Default implementation of <see cref="IScanStatus"/>.
/// </summary>
public class ScanStatus : IScanStatus
{
    private readonly IUiDispatcherService _uiDispatcherService;
    private bool _isRunning;
    private string _title;
    private string _text;
    private double? _progress;
    private SemaphoreSlim _lock;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScanStatus"/> class.
    /// </summary>
    /// <param name="uiDispatcherService">The UI Dispatcher Service.</param>
    /// <param name="title">Title of this status object.</param>
    public ScanStatus(
        IUiDispatcherService uiDispatcherService,
        string title)
    {
        _uiDispatcherService = uiDispatcherService;
        _isRunning = false;
        _title = title;
        _text = string.Empty;
        _progress = null;
        _lock = new SemaphoreSlim(1);
    }

    /// <inheritdoc />
    public event EventHandler<EventArgs>? Changed;

    /// <inheritdoc />
    public bool IsRunning => _isRunning;

    /// <inheritdoc />
    public string Title => _title;

    /// <inheritdoc />
    public string Text => _text;

    /// <inheritdoc />
    public double? Progress => _progress;

    /// <inheritdoc />
    public virtual async Task ResetAsync()
    {
        await RunSynchronizedAsync(() =>
        {
            _isRunning = false;
            _text = "Not yet started.";
            _progress = null;
        });

        await RaiseChangedEventAsync();
    }

    /// <inheritdoc />
    public async Task BeginAsync()
    {
        await RunSynchronizedAsync(() =>
        {
            _isRunning = true;
        });

        await RaiseChangedEventAsync();
    }

    /// <inheritdoc />
    public async Task UpdateAsync(string text, double? percentage)
    {
        await RunSynchronizedAsync(() =>
        {
            _text = text;
            _progress = percentage;
        });

        await RaiseChangedEventAsync();
    }

    /// <inheritdoc />
    public async Task UpdateAsync(double? percentage)
    {
        await UpdateAsync(_text, percentage);
    }

    /// <inheritdoc />
    public async Task EndAsync()
    {
        await RunSynchronizedAsync(() =>
        {
            _text = "Finished.";
            _progress = 1.0;
            _isRunning = false;
        });

        await RaiseChangedEventAsync();
    }

    /// <summary>
    /// Run the given action guarded by the sempahore.
    /// </summary>
    /// <param name="a">The action to run as synchronized code.</param>
    /// <returns>A task for async programming.</returns>
    protected async Task RunSynchronizedAsync(Action a)
    {
        try
        {
            await _lock.WaitAsync();

            a.Invoke();
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// Raise the changed event.
    /// </summary>
    /// <returns>A task for async programming.</returns>
    protected async Task RaiseChangedEventAsync()
    {
        if (_uiDispatcherService.CheckAccess())
        {
            Changed?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            var completionSource = new TaskCompletionSource();
            _uiDispatcherService.Post(() =>
            {
                Changed?.Invoke(this, EventArgs.Empty);
                completionSource.SetResult();
            });

            await completionSource.Task;
        }
    }
}
