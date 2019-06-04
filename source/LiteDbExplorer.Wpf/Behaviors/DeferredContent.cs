using System.Windows;

namespace LiteDbExplorer.Wpf.Behaviors
{
    public class DeferredContent
    {
        public static readonly DependencyProperty ContentProperty =
            DependencyProperty.RegisterAttached(
                "Content",
                typeof(object),
                typeof(DeferredContent),
                new PropertyMetadata());

        public static object GetContent(DependencyObject obj)
        {
            return obj.GetValue(ContentProperty);
        }

        public static void SetContent(DependencyObject obj, object value)
        {
            obj.SetValue(ContentProperty, value);
        }
    }
}