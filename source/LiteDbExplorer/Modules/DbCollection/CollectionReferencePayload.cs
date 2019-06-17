using System.Collections.Generic;
using LiteDbExplorer.Core;
using LiteDbExplorer.Wpf.Framework;

namespace LiteDbExplorer.Modules.DbCollection
{
    public class CollectionReferencePayload : IReferenceId
    {
        public CollectionReferencePayload(CollectionReference collectionReference, IEnumerable<DocumentReference> selectedDocuments = null)
        {
            InstanceId = collectionReference.InstanceId;
            CollectionReference = collectionReference;
            SelectedDocuments = selectedDocuments;
        }

        public CollectionReferencePayload(CollectionReference collectionReference, DocumentReference selectedDocument) : 
            this(collectionReference,  ToEnumerable(selectedDocument))
        {
        }

        public string InstanceId { get; }
        public CollectionReference CollectionReference { get; }
        public IEnumerable<DocumentReference> SelectedDocuments { get; }

        private static IEnumerable<DocumentReference> ToEnumerable(DocumentReference selectedDocument)
        {
            return selectedDocument != null ? new[] {selectedDocument} : null;
        }
    }
}