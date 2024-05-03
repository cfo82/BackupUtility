namespace BackupUtilities.Wpf.Contracts;

using System;
using BackupUtilities.Data.Interfaces;

/// <summary>
/// Manages the selected folder.
/// </summary>
public interface ISelectedFolderService
{
    /// <summary>
    /// Fired when the selected folder changes.
    /// </summary>
    public event EventHandler<EventArgs>? SelectedFolderChanged;

    /// <summary>
    /// Fired when the selected folder on the mirror drive changes.
    /// </summary>
    public event EventHandler<EventArgs>? SelectedMirrorFolderChanged;

    /// <summary>
    /// Gets or sets the currently selected folder.
    /// </summary>
    Folder? SelectedFolder { get; set; }

    /// <summary>
    /// Gets or sets the currently selected mirror drive folder.
    /// </summary>
    Folder? SelectedMirrorFolder { get; set; }
}
