using System.Collections.Generic;
using System.IO;
using System.Linq;
using LiteDB;

namespace LiteDbExplorer.Core
{
    public class DocumentReferenceAggregator
    {
        private readonly IEnumerable<DocumentReference> _documents;
        private BsonArray _value;

        public DocumentReferenceAggregator(IEnumerable<DocumentReference> documents)
        {
            _documents = documents;
        }

        public BsonArray Value
        {
            get
            {
                if (_value == null)
                {
                    _value = new BsonArray(_documents.Select(a => a.LiteDocument));
                }
                return _value;
            }
        }

        public string Serialize(bool pretty = false, bool writeBinary = true)
        {
            return JsonSerializer.Serialize(Value, pretty, writeBinary);
        }

        public void Serialize(TextWriter writer, bool pretty = false, bool writeBinary = true)
        {
            JsonSerializer.Serialize(Value, writer, pretty, writeBinary);
        }
    }
}