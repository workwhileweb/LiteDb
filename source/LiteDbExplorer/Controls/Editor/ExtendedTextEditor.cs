using System;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using ICSharpCode.AvalonEdit;
using LiteDbExplorer.Controls.Editor;
using LiteDbExplorer.Presentation;
using LiteDbExplorer.Wpf.Modules.AvalonEdit;

namespace LiteDbExplorer.Controls
{
    public class ExtendedTextEditor : TextEditor
    {
        private readonly SearchReplacePanel _searchReplacePanel;

        private static readonly Assembly _assembly = typeof(ExtendedTextEditor).Assembly;

        public ExtendedTextEditor()
        {
            _searchReplacePanel = SearchReplacePanel.Install(this);

            SetTheme();

            Loaded += (sender, args) => ThemeManager.CurrentThemeChanged += ThemeManagerOnCurrentThemeChanged;
            Unloaded += (sender, args) => ThemeManager.CurrentThemeChanged -= ThemeManagerOnCurrentThemeChanged;
        }

        private void ThemeManagerOnCurrentThemeChanged(object sender, EventArgs e)
        {
            SetTheme();
        }

        public static readonly DependencyProperty SyntaxHighlightingNameProperty = DependencyProperty.Register(
            nameof(SyntaxHighlightingName), 
            typeof(string), 
            typeof(ExtendedTextEditor), 
            new PropertyMetadata(default(string), OnSyntaxHighlightingSrcChanged));

        public string SyntaxHighlightingName
        {
            get => (string) GetValue(SyntaxHighlightingNameProperty);
            set => SetValue(SyntaxHighlightingNameProperty, value);
        }

        private static void OnSyntaxHighlightingSrcChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ExtendedTextEditor editor)
            {
                editor.SetSyntaxHighlighting();
            }
        }

        private void SetTheme()
        {
            if (App.Settings.ColorTheme == ColorTheme.Dark)
            {
                TextArea.Foreground = new SolidColorBrush(Colors.White);
                
                _searchReplacePanel.MarkerBrush = new SolidColorBrush(Color.FromArgb(63, 144, 238, 144));
                TextArea.TextView.LinkTextForegroundBrush = new SolidColorBrush(Color.FromRgb(206, 145, 120));
            }
            else
            {
                _searchReplacePanel.MarkerBrush = new SolidColorBrush(Color.FromArgb(153, 144, 238, 144));
                TextArea.TextView.LinkTextForegroundBrush = new SolidColorBrush(Color.FromRgb(26, 13, 171));
                TextArea.Foreground = new SolidColorBrush(Colors.Black);
            }

            SetSyntaxHighlighting();
        }

        private void SetSyntaxHighlighting()
        {
            if (string.IsNullOrWhiteSpace(SyntaxHighlightingName))
            {
                SyntaxHighlighting = null;
                return;
            }

            string theme = null;
            if (App.Settings.ColorTheme == ColorTheme.Dark)
            {
                theme = "dark";
            }

            SyntaxHighlighting = LocalHighlightingManager.Current.LoadDefinitionFromName(SyntaxHighlightingName, theme);;
        }
    }
}