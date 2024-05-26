namespace BackupUtilities.Wpf.Services;

using System;
using BackupUtilities.Data.Interfaces;
using BackupUtilities.Wpf.Contracts;

/// <summary>
/// Default implementation of <see cref="ISelectedFileService"/>.
/// </summary>
public class SelectedFileService : ISelectedFileService
{
    private File? _selectedFile;
    private OrphanedFile? _selectedMirrorFile;

    /// <summary>
    /// Initializes a new instance of the <see cref="SelectedFileService"/> class.
    /// </summary>
    public SelectedFileService()
    {
        _selectedFile = null;
        _selectedMirrorFile = null;
    }

    /// <inheritdoc/>
    public event EventHandler<EventArgs>? SelectedFileChanged;

    /// <inheritdoc/>
    public event EventHandler<EventArgs>? SelectedMirrorFileChanged;

    /// <inheritdoc/>
    public File? SelectedFile
    {
        get
        {
            return _selectedFile;
        }

        set
        {
            if (value != _selectedFile)
            {
                _selectedFile = value;
                SelectedFileChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    /// <inheritdoc/>
    public OrphanedFile? SelectedMirrorFile
    {
        get
        {
            return _selectedMirrorFile;
        }

        set
        {
            if (value != _selectedMirrorFile)
            {
                _selectedMirrorFile = value;
                SelectedMirrorFileChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
