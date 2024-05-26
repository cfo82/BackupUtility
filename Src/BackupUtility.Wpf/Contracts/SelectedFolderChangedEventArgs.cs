namespace BackupUtilities.Wpf.Contracts;

using System;
using BackupUtilities.Data.Interfaces;

/// <summary>
/// Implementation of <see cref="EventArgs"/> for the folder changed event.
/// </summary>
public class SelectedFolderChangedEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SelectedFolderChangedEventArgs"/> class.
    /// </summary>
    /// <param name="previousSelection">The previously selected folder.</param>
    /// <param name="newSelection">The newly selected folder.</param>
    public SelectedFolderChangedEventArgs(Folder? previousSelection, Folder? newSelection)
    {
        PreviousSelection = previousSelection;
        NewSelection = newSelection;
    }

    /// <summary>
    /// Gets the previously selected folder.
    /// </summary>
    public Folder? PreviousSelection { get; }

    /// <summary>
    /// Gets the newly selected folder.
    /// </summary>
    public Folder? NewSelection { get; }
}
