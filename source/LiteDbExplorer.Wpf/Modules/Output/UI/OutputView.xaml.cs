using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using LiteDbExplorer.Wpf.Modules.AvalonEdit;
using Serilog;

namespace LiteDbExplorer.Wpf.Modules.Output.UI
{
    /// <summary>
    /// Interaction logic for OutputView.xaml
    /// </summary>
    public partial class OutputView : UserControl, IOutputView
    {
        public OutputView()
        {
            InitializeComponent();

            outputText.SyntaxHighlighting = HighlightingProvider.LoadDefaultHighlighting("Log.xshd", false);

            ToggleWordWrap();

            toggleWordWrap.Click += (sender, args) => ToggleWordWrap();
        }

        private void ToggleWordWrap()
        {
            var isChecked = toggleWordWrap.IsChecked ?? false;
            outputText.HorizontalScrollBarVisibility = isChecked ? ScrollBarVisibility.Disabled : ScrollBarVisibility.Auto;
            outputText.WordWrap = isChecked;
        }

        public void ScrollToEnd()
        {
            outputText.ScrollToLine(outputText.LineCount);
        }

        public void Clear()
        {
            outputText.Clear();
        }

        public void AppendText(string text)
        {
            outputText.AppendText(text);
            ScrollToEnd();
        }

        public void SetText(string text)
        {
            outputText.Document.Text = text;

            ScrollToEnd();
        }
    }
}
