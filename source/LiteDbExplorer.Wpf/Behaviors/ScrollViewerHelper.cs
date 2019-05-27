using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using LiteDbExplorer.Presentation;

namespace LiteDbExplorer.Wpf.Behaviors
{
    public class ScrollViewerHelper
    {
        public static readonly DependencyProperty UseHorizontalScrollingProperty = DependencyProperty.RegisterAttached(
            "UseHorizontalScrolling", typeof(bool), typeof(ScrollViewerHelper), new PropertyMetadata(default(bool), UseHorizontalScrollingChangedCallback));

        private static void UseHorizontalScrollingChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            ItemsControl itemsControl = dependencyObject as ItemsControl;

            if (itemsControl == null) throw new ArgumentException("Element is not an ItemsControl");

            itemsControl.PreviewMouseWheel += delegate(object sender, MouseWheelEventArgs args)
            {
                ScrollViewer scrollViewer = itemsControl.FindChildByType<ScrollViewer>();

                if (scrollViewer == null) return;

                if (args.Delta < 0)
                {
                    scrollViewer.LineRight();
                }
                else
                {
                    scrollViewer.LineLeft();
                }
            };
        }


        public static void SetUseHorizontalScrolling(ItemsControl element, bool value)
        {
            element.SetValue(UseHorizontalScrollingProperty, value);
        }

        public static bool GetUseHorizontalScrolling(ItemsControl element)
        {
            return (bool)element.GetValue(UseHorizontalScrollingProperty);
        }
    }


}