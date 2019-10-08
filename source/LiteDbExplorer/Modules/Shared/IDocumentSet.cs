using System;
using System.Threading.Tasks;
using Caliburn.Micro;
using LiteDbExplorer.Wpf.Framework;
using LiteDbExplorer.Wpf.Framework.Shell;

namespace LiteDbExplorer.Modules.Shared
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
        Task OpenDocument<TDocument>() where TDocument : IDocument;

        Task<TDocument> OpenDocument<TDocument, TReferenceId>(TDocument document, TReferenceId initPayload) where TDocument : IDocument<TReferenceId> where TReferenceId : IReferenceId;

        Task<TDocument> OpenDocument<TDocument, TReferenceId>(TReferenceId initPayload) where TDocument : IDocument<TReferenceId> where TReferenceId : IReferenceId;
        
        event EventHandler ActiveDocumentChanging;
        event EventHandler ActiveDocumentChanged;
        event EventHandler<DocumentDeactivateEventArgs> DocumentDeactivated;
    }

    public class DocumentDeactivateEventArgs : EventArgs
    {
        public DocumentDeactivateEventArgs(IDocument item, bool close)
        {
            Item = item;
            Close = close;
        }

        public IDocument Item { get; }
        public bool Close { get; }
    }
}