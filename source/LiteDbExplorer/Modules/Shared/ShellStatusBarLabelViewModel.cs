using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Caliburn.Micro;
using LiteDbExplorer.Wpf.Framework.Shell;

namespace LiteDbExplorer.Modules.Shared
{
    public class ShellStatusBarLabelViewModel : PropertyChangedBase, IOwnerViewLocator, IStatusBarContent
    {
        public ShellStatusBarLabelViewModel(string contentId, int displayOrder = 0)
        {
            ContentId = contentId;
            DisplayOrder = displayOrder;
        }

        public string ContentId { get; }

        public int DisplayOrder { get; }

        public string Text { get; set; }

        public object ToolTip { get; set; }

        public UIElement GetOwnView(object context)
        {
            var view = CreateView(context);

            OnViewCreated(view);

            return view;
        }

        public virtual void OnViewCreated(TextBlock view)
        {
            // Ignore
        }

        protected virtual TextBlock CreateView(object context)
        {
            var label = new TextBlock
            {
                TextWrapping = TextWrapping.NoWrap,
                TextTrimming = TextTrimming.CharacterEllipsis,
                VerticalAlignment = VerticalAlignment.Center,
                Padding = new Thickness(5,0,5,0)
            };

            label.SetBinding(TextBlock.TextProperty, new Binding
            {
                Path = new PropertyPath(nameof(Text)),
                Mode = BindingMode.OneWay,
                Source = this
            });

            label.SetBinding(FrameworkElement.ToolTipProperty, new Binding
            {
                Path = new PropertyPath(nameof(ToolTip)),
                Mode = BindingMode.OneWay,
                Source = this
            });

            return label;
        }
    }
}