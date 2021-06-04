using LiteDbExplorer.Controls.Editor;
using LiteDbExplorer.Core;

namespace LiteDbExplorer.Modules.DbQuery
{
    public interface IQueryView : ITextEditorInteraction
    {
        void UpdateCodeCompletion(DatabaseReference databaseReference);
    }
}