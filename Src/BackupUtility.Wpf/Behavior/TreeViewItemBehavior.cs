namespace BackupUtilities.Wpf.Behavior;

using System;
using System.Windows;
using System.Windows.Controls;

/// <summary>
/// Exposes attached behaviors that can be
/// applied to TreeViewItem objects.
/// </summary>
public static class TreeViewItemBehavior
{
    /// <summary>
    /// Gets a dependency property that says if the item should be brought into view when its selected.
    /// </summary>
    public static readonly DependencyProperty IsBroughtIntoViewWhenSelectedProperty =
        DependencyProperty.RegisterAttached(
        "IsBroughtIntoViewWhenSelected",
        typeof(bool),
        typeof(TreeViewItemBehavior),
        new UIPropertyMetadata(false, OnIsBroughtIntoViewWhenSelectedChanged));

    /// <summary>
    /// Gets the value of the <see cref="IsBroughtIntoViewWhenSelectedProperty"/> attached property.
    /// </summary>
    /// <param name="treeViewItem">The tree view item from which the property should be read.</param>
    /// <returns>The value indicating whether the item should be brought into view when selected.</returns>
    public static bool GetIsBroughtIntoViewWhenSelected(TreeViewItem treeViewItem)
    {
        return (bool)treeViewItem.GetValue(IsBroughtIntoViewWhenSelectedProperty);
    }

    /// <summary>
    /// Sets the value of the <see cref="IsBroughtIntoViewWhenSelectedProperty"/> attached property.
    /// </summary>
    /// <param name="treeViewItem">The tree view item for which the property should be written.</param>
    /// <param name="value">The value indicating whether the item should be brought into view when selected.</param>
    public static void SetIsBroughtIntoViewWhenSelected(
      TreeViewItem treeViewItem, bool value)
    {
        treeViewItem.SetValue(IsBroughtIntoViewWhenSelectedProperty, value);
    }

    private static void OnIsBroughtIntoViewWhenSelectedChanged(
      DependencyObject depObj,
      DependencyPropertyChangedEventArgs e)
    {
        TreeViewItem? item = depObj as TreeViewItem;
        if (item == null)
        {
            return;
        }

        if (e.NewValue is bool == false)
        {
            return;
        }

        if ((bool)e.NewValue)
        {
            item.Selected += OnTreeViewItemSelected;
        }
        else
        {
            item.Selected -= OnTreeViewItemSelected;
        }
    }

    private static void OnTreeViewItemSelected(object sender, RoutedEventArgs e)
    {
        // Only react to the Selected event raised by the TreeViewItem
        // whose IsSelected property was modified. Ignore all ancestors
        // who are merely reporting that a descendant's Selected fired.
        if (!object.ReferenceEquals(sender, e.OriginalSource))
        {
            return;
        }

        TreeViewItem? item = e.OriginalSource as TreeViewItem;
        if (item != null)
        {
            item.BringIntoView();
        }
    }
}
