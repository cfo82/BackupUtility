namespace BackupUtilities.Wpf.ViewModels.Settings;

using BackupUtilities.Data.Interfaces;
using Prism.Mvvm;

/// <summary>
/// ViewModel representing an ignored folder for the settings.
/// </summary>
public class IgnoredFolderViewModel : BindableBase
{
    private IgnoredFolder _ignoredFolder;

    /// <summary>
    /// Initializes a new instance of the <see cref="IgnoredFolderViewModel"/> class.
    /// </summary>
    /// <param name="ignoredFolder">The ignored folder instance that is represented by this view model.</param>
    public IgnoredFolderViewModel(IgnoredFolder ignoredFolder)
    {
        _ignoredFolder = ignoredFolder;
    }

    /// <summary>
    /// Gets the absolute path to the ignored folder.
    /// </summary>
    public string Path => _ignoredFolder.Path;

    /// <summary>
    /// Gets the ignored folder instance that is represented by this view model.
    /// </summary>
    public IgnoredFolder IgnoredFolder => _ignoredFolder;
}
