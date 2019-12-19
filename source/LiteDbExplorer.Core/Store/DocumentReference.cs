using System.IO;
using System.Runtime.CompilerServices;
using LiteDB;

namespace LiteDbExplorer.Core
{
    public sealed class DocumentReference : ReferenceNode<DocumentReference>, IJsonSerializerProvider
    {
        public DocumentReference()
        {
        }

        public DocumentReference(BsonDocument document, CollectionReference collection) : this()
        {
            LiteDocument = document;
            Collection = collection;
        }

        public BsonDocument LiteDocument { get; set; }

        public CollectionReference Collection { get; set; }

        [IndexerName(@"Item")]
        public BsonValue this[string name]
        {
            get => LiteDocument[name];
            set => LiteDocument[name] = value;
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

        public void NotifyDocumentChanged()
        {
            OnPropertyChanged(nameof(LiteDocument));
            OnPropertyChanged(@"Item[]");
        }

        protected override void Dispose(bool disposing)
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