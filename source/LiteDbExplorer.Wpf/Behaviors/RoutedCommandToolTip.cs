using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using LiteDbExplorer.Wpf.Converters;

namespace LiteDbExplorer.Wpf.Behaviors
{
    public class RoutedCommandToolTip
    {
        public static readonly DependencyProperty ShowToolTipProperty = DependencyProperty.RegisterAttached(
            "ShowToolTip",
            typeof(bool),
            typeof(RoutedCommandToolTip),
            new UIPropertyMetadata(false, ShowToolTip_PropertyChangedCallback)
        );

        public static void SetShowToolTip(UIElement element, bool value)
        {
            element.SetValue(ShowToolTipProperty, value);
        }

        public static bool GetShowToolTip(UIElement element)
        {
            return (bool)element.GetValue(ShowToolTipProperty);
        }

        private static void ShowToolTip_PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ButtonBase buttonBase)
            {
                if (e.NewValue is bool val && val)
                {
                    buttonBase.SetBinding(FrameworkElement.ToolTipProperty, new Binding
                    {
                        Path = new PropertyPath(nameof(ButtonBase.Command)),
                        RelativeSource = RelativeSource.Self,
                        Converter = RoutedCommandToInputGestureTextConverter.Instance
                    });
                
                    ToolTipService.SetShowOnDisabled(buttonBase, true);
                } 
                else
                {
                    BindingOperations.ClearBinding(buttonBase, FrameworkElement.ToolTipProperty);
                    ToolTipService.SetShowOnDisabled(buttonBase, false);
                }
            }
        }

        public static readonly DependencyProperty ShowToolTipForCommandProperty = DependencyProperty.RegisterAttached(
            "ShowToolTipForCommand",
            typeof(ICommand),
            typeof(RoutedCommandToolTip),
            new UIPropertyMetadata(default(ICommand), ShowTooltipForCommand_PropertyChangedCallback)
        );

        public static void SetShowToolTipForCommand(UIElement element, ICommand value)
        {
            element.SetValue(ShowToolTipForCommandProperty, value);
        }

        public static ICommand GetShowToolTipForCommand(UIElement element)
        {
            return (ICommand)element.GetValue(ShowToolTipForCommandProperty);
        }

        private static void ShowTooltipForCommand_PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FrameworkElement frameworkElement)
            {
                if (e.NewValue != null)
                {
                    frameworkElement.SetBinding(FrameworkElement.ToolTipProperty, new Binding
                    {
                        Path = new PropertyPath(RoutedCommandToolTip.ShowToolTipForCommandProperty),
                        RelativeSource = RelativeSource.Self,
                        Converter = RoutedCommandToInputGestureTextConverter.Instance
                    });
                
                    ToolTipService.SetShowOnDisabled(frameworkElement, true);
                } 
                else
                {
                    BindingOperations.ClearBinding(frameworkElement, FrameworkElement.ToolTipProperty);
                    ToolTipService.SetShowOnDisabled(frameworkElement, false);
                }
            }
        }

    }
}