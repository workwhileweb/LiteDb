using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LiteDbExplorer.Presentation
{
    public static class VisualTreeExtensions
    {
        public static TreeViewItem VisualUpwardSearch(this DependencyObject source)
        {
            while (source != null && !(source is TreeViewItem))
            {
                source = VisualTreeHelper.GetParent(source);
            }

            return source as TreeViewItem;
        }

        public static TreeViewItem ContainerFromItemRecursive(this ItemContainerGenerator root, object item)
        {
            if (root.ContainerFromItem(item) is TreeViewItem treeViewItem)
            {
                return treeViewItem;
            }

            foreach (var subItem in root.Items)
            {
                treeViewItem = root.ContainerFromItem(subItem) as TreeViewItem;
                var search = treeViewItem?.ItemContainerGenerator.ContainerFromItemRecursive(item);
                if (search != null)
                {
                    return search;
                }
            }
            return null;
        }

        public static TreeViewItem FirstChildrenContainerFromItem(this TreeViewItem parent)
        {
            if (parent != null && parent.Items.Count > 0)
            {
                return parent.ItemContainerGenerator.ContainerFromItemRecursive(parent.Items[0]);
            }

            return null;
        }

        public static T FindChildByType<T>(this DependencyObject depObj) 
            where T : DependencyObject
        {
            if (depObj == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);

                var result = (child as T) ?? FindChildByType<T>(child);
                if (result != null) return result;
            }
            return null;
        }
    }
}