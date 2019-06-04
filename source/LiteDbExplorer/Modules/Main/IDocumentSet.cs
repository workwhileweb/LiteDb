using System;
using System.Threading.Tasks;
using Caliburn.Micro;
using LiteDbExplorer.Framework;

namespace LiteDbExplorer.Modules.Main
{
    public interface IDocumentSet : IScreen, IHaveActiveItem
    {
        Guid Id { get; }
        string ContentId { get; }
        IObservableCollection<IDocument> Documents { get; }
        void OpenDocument(IDocument model);
        void CloseDocument(IDocument document);
        void ActivateItem(IDocument item);
        void DeactivateItem(IDocument item, bool close);
        Task OpenDocument<TDoc>() where TDoc : IDocument;

        Task<TDoc> OpenDocument<TDoc, TNode>(TDoc model, TNode init)
            where TDoc : IDocument<TNode> where TNode : IReferenceNode;
        Task<TDoc> OpenDocument<TDoc, TNode>(TNode init) where TDoc : IDocument<TNode> where TNode : IReferenceNode;
        
        event EventHandler ActiveDocumentChanging;
        event EventHandler ActiveDocumentChanged;
    }
}