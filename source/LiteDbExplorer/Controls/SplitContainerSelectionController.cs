using System.Windows.Controls;
using LiteDbExplorer.Wpf.Controls;

namespace LiteDbExplorer.Controls
{
    public class SplitContainerSelectionController
    {
        private bool _ignoreSelectionChanged;

        public SplitContainerSelectionController(SplitContainer splitContainer, ListBox splitOrientationSelector)
        {
            splitOrientationSelector.SelectionChanged += (sender, args) =>
            {
                if (_ignoreSelectionChanged)
                {
                    return;
                }

                splitContainer.Orientation = splitOrientationSelector.SelectedIndex == 0
                    ? Orientation.Vertical
                    : Orientation.Horizontal;
            };

            splitContainer.OrientationChanged += (sender, args) =>
            {
                if (args.Orientation == SplitOrientation.Auto && sender is SplitContainer container)
                {
                    _ignoreSelectionChanged = true;
                    splitOrientationSelector.SelectedIndex =
                        container.CurrentOrientation == Orientation.Vertical ? 0 : 1;
                    _ignoreSelectionChanged = false;
                }
            };
        }

        public static void Attach(SplitContainer splitContainer, ListBox splitOrientationSelector)
        {
            var ignore = new SplitContainerSelectionController(splitContainer, splitOrientationSelector);
        }
    }
}