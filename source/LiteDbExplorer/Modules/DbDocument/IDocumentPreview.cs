using Caliburn.Micro;
using LiteDbExplorer.Core;

namespace LiteDbExplorer.Modules.DbDocument
{
    public interface IDocumentPreview : IScreen
    {
        void SetActiveDocument(DocumentReference document);
        bool HasDocument { get; }
    }
}