extern alias v4;
extern alias v5;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LiteDBv4 = v4::LiteDB;
using LiteDBv5 = v5::LiteDB;

namespace LiteDbExplorer.Interop
{
    public interface IBsonArray : IBsonValue, IList<IBsonValue>
    {
        void AddRange(IEnumerable<IBsonValue> items);
    }

    public class BsonArrayV4Adapter : LiteDBv4.BsonArray, IBsonArray
    {
        public BsonArrayV4Adapter(LiteDBv4.BsonArray bsonArray)
        {
        }
        
        public int CompareTo(IBsonValue other)
        {
            return base.CompareTo((LiteDBv4.BsonValue) other);
        }

        public bool Equals(IBsonValue other)
        {
            return base.Equals(other);
        }

        public int CompareTo(IBsonValue other, object collation)
        {
            return base.CompareTo((LiteDBv4.BsonValue) other);
        }

        public new BaseBsonType Type => BsonValueV4Adapter.BsonTypeToBaseMap[base.Type];
        
        public IBsonArray AsArray { get; }
        
        public IBsonDocument AsDocument { get; }

        public IObjectId AsObjectId { get; }

        public IBsonValue this[string name]
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public IBsonValue this[int index]
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public int IndexOf(IBsonValue item)
        {
            return base.IndexOf((LiteDBv4.BsonValue) item);
        }

        public void Insert(int index, IBsonValue item)
        {
            base.Insert(index, (LiteDBv4.BsonValue)item);
        }

        
        public void AddRange(IEnumerable<IBsonValue> items)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));

            foreach (var item in items.Cast<LiteDBv4.BsonValue>())
            {
                base.Add(item ?? LiteDBv4.BsonValue.Null);
            }
        }

        public void Add(IBsonValue item)
        {
            base.Add((LiteDBv4.BsonValue) item);
        }

        public bool Contains(IBsonValue item)
        {
            return base.Contains((LiteDBv4.BsonValue) item);
        }

        public void CopyTo(IBsonValue[] array, int arrayIndex)
        {
            base.CopyTo((LiteDBv4.BsonValue[]) array, arrayIndex);
        }

        public bool Remove(IBsonValue item)
        {
            return base.Remove((LiteDBv4.BsonValue) item);
        }

        public IEnumerator<IBsonValue> GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }

    public class BsonArrayV5Adapter : LiteDBv5.BsonArray
    {
        
    }
}