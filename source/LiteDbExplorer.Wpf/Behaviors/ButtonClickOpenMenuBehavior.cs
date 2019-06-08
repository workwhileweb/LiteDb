using System.Windows;
using System.Windows.Controls.Primitives;
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
                button.ContextMenu.Placement = PlacementMode ?? System.Windows.Controls.Primitives.PlacementMode.Bottom;
                button.ContextMenu.IsOpen = true;
            }
        }
    }
}