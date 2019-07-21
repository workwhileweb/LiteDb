using System.IO;
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