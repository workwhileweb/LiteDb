extern alias v4;
extern alias v5;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using LiteDBv4 = v4::LiteDB;
using LiteDBv5 = v5::LiteDB;

namespace LiteDbExplorer.Interop
{
    public enum BaseBsonType : byte
    {
        MinValue,
        Null,
        Int32,
        Int64,
        Double,
        Decimal,
        String,
        Document,
        Array,
        Binary,
        ObjectId,
        Guid,
        Boolean,
        DateTime,
        MaxValue,
    }

    public interface IBsonValue : IComparable<IBsonValue>, IEquatable<IBsonValue>
    {
        string ToString();
        int CompareTo(IBsonValue other, object collation);
        bool Equals(object obj);
        int GetHashCode();
        BaseBsonType Type { get; }
        IBsonArray AsArray { get; }
        IBsonDocument AsDocument { get; }
        byte[] AsBinary { get; }
        bool AsBoolean { get; }
        string AsString { get; }
        int AsInt32 { get; }
        long AsInt64 { get; }
        double AsDouble { get; }
        decimal AsDecimal { get; }
        DateTime AsDateTime { get; }
        IObjectId AsObjectId { get; }
        Guid AsGuid { get; }
        bool IsNull { get; }
        bool IsArray { get; }
        bool IsDocument { get; }
        bool IsInt32 { get; }
        bool IsInt64 { get; }
        bool IsDouble { get; }
        bool IsDecimal { get; }
        bool IsNumber { get; }
        bool IsBinary { get; }
        bool IsBoolean { get; }
        bool IsString { get; }
        bool IsObjectId { get; }
        bool IsGuid { get; }
        bool IsDateTime { get; }
        bool IsMinValue { get; }
        bool IsMaxValue { get; }
        IBsonValue this[string name] { get; set; }
        IBsonValue this[int index] { get; set; }
    }

    public class BsonValueV4Adapter : IBsonValue
    {
        protected readonly LiteDBv4.BsonValue _bsonValue;

        public BsonValueV4Adapter(LiteDBv4.BsonValue bsonValue)
        {
            _bsonValue = bsonValue;
        }

        public int CompareTo(IBsonValue other)
        {
            return _bsonValue.CompareTo(other as LiteDBv4.BsonValue);
        }

        public bool Equals(IBsonValue other)
        {
            return _bsonValue.Equals(other as LiteDBv4.BsonValue);
        }

        public int CompareTo(IBsonValue other, object collation)
        {
            return _bsonValue.CompareTo(other as LiteDBv4.BsonValue);
        }

        public static readonly IReadOnlyDictionary<LiteDBv4.BsonType, BaseBsonType> BsonTypeToBaseMap = new Dictionary<LiteDBv4.BsonType, BaseBsonType>
        {
            { LiteDBv4.BsonType.MinValue, BaseBsonType.MinValue },
            { LiteDBv4.BsonType.Null, BaseBsonType.Null },
            { LiteDBv4.BsonType.Int32, BaseBsonType.Int32 },
            { LiteDBv4.BsonType.Int64, BaseBsonType.Int64 },
            { LiteDBv4.BsonType.Double, BaseBsonType.Double },
            { LiteDBv4.BsonType.Decimal, BaseBsonType.Decimal },
            { LiteDBv4.BsonType.String, BaseBsonType.String },
            { LiteDBv4.BsonType.Document, BaseBsonType.Document },
            { LiteDBv4.BsonType.Array, BaseBsonType.Array },
            { LiteDBv4.BsonType.Binary, BaseBsonType.Binary },
            { LiteDBv4.BsonType.ObjectId, BaseBsonType.ObjectId },
            { LiteDBv4.BsonType.Guid, BaseBsonType.Guid },
            { LiteDBv4.BsonType.Boolean, BaseBsonType.Boolean },
            { LiteDBv4.BsonType.DateTime, BaseBsonType.DateTime },
            { LiteDBv4.BsonType.MaxValue, BaseBsonType.MaxValue },
        };

        public BaseBsonType Type => BsonTypeToBaseMap[_bsonValue.Type];

        public IBsonArray AsArray => new BsonArrayV4Adapter(_bsonValue.AsArray);

        public IBsonDocument AsDocument => new BsonDocumentV4Adapter(_bsonValue.AsDocument);

        public IObjectId AsObjectId => new ObjectIdV4Adapter(_bsonValue.AsObjectId);

        public byte[] AsBinary => _bsonValue.AsBinary;

        public bool AsBoolean => _bsonValue.AsBoolean;

        public string AsString => _bsonValue.AsString;

        public int AsInt32 => _bsonValue.AsInt32;

        public long AsInt64 => _bsonValue.AsInt64;

        public double AsDouble => _bsonValue.AsDouble;

        public decimal AsDecimal => _bsonValue.AsDecimal;

        public DateTime AsDateTime => _bsonValue.AsDateTime;

        public Guid AsGuid => _bsonValue.AsGuid;

        public bool IsNull => _bsonValue.IsNull;

        public bool IsArray => _bsonValue.IsArray;

        public bool IsDocument => _bsonValue.IsDocument;

        public bool IsInt32 => _bsonValue.IsInt32;

        public bool IsInt64 => _bsonValue.IsInt64;

        public bool IsDouble => _bsonValue.IsDouble;

        public bool IsDecimal => _bsonValue.IsDecimal;

        public bool IsNumber => _bsonValue.IsNumber;

        public bool IsBinary => _bsonValue.IsBinary;

        public bool IsBoolean => _bsonValue.IsBoolean;

        public bool IsString => _bsonValue.IsString;

        public bool IsObjectId => _bsonValue.IsObjectId;

        public bool IsGuid => _bsonValue.IsGuid;

        public bool IsDateTime => _bsonValue.IsDateTime;

        public bool IsMinValue => _bsonValue.IsMinValue;

        public bool IsMaxValue => _bsonValue.IsMaxValue;

        public IBsonValue this[string name]
        {
            get => throw new InvalidOperationException("Cannot access non-array type value on " + _bsonValue.RawValue);
            set => throw new InvalidOperationException("Cannot access non-array type value on " + _bsonValue.RawValue);
        }

        public IBsonValue this[int index]
        {
            get => throw new InvalidOperationException("Cannot access non-array type value on " + _bsonValue.RawValue);
            set => throw new InvalidOperationException("Cannot access non-array type value on " + _bsonValue.RawValue);
        }

    }

    public class BsonValueV5Adapter : LiteDBv5.BsonValue
    {
        
    }
}