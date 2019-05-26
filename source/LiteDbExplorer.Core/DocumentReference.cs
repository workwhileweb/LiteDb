using System;
using System.IO;
using LiteDB;
using LiteDbExplorer.Core;
using LiteDbExplorer.Extensions;

namespace LiteDbExplorer
{
    public enum DocumentTypeFilter
    {
        All = -1,
        BsonDocument = 0,
        File = 1
    }

    public enum DocumentType
    {
        BsonDocument = 0,
        File = 1
    }

    public class DocumentReference : ReferenceNode<DocumentReference>, IDisposable, IJsonSerializerProvider
    {
        private BsonDocument _liteDocument;
        private CollectionReference _collection;

        public DocumentReference()
        {
        }

        public DocumentReference(BsonDocument document, CollectionReference collection) : this()
        {
            LiteDocument = document;
            Collection = collection;
        }

        public BsonDocument LiteDocument
        {
            get => _liteDocument;
            set
            {
                OnPropertyChanging();
                _liteDocument = value;
                OnPropertyChanged();
            }
        }

        public CollectionReference Collection
        {
            get => _collection;
            set
            {
                OnPropertyChanging();
                _collection = value;
                OnPropertyChanged();
            }
        }
        
        public bool ContainsReference(CollectionReference collectionReference)
        {
            if (Collection == null)
            {
                return false;
            }

            return Collection.InstanceId.Equals(collectionReference?.InstanceId);
        }

        public void RemoveSelf()
        {
            Collection?.RemoveItem(this);
        }

        public void Dispose()
        {
            LiteDocument = null;
            Collection = null;
        }

        public string Serialize(bool pretty = false, bool decoded = true)
        {
            return decoded ? LiteDocument.SerializeDecoded(true) : JsonSerializer.Serialize(LiteDocument, pretty, false);
        }

        public void Serialize(TextWriter writer, bool pretty = false)
        {
            JsonSerializer.Serialize(LiteDocument, writer, pretty, false);
        }
    }
}