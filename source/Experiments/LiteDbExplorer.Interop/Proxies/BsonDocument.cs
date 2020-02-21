extern alias v4;
extern alias v5;
using LiteDBv4 = v4::LiteDB;
using LiteDBv5 = v5::LiteDB;

namespace LiteDbExplorer.Interop
{
    

    public interface IBsonDocumentProxy : IBsonValueProxy// , IDictionary<string, IBsonValueProxy>
    {
        // IEnumerable<IBsonValueProxy> Get(string path, bool includeNullIfEmpty);
        // bool Set(string path, LiteDBv5.BsonExpression expr);
        // bool Set(string path, LiteDBv5.BsonValue value);
        // bool Set(string path, LiteDBv5.BsonExpression expr, bool addInArray);
        // bool Set(string path, IBsonValueProxy value, bool addInArray);
        
        // void CopyTo(IBsonDocumentProxy doc);
        // Dictionary<string, IBsonValueProxy> RawValue { get; }
    }

    public class BsonDocumentProxyV4 : BsonValueProxyV4, IBsonDocumentProxy
    {
        private readonly LiteDBv4.BsonDocument _bsonDocument;

        public BsonDocumentProxyV4(LiteDBv4.BsonDocument bsonDocument) : base(bsonDocument)
        {
            _bsonDocument = bsonDocument;
        }
    }

    public class BsonDocumentProxyV5 : BsonValueProxyV5, IBsonDocumentProxy
    {
        private readonly LiteDBv5.BsonDocument _bsonDocument;

        public BsonDocumentProxyV5(LiteDBv5.BsonDocument bsonDocument) : base(bsonDocument)
        {
            _bsonDocument = bsonDocument;
        }
    }

}