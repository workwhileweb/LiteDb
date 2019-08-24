using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LiteDbExplorer.Wpf.Behaviors
{
    public class MouseDoubleClick
    {
        public static DependencyProperty CommandProperty =
            DependencyProperty.RegisterAttached("Command",
                typeof(object),
                typeof(MouseDoubleClick),
                new UIPropertyMetadata(OnCommandChanged));

        public static DependencyProperty CommandParameterProperty =
            DependencyProperty.RegisterAttached("CommandParameter",
                typeof(object),
                typeof(MouseDoubleClick),
                new UIPropertyMetadata(null));

        public static void SetCommand(DependencyObject target, object value)
        {
            target.SetValue(CommandProperty, value);
        }

        public static object GetCommand(UIElement inUIElement)
        {
            return inUIElement.GetValue(CommandProperty) as ICommand;
        }

        public static void SetCommandParameter(DependencyObject target, object value)
        {
            target.SetValue(CommandParameterProperty, value);
        }
        public static object GetCommandParameter(DependencyObject target)
        {
            return target.GetValue(CommandParameterProperty);
        }

        private static void OnCommandChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            if (target is Control control)
            {
                if ((e.NewValue != null) && (e.OldValue == null))
                {
                    control.MouseDoubleClick += OnMouseDoubleClick;
                }
                else if ((e.NewValue == null) && (e.OldValue != null))
                {
                    control.MouseDoubleClick -= OnMouseDoubleClick;
                }
            }
        }

        private static void OnMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            if (sender is Control control && control.GetValue(CommandProperty) is ICommand command)
            {
                var commandParameter = control.GetValue(CommandParameterProperty);
                command.Execute(commandParameter);
                e.Handled = true;
            }
        }
    }
}