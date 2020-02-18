extern alias v4;
extern alias v5;
using System;
using System.Collections;
using System.Collections.Generic;
using LiteDBv4 = v4::LiteDB;
using LiteDBv5 = v5::LiteDB;

namespace LiteDbExplorer.Interop
{
    

    public interface IBsonDocument : IBsonValue, IDictionary<string, IBsonValue>
    {
        IEnumerable<IBsonValue> Get(string path, bool includeNullIfEmpty);
        // bool Set(string path, LiteDBv5.BsonExpression expr);
        // bool Set(string path, LiteDBv5.BsonValue value);
        // bool Set(string path, LiteDBv5.BsonExpression expr, bool addInArray);
        bool Set(string path, IBsonValue value, bool addInArray);
        
        void CopyTo(IBsonDocument doc);
        Dictionary<string, IBsonValue> RawValue { get; }
    }

    public class BsonDocumentV4Adapter : BsonValueV4Adapter, IBsonDocument
    {
        private readonly LiteDBv4.BsonDocument _bsonDocument;

        public BsonDocumentV4Adapter(LiteDBv4.BsonDocument bsonDocument) : base(bsonDocument)
        {
            _bsonDocument = bsonDocument;
        }


        public IEnumerator<KeyValuePair<string, IBsonValue>> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(KeyValuePair<string, IBsonValue> item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            _bsonDocument.Clear();
        }

        public bool Contains(KeyValuePair<string, IBsonValue> item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(KeyValuePair<string, IBsonValue>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(KeyValuePair<string, IBsonValue> item)
        {
            throw new NotImplementedException();
        }

        public int Count { get; }
        public bool IsReadOnly { get; }
        public bool ContainsKey(string key)
        {
            throw new NotImplementedException();
        }

        public void Add(string key, IBsonValue value)
        {
            throw new NotImplementedException();
        }

        public bool Remove(string key)
        {
            throw new NotImplementedException();
        }

        public bool TryGetValue(string key, out IBsonValue value)
        {
            throw new NotImplementedException();
        }

        public ICollection<string> Keys { get; }
        public ICollection<IBsonValue> Values { get; }
        public IEnumerable<IBsonValue> Get(string path, bool includeNullIfEmpty)
        {
            throw new NotImplementedException();
        }

        public bool Set(string path, IBsonValue value, bool addInArray)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(IBsonDocument doc)
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, IBsonValue> RawValue { get; }
    }

    public class BsonDocumentV5Adapter : LiteDBv5.BsonDocument
    {
    }

    
}