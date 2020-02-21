using System;
using System.Linq;

namespace LiteDbExplorer.Core
{
    extern alias v4;
    using LiteDBv4 = v4::LiteDB;

    public class NewIdLookup
    {
        public static readonly LiteDBv4.BsonType[] HandledIdTypes = new[]
        {
            LiteDBv4.BsonType.ObjectId,
            LiteDBv4.BsonType.Int32,
            LiteDBv4.BsonType.Int64,
            LiteDBv4.BsonType.Guid,
            LiteDBv4.BsonType.String,
        };

        private LiteDBv4.BsonType _idType;
        private readonly LiteDBv4.BsonValue _referenceIdBsonValue;

        public NewIdLookup(CollectionReference collection)
        {
            var bsonValue = collection.LiteCollection.Max();
            if (HandledIdTypes.Contains(bsonValue.Type))
            {
                _referenceIdBsonValue = bsonValue;
                IdType = bsonValue.Type;
            }
            else
            {
                IdType = HandledIdTypes[0];       
            }
        }

        public LiteDBv4.BsonType IdType
        {
            get => _idType;
            set
            {
                _idType = value;
                SetNewId();
            }
        }

        public LiteDBv4.BsonValue NewId { get; private set; }

        protected virtual void SetNewId()
        {
            NewId = GetNewId(IdType, _referenceIdBsonValue);
        }

        public static bool TryParse(LiteDBv4.BsonType idType, string rawValue, out LiteDBv4.BsonValue bsonValue)
        {
            bsonValue = null;

            if (string.IsNullOrEmpty(rawValue))
            {
                return false;
            }

            switch (idType)
            {
                case LiteDBv4.BsonType.Int32:
                    if (Int32.TryParse(rawValue, out var intValue))
                    {
                        bsonValue = new LiteDBv4.BsonValue(intValue);
                        return true;
                    }
                    return false;
                case LiteDBv4.BsonType.Int64:
                    if (Int64.TryParse(rawValue, out var longValue))
                    {
                        bsonValue = new LiteDBv4.BsonValue(longValue);
                        return true;
                    }
                    return false;
                case LiteDBv4.BsonType.String:
                    bsonValue = new LiteDBv4.BsonValue(rawValue);
                    return true;
                case LiteDBv4.BsonType.ObjectId:
                    try
                    {
                        var objectId = new LiteDBv4.ObjectId(rawValue);
                        bsonValue = new LiteDBv4.BsonValue(objectId);
                        return true;
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                case LiteDBv4.BsonType.Guid:
                    if (Guid.TryParse(rawValue, out var guidValue))
                    {
                        bsonValue = new LiteDBv4.BsonValue(guidValue);
                        return true;
                    }
                    return false;
                default:
                    return false;
            }
        }

        public static LiteDBv4.BsonValue GetNewId(LiteDBv4.BsonType idType, LiteDBv4.BsonValue idBsonValue = null)
        {
            switch (idType)
            {
                case LiteDBv4.BsonType.Int32:
                    if (idBsonValue != null && idBsonValue.IsInt32)
                    {
                        return new LiteDBv4.BsonValue(idBsonValue.AsInt32 + 1);
                    }
                    return new LiteDBv4.BsonValue(1);
                case LiteDBv4.BsonType.Int64:
                    if (idBsonValue != null && idBsonValue.IsInt64)
                    {
                        return new LiteDBv4.BsonValue(idBsonValue.AsInt64 + 1L);
                    }
                    return new LiteDBv4.BsonValue(1L);
                case LiteDBv4.BsonType.String:
                    return new LiteDBv4.BsonValue(Guid.NewGuid().ToString());
                case LiteDBv4.BsonType.ObjectId:
                    return new LiteDBv4.BsonValue(LiteDBv4.ObjectId.NewObjectId());
                case LiteDBv4.BsonType.Guid:
                    return new LiteDBv4.BsonValue(LiteDBv4.ObjectId.NewObjectId().ToString());
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}