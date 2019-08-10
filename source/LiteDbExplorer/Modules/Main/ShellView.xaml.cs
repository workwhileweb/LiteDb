using System;
using System.Windows;
using System.Windows.Controls;

namespace LiteDbExplorer.Modules.Main
{
    /// <summary>
    /// Interaction logic for ShellView.xaml
    /// </summary>
    public partial class ShellView : UserControl, IOwnerViewModelMessageHandler
    {
        public ShellView()
        {
            InitializeComponent();

            InvalidateLeftContentVisibility();

            ToolPanelsGridSplitter.DragCompleted += (sender, args) =>
            {
                var heightValue = (int) toolPanelRowDefinition.Height.Value;
                var isVisible = heightValue > 80;
                Commands.ShowToolsPanel.Execute(isVisible, Application.Current.MainWindow);
                InvalidateToolsPanelVisibility(heightValue);
            };
        }

        public static readonly DependencyProperty LeftContentIsVisibleProperty = DependencyProperty.Register(
            nameof(LeftContentIsVisible), typeof(bool),
            typeof(ShellView), new FrameworkPropertyMetadata(
                default(bool),
                OnLeftContentIsVisiblePropertyChanged));

        public bool LeftContentIsVisible
        {
            get => (bool) GetValue(LeftContentIsVisibleProperty);
            set => SetValue(LeftContentIsVisibleProperty, value);
        }

        private static void OnLeftContentIsVisiblePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ShellView shellView)
            {
                shellView.InvalidateLeftContentVisibility();
            }
        }

        public static readonly DependencyProperty ToolsPanelIsVisibleProperty = DependencyProperty.Register(
            nameof(ToolsPanelIsVisible), typeof(bool), typeof(ShellView), 
            new PropertyMetadata(default(bool), OnToolsPanelIsVisiblePropertyChanged));

        private static void OnToolsPanelIsVisiblePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ShellView shellView)
            {
                shellView.InvalidateToolsPanelVisibility();
            }
        }

        public bool ToolsPanelIsVisible
        {
            get => (bool) GetValue(ToolsPanelIsVisibleProperty);
            set => SetValue(ToolsPanelIsVisibleProperty, value);
        }

        private void InvalidateLeftContentVisibility()
        {
            void SetGridColumn(int column, int columnsSpan, params UIElement[] uiElements)
            {
                foreach (var uiElement in uiElements)
                {
                    Grid.SetColumn(uiElement, column);
                    Grid.SetColumnSpan(uiElement, columnsSpan);
                }
            }

            if (LeftContentIsVisible)
            {
                LeftContentContainer.Visibility = Visibility.Visible;
                LeftContentGridSplitter.Visibility = Visibility.Visible;

                SetGridColumn(2, 1, MainContentContainer, ToolPanelsGridSplitter, ToolPanelsContent);
            }
            else
            {
                LeftContentContainer.Visibility = Visibility.Collapsed;
                LeftContentGridSplitter.Visibility = Visibility.Collapsed;

                SetGridColumn(0, 3, MainContentContainer, ToolPanelsGridSplitter, ToolPanelsContent);
            }
        }

        private void InvalidateToolsPanelVisibility(double? visibleSize = null)
        {
            if (ToolsPanelIsVisible)
            {
                if (!visibleSize.HasValue)
                {
                    visibleSize = Math.Max(200, ActualHeight / 3.32);
                }
                toolPanelRowDefinition.Height = new GridLength(visibleSize.Value);
            }
            else
            {
                toolPanelRowDefinition.Height = new GridLength(0);
            }
        }

        public void Handle(string message, object payload = null)
        {
            
        }
    }
}
