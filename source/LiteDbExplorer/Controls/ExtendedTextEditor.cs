using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using System.Xml;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using LiteDbExplorer.Controls.Editor;
using LiteDbExplorer.Presentation;

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
            
            ThemeManager.CurrentThemeChanged += (sender, args) => { SetTheme(); };
        }

        public static readonly DependencyProperty SyntaxHighlightingSrcProperty = DependencyProperty.Register(
            nameof(SyntaxHighlightingSrc), 
            typeof(string), 
            typeof(ExtendedTextEditor), 
            new PropertyMetadata(default(string), OnSyntaxHighlightingSrcChanged));

        public string SyntaxHighlightingSrc
        {
            get => (string) GetValue(SyntaxHighlightingSrcProperty);
            set => SetValue(SyntaxHighlightingSrcProperty, value);
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
            if (string.IsNullOrWhiteSpace(SyntaxHighlightingSrc))
            {
                SyntaxHighlighting = null;
                return;
            }

            IHighlightingDefinition highlightingDefinition = null;
            if (App.Settings.ColorTheme == ColorTheme.Dark)
            {
                var darkResourceName = SyntaxHighlightingSrc.Replace(@".xshd", @".dark.xshd");
                highlightingDefinition = LoadHighlightingFromAssembly(darkResourceName);
            }

            SyntaxHighlighting = highlightingDefinition ?? LoadHighlightingFromAssembly(SyntaxHighlightingSrc);
        }

        private static IHighlightingDefinition LoadHighlightingFromAssembly(string name)
        {
            using (var s = _assembly.GetManifestResourceStream(name))
            {
                if (s != null)
                {
                    using (var reader = new XmlTextReader(s))
                    {
                        return HighlightingLoader.Load(reader, HighlightingManager.Instance);
                    }
                }
            }

            return null;
        }
    }
}