using System.IO;
using System.Runtime.CompilerServices;

namespace LiteDbExplorer.Core
{
    extern alias v4;
    using LiteDBv4 = v4::LiteDB;

    public sealed class DocumentReference : ReferenceNode<DocumentReference>, IJsonSerializerProvider
    {
        public DocumentReference()
        {
        }

        public DocumentReference(LiteDBv4.BsonDocument document, CollectionReference collection) : this()
        {
            LiteDocument = document;
            Collection = collection;
        }

        public LiteDBv4.BsonDocument LiteDocument { get; set; }

        public CollectionReference Collection { get; set; }

        [IndexerName(@"Item")]
        public LiteDBv4.BsonValue this[string name]
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
            return decoded ? LiteDocument.SerializeDecoded(true) : LiteDBv4.JsonSerializer.Serialize(LiteDocument, pretty, false);
        }

        public void Serialize(TextWriter writer, bool pretty = false)
        {
            LiteDBv4.JsonSerializer.Serialize(LiteDocument, writer, pretty, false);
        }
    }
}