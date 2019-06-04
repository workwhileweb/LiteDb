using System;
using System.Linq;
using System.Windows.Controls;
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
    }
}