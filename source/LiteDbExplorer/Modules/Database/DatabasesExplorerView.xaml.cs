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

            var findTextHandled = false;

            PreviewKeyDown += (sender, args) =>
            {
                var resultText = FindTerm;
                findTextHandled = false;
                if (args.Key == Key.Back)
                {
                    if (resultText.Length > 0)
                    {
                        resultText = resultText.Remove( resultText.Length -1, 1);
                    }

                    findTextHandled = true;
                }
                else if (args.Key == Key.Escape)
                {
                    resultText = string.Empty;
                    findTextHandled = true;
                }
                else if (args.Key == Key.Enter)
                {
                    findTextHandled = true;
                }

                if (findTextHandled)
                {
                    FindTextAdorner.Visibility = string.IsNullOrEmpty(resultText) ? Visibility.Collapsed : Visibility.Visible;
                    FindTerm = resultText;
                    args.Handled = true;
                }
            };

            PreviewTextInput += (sender, args) =>
            {
                if (findTextHandled)
                {
                    return;
                }

                var resultText = FindTerm ?? string.Empty;
                
                resultText += args.Text;
                
                FindTextAdorner.Visibility = string.IsNullOrEmpty(resultText) ? Visibility.Collapsed : Visibility.Visible;

                FindTerm = resultText;
            };

            FindTextClose.MouseDown += (sender, args) =>
            {
                FindTerm = null;
                FindTextAdorner.Visibility = Visibility.Collapsed;
            };

            FindDisplayModeHighlight.MouseDown += (sender, args) =>
            {
                FindDisplayModeIsCollapsed = false;
                HandleTreeViewItemByFindText();
            };

            FindDisplayModeCollapsed.MouseDown += (sender, args) =>
            {
                FindDisplayModeIsCollapsed = true;
                HandleTreeViewItemByFindText();
            };
        }

        public static readonly DependencyProperty FindTermProperty = DependencyProperty.Register(
            nameof(FindTerm), typeof(string), typeof(DatabasesExplorerView), new PropertyMetadata(default(string), OnFindTermPropertyChanged));

        private static void OnFindTermPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DatabasesExplorerView databasesExplorerView)
            {
                databasesExplorerView.HandleTreeViewItemByFindText();
            }
        }

        public static readonly DependencyProperty FindDisplayModeIsCollapsedProperty = DependencyProperty.Register(
            nameof(FindDisplayModeIsCollapsed), typeof(bool), typeof(DatabasesExplorerView), new PropertyMetadata(default(bool), OnFindTermPropertyChanged));

        public bool FindDisplayModeIsCollapsed
        {
            get => (bool) GetValue(FindDisplayModeIsCollapsedProperty);
            set => SetValue(FindDisplayModeIsCollapsedProperty, value);
        }

        public string FindTerm
        {
            get => (string) GetValue(FindTermProperty);
            set => SetValue(FindTermProperty, value);
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

        private void HandleTreeViewItemByFindText()
        {
            var modeIsCollapsed = FindDisplayModeIsCollapsed;
            if (modeIsCollapsed)
            {
                FindDisplayModeCollapsed.Visibility = Visibility.Collapsed;
                FindDisplayModeHighlight.Visibility = Visibility.Visible;
            }
            else
            {
                FindDisplayModeCollapsed.Visibility = Visibility.Visible;
                FindDisplayModeHighlight.Visibility = Visibility.Collapsed;
            }

            var treeViewItems = TreeDatabase.FindTreeViewItems();

            if (string.IsNullOrWhiteSpace(FindTerm))
            {
                treeViewItems.ForEach(p => p.Visibility = Visibility.Visible);
                return;
            }

            var hasMatchRank = 0;
            TreeViewItem selectTreeViewItem = null;
            foreach (var treeViewItem in treeViewItems)
            {
                var textBlock = treeViewItem.FindChildByType<TextBlock>();
                if (textBlock == null)
                {
                    continue;
                }

                
                if (textBlock.Text.StartsWith(FindTerm, StringComparison.OrdinalIgnoreCase))
                {
                    treeViewItem.Visibility = Visibility.Visible;
                    if (hasMatchRank < 2)
                    {
                        selectTreeViewItem = treeViewItem;
                        hasMatchRank = 2;
                    }
                }
                else if (textBlock.Text.ToLower().Contains(FindTerm.ToLower()))
                {
                    treeViewItem.Visibility = Visibility.Visible;
                    
                    if (hasMatchRank < 1)
                    {
                        selectTreeViewItem = treeViewItem;
                        hasMatchRank = 1;
                    }
                }
                else if (modeIsCollapsed)
                {
                    treeViewItem.Visibility = Visibility.Collapsed;
                }
                else
                {
                    treeViewItem.Visibility = Visibility.Visible;
                }
            }

            if (modeIsCollapsed)
            {
                foreach (var treeViewItem in treeViewItems)
                {
                    var parent = ItemsControl.ItemsControlFromItemContainer(treeViewItem) as TreeViewItem;
                    if (parent == null)
                    {
                        continue;
                    }

                    if (treeViewItem.Visibility == Visibility.Visible)
                    {
                        parent.Visibility = Visibility.Visible;
                    }
                }
            }

            FindTextContent.Opacity = hasMatchRank > 0 ? 1 : 0.6;

            Dispatcher.Invoke(() =>
            {
                if (selectTreeViewItem != null)
                {
                    selectTreeViewItem.IsSelected = true;
                    selectTreeViewItem.Focus();
                }
                else
                {
                    TreeDatabase.Focus();
                }
            }, DispatcherPriority.Normal);

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
