using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using LiteDbExplorer.Presentation;

namespace LiteDbExplorer.Modules.Database
{
    /// <summary>
    /// Interaction logic for DatabasesNavView.xaml
    /// </summary>
    public partial class DatabasesExplorerView : UserControl, IDatabasesExplorerView
    {
        public DatabasesExplorerView()
        {
            InitializeComponent();

            TreeDatabase.PreviewMouseRightButtonDown += (sender, e) =>
            {
                if ((e.OriginalSource as DependencyObject).VisualUpwardSearch() != null)
                {
                    return;
                }

                if (sender is TreeView treeView && treeView.SelectedItem != null)
                {
                    if (treeView.ItemContainerGenerator.ContainerFromItemRecursive(treeView.SelectedItem) is TreeViewItem treeViewItem)
                    {
                        treeViewItem.Focus();
                        treeViewItem.IsSelected = true;
                        e.Handled = true;
                    }
                }
            };
        }

        public async void FocusItem(object item, bool bringIntoView)
        {
            if (Dispatcher == null)
            {
                return;
            }

            await Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new Action(() =>
            {
                try
                {
                    var treeViewItem = TreeDatabase.ItemContainerGenerator.ContainerFromItemRecursive(item);
                    if (treeViewItem == null)
                    {
                        return;
                    }

                    treeViewItem.IsSelected = true;
                    treeViewItem.Focus();

                    if (bringIntoView)
                    {
                        var firsChildTreeViewItem = treeViewItem.FirstChildrenContainerFromItem();
                        if (firsChildTreeViewItem != null)
                        {
                            treeViewItem.IsExpanded = true;
                            firsChildTreeViewItem.BringIntoView();
                        }
                        else
                        {
                            treeViewItem.BringIntoView();
                        }
                    }
                }
                catch (Exception)
                {
                    // Ignore
                }
            }));
        }
        
        private void RecentItemMoreBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (OpenDatabase.ContextMenu != null)
            {
                OpenDatabase.ContextMenu.IsEnabled = true;
                OpenDatabase.ContextMenu.PlacementTarget = OpenDatabase;
                OpenDatabase.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                OpenDatabase.ContextMenu.IsOpen = true;
            }
        }

        private void OnPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var treeViewItem = (e.OriginalSource as DependencyObject).VisualUpwardSearch();
            if (treeViewItem != null)
            {
                treeViewItem.Focus();
                treeViewItem.IsSelected = true;
                e.Handled = true;
            }
        }
    }
}
