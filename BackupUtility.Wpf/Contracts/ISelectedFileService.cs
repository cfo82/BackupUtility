namespace BackupUtilities.Wpf.Contracts;

using System;
using BackupUtilities.Data.Interfaces;

/// <summary>
/// Manages the selected folder.
/// </summary>
public interface ISelectedFileService
{
    /// <summary>
    /// Fired when <see cref="SelectedFile"/> changed.
    /// </summary>
    public event EventHandler<EventArgs>? SelectedFileChanged;

    /// <summary>
    /// Fired when <see cref="SelectedMirrorFile"/> changed.
    /// </summary>
    public event EventHandler<EventArgs>? SelectedMirrorFileChanged;

    /// <summary>
    /// Gets or sets the currently selected file.
    /// </summary>
    File? SelectedFile { get; set; }

    /// <summary>
    /// Gets or sets the currently selected file on the mirror drive.
    /// </summary>
    OrphanedFile? SelectedMirrorFile { get; set; }
}
