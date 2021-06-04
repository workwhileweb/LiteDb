using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace LiteDbExplorer.Presentation
{
    public static class TreeViewExtensions
    {
        public static void ExpandToTreeLevel(this TreeView treeView, int depth)
        {
            foreach (var treeViewItem in treeView.Items.OfType<TreeViewItem>())
            {
                treeViewItem.IsExpanded = true;
            }

            void Recurse() => SetTreeIsExpanded(treeView.ItemContainerGenerator, treeView.Items, depth, true);

            treeView.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, (Action) Recurse);
        }

        public static void CollapseAll(this TreeView treeView)
        {
            SetTreeIsExpanded(treeView.ItemContainerGenerator, treeView.Items, -1, false);
        }

        private static void SetTreeIsExpanded(ItemContainerGenerator itemContainerGenerator, ItemCollection items, int depth, bool isExpanded)
        {
            depth--;
            foreach (var item in items)
            {
                if (!(itemContainerGenerator.ContainerFromItem(item) is TreeViewItem itm))
                {
                    continue;
                }

                itm.IsExpanded = isExpanded;

                if (depth != 0 && itm.Items.Count > 0)
                {
                    SetTreeIsExpanded(itm.ItemContainerGenerator, itm.Items, depth, isExpanded);
                }
            }
        }

        public static List<TreeViewItem> FindTreeViewItems(this Visual @this)
        {
            if (@this == null)
            {
                return null;
            }

            var result = new List<TreeViewItem>();

            var frameworkElement = @this as FrameworkElement;
            frameworkElement?.ApplyTemplate();

            for (int i = 0, count = VisualTreeHelper.GetChildrenCount(@this); i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(@this, i) as Visual;

                if (child is TreeViewItem treeViewItem)
                {
                    result.Add(treeViewItem);
                    if (!treeViewItem.IsExpanded)
                    {
                        treeViewItem.IsExpanded = true;
                        treeViewItem.UpdateLayout();
                    }
                }

                result.AddRange(FindTreeViewItems(child));
            }
            return result;
        }
    }
}