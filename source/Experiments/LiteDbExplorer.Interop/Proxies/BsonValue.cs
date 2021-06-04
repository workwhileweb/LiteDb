extern alias v4;
extern alias v5;
using System;
using System.Collections.Generic;
using LiteDBv4 = v4::LiteDB;
using LiteDBv5 = v5::LiteDB;

namespace LiteDbExplorer.Interop
{
    public enum ProxyBsonType : byte
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

    public interface IBsonValueProxy : IComparable<IBsonValueProxy>, IEquatable<IBsonValueProxy>
    {
        int CompareTo(IBsonValueProxy other, object collation);
        ProxyBsonType Type { get; }
        IBsonArrayProxy AsArray { get; }
        IBsonDocumentProxy AsDocument { get; }
        IObjectIdProxy AsObjectId { get; }
        byte[] AsBinary { get; }
        bool AsBoolean { get; }
        string AsString { get; }
        int AsInt32 { get; }
        long AsInt64 { get; }
        double AsDouble { get; }
        decimal AsDecimal { get; }
        DateTime AsDateTime { get; }
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
        IBsonValueProxy this[string name] { get; set; }
        IBsonValueProxy this[int index] { get; set; }
    }

    public class BsonValueProxyV4 : IBsonValueProxy
    {
        protected readonly LiteDBv4.BsonValue _bsonValue;

        public BsonValueProxyV4(LiteDBv4.BsonValue bsonValue)
        {
            _bsonValue = bsonValue;
        }

        public ProxyBsonType Type => BsonTypeToBaseMap[_bsonValue.Type];

        public IBsonArrayProxy AsArray => _bsonValue.IsArray ? new BsonArrayProxyV4(_bsonValue.AsArray) : null;

        public IBsonDocumentProxy AsDocument => _bsonValue.IsDocument ? new BsonDocumentProxyV4(_bsonValue.AsDocument) : null;

        public IObjectIdProxy AsObjectId => _bsonValue.IsObjectId ? new ObjectIdProxyV4(_bsonValue) : null;

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

        public IBsonValueProxy this[string name]
        {
            get => throw new InvalidOperationException("Cannot access non-array type value on " + _bsonValue.RawValue);
            set => throw new InvalidOperationException("Cannot access non-array type value on " + _bsonValue.RawValue);
        }

        public IBsonValueProxy this[int index]
        {
            get => throw new InvalidOperationException("Cannot access non-array type value on " + _bsonValue.RawValue);
            set => throw new InvalidOperationException("Cannot access non-array type value on " + _bsonValue.RawValue);
        }

        public int CompareTo(IBsonValueProxy other)
        {
            var proxy = other as BsonValueProxyV4;
            return _bsonValue.CompareTo(proxy?._bsonValue);
        }

        public bool Equals(IBsonValueProxy other)
        {
            var proxy = other as BsonValueProxyV4;
            return _bsonValue.Equals(proxy?._bsonValue);
        }

        public int CompareTo(IBsonValueProxy other, object collation)
        {
            var proxy = other as BsonValueProxyV4;
            return _bsonValue.CompareTo(proxy?._bsonValue);
        }

        public override int GetHashCode()
        {
            return _bsonValue.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return _bsonValue.Equals(obj);
        }

        public override string ToString()
        {
            return _bsonValue.ToString();
        }

        public static readonly IReadOnlyDictionary<LiteDBv4.BsonType, ProxyBsonType> BsonTypeToBaseMap = new Dictionary<LiteDBv4.BsonType, ProxyBsonType>
        {
            { LiteDBv4.BsonType.MinValue, ProxyBsonType.MinValue },
            { LiteDBv4.BsonType.Null, ProxyBsonType.Null },
            { LiteDBv4.BsonType.Int32, ProxyBsonType.Int32 },
            { LiteDBv4.BsonType.Int64, ProxyBsonType.Int64 },
            { LiteDBv4.BsonType.Double, ProxyBsonType.Double },
            { LiteDBv4.BsonType.Decimal, ProxyBsonType.Decimal },
            { LiteDBv4.BsonType.String, ProxyBsonType.String },
            { LiteDBv4.BsonType.Document, ProxyBsonType.Document },
            { LiteDBv4.BsonType.Array, ProxyBsonType.Array },
            { LiteDBv4.BsonType.Binary, ProxyBsonType.Binary },
            { LiteDBv4.BsonType.ObjectId, ProxyBsonType.ObjectId },
            { LiteDBv4.BsonType.Guid, ProxyBsonType.Guid },
            { LiteDBv4.BsonType.Boolean, ProxyBsonType.Boolean },
            { LiteDBv4.BsonType.DateTime, ProxyBsonType.DateTime },
            { LiteDBv4.BsonType.MaxValue, ProxyBsonType.MaxValue },
        };

    }

