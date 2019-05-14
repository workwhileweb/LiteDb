using System.Collections.Generic;

namespace LiteDbExplorer.Core.Events
{
    public class DocumentChangeEventArgs : ReferenceNodeChangeEventArgs<DocumentReference>
    {
        public static DocumentChangeEventArgs Nome = new DocumentChangeEventArgs();

        private DocumentChangeEventArgs()
        {
        }

        public DocumentChangeEventArgs(ReferenceNodeChangeAction action, IReadOnlyCollection<DocumentReference> items) : base(action, items)
        {
        }

        public DocumentChangeEventArgs(ReferenceNodeChangeAction action, DocumentReference item) : base(action, item)
        {
        }
    }

    public class CollectionDocumentChangeEventArgs : ReferenceNodeChangeEventArgs<DocumentReference>
    {
        public static CollectionDocumentChangeEventArgs Nome = new CollectionDocumentChangeEventArgs();

        private CollectionDocumentChangeEventArgs()
        {
        }

        public CollectionDocumentChangeEventArgs(ReferenceNodeChangeAction action, IReadOnlyCollection<DocumentReference> items, CollectionReference collectionReference) : base(action, items)
        {
            CollectionReference = collectionReference;
        }

        public CollectionDocumentChangeEventArgs(ReferenceNodeChangeAction action, DocumentReference item, CollectionReference collectionReference) : base(action, item)
        {
            CollectionReference = collectionReference ?? item?.Collection;
        }

        public CollectionReference CollectionReference { get; }
    }

    public class CollectionChangeEventArgs : ReferenceNodeChangeEventArgs<CollectionReference>
    {
        public static CollectionChangeEventArgs Nome = new CollectionChangeEventArgs();

        private CollectionChangeEventArgs()
        {
        }

        public CollectionChangeEventArgs(ReferenceNodeChangeAction action, IReadOnlyCollection<CollectionReference> items) : base(action, items)
        {
        }

        public CollectionChangeEventArgs(ReferenceNodeChangeAction action, CollectionReference item) : base(action, item)
        {
        }
    }

    public class DatabaseChangeEventArgs : ReferenceNodeChangeEventArgs<DatabaseReference>
    {
        public static DatabaseChangeEventArgs Nome = new DatabaseChangeEventArgs();

        private DatabaseChangeEventArgs()
        {
        }

        public DatabaseChangeEventArgs(ReferenceNodeChangeAction action, IReadOnlyCollection<DatabaseReference> items) : base(action, items)
        {
        }

        public DatabaseChangeEventArgs(ReferenceNodeChangeAction action, DatabaseReference item) : base(action, item)
        {
        }
    }
}