namespace LiteDbExplorer.Modules.DbQuery
{
    public interface IQueryEditorView
    {
        void SelectEnd(bool focus = true);
        void SelectStart(bool focus = true);
        void SelectAll(bool focus = true);
        void SetDocumentText(string content);
        void InsetDocumentText(string content, DocumentInsetMode documentInsetMode);
        void InsetDocumentText(string content);
    }
}