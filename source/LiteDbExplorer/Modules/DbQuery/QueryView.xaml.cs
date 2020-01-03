using System.Windows.Controls;
using LiteDB;
using LiteDbExplorer.Controls.Editor;
using LiteDbExplorer.Core;
using Serilog;


namespace LiteDbExplorer.Modules.DbQuery
{
    /// <summary>
    /// Interaction logic for QueryView.xaml
    /// </summary>
    public partial class QueryView : UserControl, IQueryView
    {
        private readonly TextEditorInteraction _textEditorInteraction;
        private readonly ShellCommandCompletion _shellCommandCompletion;

        public QueryView()
        {
            InitializeComponent();

            _textEditorInteraction = new TextEditorInteraction(queryEditor);

            if (Properties.Settings.Default.QueryEditor_EnableShellCommandAutocomplete)
            {
                _shellCommandCompletion = new ShellCommandCompletion(queryEditor);
            }
        }


        public void SelectEnd(bool focus = true)
        {
            _textEditorInteraction.SelectEnd(focus);
        }

        public void SelectStart(bool focus = true)
        {
            _textEditorInteraction.SelectStart(focus);
        }

        public void SelectAll(bool focus = true)
        {
            _textEditorInteraction.SelectAll(focus);
        }

        public void SetDocumentText(string content)
        {
            _textEditorInteraction.SetDocumentText(content);
        }

        public void InsetDocumentText(string content, DocumentInsetMode documentInsetMode)
        {
            _textEditorInteraction.InsetDocumentText(content, documentInsetMode);
        }

        public void InsetDocumentText(string content)
        {
            _textEditorInteraction.InsetDocumentText(content);
        }

        public void UpdateCodeCompletion(DatabaseReference databaseReference)
        {
            _shellCommandCompletion?.UpdateCodeCompletion(databaseReference);
        }
    }
}