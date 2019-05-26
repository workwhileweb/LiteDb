using System.Windows.Controls;
using System.Windows.Navigation;
using Markdig;
using Markdig.Wpf;

namespace LiteDbExplorer.Modules.Help
{
    /// <summary>
    /// Interaction logic for MarkdownDocView.xaml
    /// </summary>
    public partial class MarkdownDocView : UserControl
    {
        public MarkdownDocView()
        {
            InitializeComponent();

            markdownViewer.Pipeline = new MarkdownPipelineBuilder()
                .UseSupportedExtensions()
                .UseSoftlineBreakAsHardlineBreak()
                .Build();
        }

        private void OpenHyperlink(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var viewModel = DataContext as MarkdownDocViewModel;
            viewModel?.OpenHyperlink(e.Parameter.ToString());
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            var viewModel = DataContext as MarkdownDocViewModel;
            viewModel?.OpenHyperlink(e.Uri.AbsoluteUri);
            e.Handled = true;
        }
    }
}
