using System.Windows.Controls;
using Markdig;
using Markdig.Wpf;

namespace LiteDbExplorer.Modules.Help
{
    /// <summary>
    /// Interaction logic for IssueHelperView.xaml
    /// </summary>
    public partial class IssueHelperView : UserControl
    {
        public IssueHelperView()
        {
            InitializeComponent();

            markdownViewer.Pipeline = new MarkdownPipelineBuilder()
                .UseSupportedExtensions()
                .UseSoftlineBreakAsHardlineBreak()
                .Build();
        }
    }
}
