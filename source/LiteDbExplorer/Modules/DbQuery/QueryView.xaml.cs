using System.Linq;
using System.Windows.Controls;
using LiteDbExplorer.Framework;

namespace LiteDbExplorer.Modules.DbQuery
{
    /// <summary>
    /// Interaction logic for QueryView.xaml
    /// </summary>
    public partial class QueryView : UserControl, IQueryEditorView
    {
        public QueryView()
        {
            InitializeComponent();
        }

        public void SelectEnd(bool focus = true)
        {
            queryEditor.Select(queryEditor.Text.Length, 0);

            if (focus)
            {
                queryEditor.Focus();
            }
        }

        public void SelectStart(bool focus = true)
        {
            queryEditor.Select(0, 0);

            if (focus)
            {
                queryEditor.Focus();
            }
        }

        public void SelectAll(bool focus = true)
        {
            queryEditor.Select(0, queryEditor.Text.Length);

            if (focus)
            {
                queryEditor.Focus();
            }
        }

        public void SetDocumentText(string content)
        {
            queryEditor.Document.Text = content;
        }

        public void InsetDocumentText(string content, DocumentInsetMode documentInsetMode)
        {
            var originalContentLength = content.Length;

            switch (documentInsetMode)
            {
                case DocumentInsetMode.Start:
                    if (queryEditor.Document.GetLineByNumber(1).Length > 0)
                    {
                        content += "\n";
                    }
                    queryEditor.Document.Insert(0, content);
                    queryEditor.Select(0, originalContentLength);
                    break;
                case DocumentInsetMode.End:
                    if (queryEditor.Document.GetLineByNumber(queryEditor.Document.LineCount).Length > 0)
                    {
                        content = "\n" + content;
                    }
                    queryEditor.Document.Insert(queryEditor.Text.Length, content);
                    queryEditor.Select(queryEditor.Text.Length - originalContentLength, originalContentLength);
                    break;
                default:
                    queryEditor.Document.Text = content;
                    queryEditor.Select(0, queryEditor.Text.Length);
                    break;
            }

            queryEditor.Focus();
        }

        public void InsetDocumentText(string content)
        {
            var command = new RelayCommand<TextInsertAction>(action =>
            {
                InsetDocumentText(action.Text, action.Mode);
            });

            var insertContextMenu = new ContextMenu
            {
                MinWidth = 160,
                Items =
                {
                    new MenuItem
                    {
                        Header = "Replace All",
                        Command = command,
                        CommandParameter = new TextInsertAction(DocumentInsetMode.Replace, content)
                    },
                    new MenuItem
                    {
                        Header = "Insert in Start",
                        Command = command,
                        CommandParameter = new TextInsertAction(DocumentInsetMode.Start, content)
                    },
                    new MenuItem
                    {
                        Header = "Insert in End",
                        Command = command,
                        CommandParameter = new TextInsertAction(DocumentInsetMode.End, content)
                    }
                },
                // PlacementTarget = queryEditor.TextArea,
                // Placement = PlacementMode.Right,
                // HorizontalOffset = -(queryEditor.TextArea.ActualWidth - 16),
                IsOpen = true
            };

            insertContextMenu.Focus();
            insertContextMenu.Items.OfType<MenuItem>().FirstOrDefault()?.Focus();
        }

        public class TextInsertAction
        {
            public TextInsertAction(DocumentInsetMode mode, string text)
            {
                Mode = mode;
                Text = text;
            }

            public DocumentInsetMode Mode { get; set; }
            public string Text { get; set; }
        }
    }

    public enum DocumentInsetMode
    {
        Replace,
        Start,
        End
    }
}