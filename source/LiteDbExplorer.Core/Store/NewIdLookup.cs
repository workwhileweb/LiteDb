using System;
using System.Linq;
using LiteDB;

namespace LiteDbExplorer.Core
{
    public class NewIdLookup
    {
        public static readonly BsonType[] HandledIdTypes = new[]
        {
            BsonType.ObjectId,
            BsonType.Int32,
            BsonType.Int64,
            BsonType.Guid,
            BsonType.String,
        };

        private BsonType _idType;
        private readonly BsonValue _referenceIdBsonValue;

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

        public BsonType IdType
        {
            get => _idType;
            set
            {
                _idType = value;
                SetNewId();
            }
        }

        public BsonValue NewId { get; private set; }

        protected virtual void SetNewId()
        {
            NewId = GetNewId(IdType, _referenceIdBsonValue);
        }

        public static bool TryParse(BsonType idType, string rawValue, out BsonValue bsonValue)
        {
            bsonValue = null;

            if (string.IsNullOrEmpty(rawValue))
            {
                return false;
            }

            switch (idType)
            {
                case BsonType.Int32:
                    if (Int32.TryParse(rawValue, out var intValue))
                    {
                        bsonValue = new BsonValue(intValue);
                        return true;
                    }
                    return false;
                case BsonType.Int64:
                    if (Int64.TryParse(rawValue, out var longValue))
                    {
                        bsonValue = new BsonValue(longValue);
                        return true;
                    }
                    return false;
                case BsonType.String:
                    bsonValue = new BsonValue(rawValue);
                    return true;
                case BsonType.ObjectId:
                    try
                    {
                        var objectId = new ObjectId(rawValue);
                        bsonValue = new BsonValue(objectId);
                        return true;
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                case BsonType.Guid:
                    if (Guid.TryParse(rawValue, out var guidValue))
                    {
                        bsonValue = new BsonValue(guidValue);
                        return true;
                    }
                    return false;
                default:
                    return false;
            }
        }

        public static BsonValue GetNewId(BsonType idType, BsonValue idBsonValue = null)
        {
            switch (idType)
            {
                case BsonType.Int32:
                    if (idBsonValue != null && idBsonValue.IsInt32)
                    {
                        return idBsonValue.AsInt32 + 1;
                    }
                    return 1;
                case BsonType.Int64:
                    if (idBsonValue != null && idBsonValue.IsInt64)
                    {
                        return idBsonValue.AsInt64 + 1L;
                    }
                    return 1L;
                case BsonType.String:
                    return Guid.NewGuid().ToString();
                case BsonType.ObjectId:
                    return ObjectId.NewObjectId();
                case BsonType.Guid:
                    return ObjectId.NewObjectId().ToString();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}