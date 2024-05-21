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
    public event EventHandler<SelectedFolderChangedEventArgs>? SelectedFolderChanged;

    /// <summary>
    /// Fired when the selected folder on the mirror drive changes.
    /// </summary>
    public event EventHandler<EventArgs>? SelectedMirrorFolderChanged;

    /// <summary>
    /// Gets or sets a value indicating whether events are fired when the properties change.
    /// </summary>
    public bool FireEvents { get; set; }

    /// <summary>
    /// Gets or sets the currently selected folder.
    /// </summary>
    Folder? SelectedFolder { get; set; }

    /// <summary>
    /// Gets or sets the currently selected mirror drive folder.
    /// </summary>
    Folder? SelectedMirrorFolder { get; set; }
}
