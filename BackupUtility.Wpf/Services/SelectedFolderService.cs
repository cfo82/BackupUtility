namespace BackupUtilities.Wpf.Services;

using System;
using BackupUtilities.Data.Interfaces;
using BackupUtilities.Wpf.Contracts;

/// <summary>
/// Default implementation of <see cref="ISelectedFolderService"/>.
/// </summary>
public class SelectedFolderService : ISelectedFolderService
{
    private Folder? _selectedFolder;
    private Folder? _selectedMirrorFolder;

    /// <summary>
    /// Initializes a new instance of the <see cref="SelectedFolderService"/> class.
    /// </summary>
    public SelectedFolderService()
    {
        _selectedFolder = null;
        FireEvents = true;
    }

    /// <inheritdoc/>
    public event EventHandler<SelectedFolderChangedEventArgs>? SelectedFolderChanged;

    /// <inheritdoc/>
    public event EventHandler<EventArgs>? SelectedMirrorFolderChanged;

    /// <inheritdoc/>
    public bool FireEvents { get; set; }

    /// <inheritdoc/>
    public Folder? SelectedFolder
    {
        get
        {
            return _selectedFolder;
        }

        set
        {
            if (value != _selectedFolder)
            {
                var previous = _selectedFolder;
                _selectedFolder = value;
                if (FireEvents)
                {
                    SelectedFolderChanged?.Invoke(this, new SelectedFolderChangedEventArgs(previous, _selectedFolder));
                }
            }
        }
    }

    /// <inheritdoc/>
    public Folder? SelectedMirrorFolder
    {
        get
        {
            return _selectedMirrorFolder;
        }

        set
        {
            if (value != _selectedMirrorFolder)
            {
                _selectedMirrorFolder = value;
                if (FireEvents)
                {
                    SelectedMirrorFolderChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }
    }
}
