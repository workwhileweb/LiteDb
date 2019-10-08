using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Interactivity;

namespace LiteDbExplorer.Wpf.Behaviors
{
    public class ButtonClickOpenMenuBehavior : Behavior<ButtonBase>
    {
        public PlacementMode? PlacementMode { get; set; }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Click += OnClick;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.Click -= OnClick;
        }

        private void OnClick(object sender, RoutedEventArgs e)
        {
            if (sender is ButtonBase button && button.ContextMenu != null)
            {
                button.ContextMenu.PlacementTarget = button;

                if (PlacementMode == System.Windows.Controls.Primitives.PlacementMode.Left)
                {
                    button.ContextMenu.HorizontalOffset = button.ActualWidth;
                    button.ContextMenu.VerticalOffset = button.ActualHeight;
                }

                button.ContextMenu.Placement = PlacementMode ?? System.Windows.Controls.Primitives.PlacementMode.Bottom;
                button.ContextMenu.IsOpen = true;
            }
        }
    }
}