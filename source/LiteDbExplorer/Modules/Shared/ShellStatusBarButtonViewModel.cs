using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using Caliburn.Micro;
using LiteDbExplorer.Wpf.Framework.Shell;

namespace LiteDbExplorer.Modules.Shared
{
    public class ShellStatusBarButtonViewModel : PropertyChangedBase, IOwnerViewLocator, IStatusBarContent
    {
        private object _content;

        public string ContentId { get; protected set; }

        public int DisplayOrder => 10;

        public object Content
        {
            get => _content;
            set
            {
                _content = value;
                UseContent = true;
            }
        }

        public string Text { get; set; }

        public object Icon { get; set; }

        public object ToolTip { get; set; }

        public double MinWidth { get; set; }

        public ICommand Command { get; set; }

        public object CommandParameter { get; set; }

        public bool UseContent { get; set; }

        public ShellStatusBarButtonViewModel(string contentId)
        {
            ContentId = contentId;
        }

        public UIElement GetOwnView(object context)
        {
            var view = CreateView(context);

            OnViewCreated(view);

            return view;
        }

        public virtual void OnViewCreated(Button view)
        {
            // Ignore
        }

        protected virtual Button CreateView(object context)
        {
            var button = new Button
            {
                HorizontalContentAlignment = HorizontalAlignment.Stretch
            };

            if (UseContent)
            {
                button.SetBinding(ContentControl.ContentProperty, new Binding
                {
                    Path = new PropertyPath(nameof(Content)),
                    Mode = BindingMode.OneWay,
                    Source = this
                });
            }
            else
            {
                var buttonIcon = new ContentPresenter
                {
                    VerticalAlignment = VerticalAlignment.Center
                };

                buttonIcon.SetBinding(ContentPresenter.ContentProperty, new Binding
                {
                    Path = new PropertyPath(nameof(Icon)),
                    Mode = BindingMode.OneWay,
                    Source = this
                });

                Grid.SetColumn(buttonIcon, 0);

                var buttonText = new TextBlock
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(5,0,5,0)
                };

                buttonText.SetBinding(TextBlock.TextProperty, new Binding
                {
                    Path = new PropertyPath(nameof(Text)),
                    Mode = BindingMode.OneWay,
                    Source = this
                });

                Grid.SetColumn(buttonText, 1);

                button.Content = new Grid
                {
                    ColumnDefinitions =
                    {
                        new ColumnDefinition
                        {
                            Width = GridLength.Auto
                        },
                        new ColumnDefinition
                        {
                            Width = new GridLength(0, GridUnitType.Star)
                        }
                    },
                    Children =
                    {
                        buttonIcon,
                        buttonText
                    }
                };
            }

            button.SetBinding(FrameworkElement.ToolTipProperty, new Binding
            {
                Path = new PropertyPath(nameof(ToolTip)),
                Mode = BindingMode.OneWay,
                Source = this
            });

            button.SetBinding(ButtonBase.CommandProperty, new Binding
            {
                Path = new PropertyPath(nameof(Command)),
                Mode = BindingMode.OneWay,
                Source = this
            });

            button.SetBinding(ButtonBase.CommandParameterProperty, new Binding
            {
                Path = new PropertyPath(nameof(CommandParameter)),
                Mode = BindingMode.OneWay,
                Source = this
            });

            button.SetBinding(FrameworkElement.MinWidthProperty, new Binding
            {
                Path = new PropertyPath(nameof(MinWidth)),
                Mode = BindingMode.OneWay,
                Source = this
            });

            return button;
        }
    }
}