    public class BsonValueProxyV5 : IBsonValueProxy
    {
        protected readonly LiteDBv5.BsonValue _bsonValue;

        public BsonValueProxyV5(LiteDBv5.BsonValue bsonValue)
        {
            _bsonValue = bsonValue;
        }

        public ProxyBsonType Type => BsonTypeToBaseMap[_bsonValue.Type];

        public IBsonArrayProxy AsArray => _bsonValue.IsArray ? new BsonArrayProxyV5(_bsonValue.AsArray) : null;

        public IBsonDocumentProxy AsDocument => _bsonValue.IsDocument ? new BsonDocumentProxyV5(_bsonValue.AsDocument) : null;

        public IObjectIdProxy AsObjectId => _bsonValue.IsObjectId ? new ObjectIdProxyV5(_bsonValue.AsObjectId) : null;

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

        public IBsonValueProxy this[string name]
        {
            get => throw new InvalidOperationException("Cannot access non-array type value on " + _bsonValue);
            set => throw new InvalidOperationException("Cannot access non-array type value on " + _bsonValue);
        }

        public IBsonValueProxy this[int index]
        {
            get => throw new InvalidOperationException("Cannot access non-array type value on " + _bsonValue);
            set => throw new InvalidOperationException("Cannot access non-array type value on " + _bsonValue);
        }

        public int CompareTo(IBsonValueProxy other)
        {
            var proxy = other as BsonValueProxyV5;
            return _bsonValue.CompareTo(proxy?._bsonValue);
        }

        public bool Equals(IBsonValueProxy other)
        {
            var proxy = other as BsonValueProxyV5;
            return _bsonValue.Equals(proxy?._bsonValue);
        }

        public int CompareTo(IBsonValueProxy other, object collation)
        {
            var proxy = other as BsonValueProxyV5;
            return _bsonValue.CompareTo(proxy?._bsonValue);
        }

        public override int GetHashCode()
        {
            return _bsonValue.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return _bsonValue.Equals(obj);
        }

        public override string ToString()
        {
            return _bsonValue.ToString();
        }

        public static readonly IReadOnlyDictionary<LiteDBv5.BsonType, ProxyBsonType> BsonTypeToBaseMap = new Dictionary<LiteDBv5.BsonType, ProxyBsonType>
        {
            { LiteDBv5.BsonType.MinValue, ProxyBsonType.MinValue },
            { LiteDBv5.BsonType.Null, ProxyBsonType.Null },
            { LiteDBv5.BsonType.Int32, ProxyBsonType.Int32 },
            { LiteDBv5.BsonType.Int64, ProxyBsonType.Int64 },
            { LiteDBv5.BsonType.Double, ProxyBsonType.Double },
            { LiteDBv5.BsonType.Decimal, ProxyBsonType.Decimal },
            { LiteDBv5.BsonType.String, ProxyBsonType.String },
            { LiteDBv5.BsonType.Document, ProxyBsonType.Document },
            { LiteDBv5.BsonType.Array, ProxyBsonType.Array },
            { LiteDBv5.BsonType.Binary, ProxyBsonType.Binary },
            { LiteDBv5.BsonType.ObjectId, ProxyBsonType.ObjectId },
            { LiteDBv5.BsonType.Guid, ProxyBsonType.Guid },
            { LiteDBv5.BsonType.Boolean, ProxyBsonType.Boolean },
            { LiteDBv5.BsonType.DateTime, ProxyBsonType.DateTime },
            { LiteDBv5.BsonType.MaxValue, ProxyBsonType.MaxValue },
        };

    }
}