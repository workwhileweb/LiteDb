extern alias v5;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Humanizer;

namespace LiteDbExplorer.Core
{
    extern alias v4;

    public static class LiteDbReferenceExtensions
    {
        public static IDictionary<string, v4::LiteDB.BsonType> SelectFirstDistinctKeyTypePair(this IEnumerable<DocumentReference> documents)
        {
            return documents
                .SelectMany(p => p.LiteDocument.RawValue)
                .GroupBy(p => p.Key)
                .Select(p => new {p.First().Key, Value = p.First().Value.Type})
                .ToDictionary(p => p.Key, p => p.Value);
        }

        public static IEnumerable<string> SelectAllDistinctKeys(this IEnumerable<DocumentReference> documents, FieldSortOrder sortOrder = FieldSortOrder.Original)
        {
            if (documents == null)
            {
                return Enumerable.Empty<string>();
            }

            var result = documents
                .SelectMany(p => p.LiteDocument.Keys)
                .Distinct(StringComparer.InvariantCulture);

            if (sortOrder == FieldSortOrder.Alphabetical)
            {
                result = result.OrderBy(_ => _);
            }

            return result;
        }

        public static IEnumerable<string> SelectAllDistinctKeys(this IEnumerable<v4::LiteDB.BsonDocument> documents,
            FieldSortOrder sortOrder = FieldSortOrder.Original)
        {
            if (documents == null)
            {
                return Enumerable.Empty<string>();
            }

            var result = documents
                .SelectMany(p => p.Keys)
                .Distinct(StringComparer.InvariantCulture);

            if (sortOrder == FieldSortOrder.Alphabetical)
            {
                result = result.OrderBy(_ => _);
            }

            return result;
        }

        public static bool IsFilesOrChunksCollection(this CollectionReference reference)
        {
            if (reference == null)
            {
                return false;
            }

            return reference.Name == @"_files" || reference.Name == @"_chunks";
        }

        public static bool HasAnyDocumentsReference(this IEnumerable<DocumentReference> documentReferences, DocumentTypeFilter filter = DocumentTypeFilter.All)
        {
            if (documentReferences == null)
            {
                return false;
            }

            if (filter == DocumentTypeFilter.All)
            {
                return documentReferences.Any();
            }

            return documentReferences
                .Where(p => p.Collection != null)
                .All(p => filter == DocumentTypeFilter.File ? p.Collection.IsFilesOrChunks : !p.Collection.IsFilesOrChunks);
        }
        
        public static bool IsFilesCollection(this CollectionReference collectionReference)
        {
            return collectionReference != null && collectionReference.IsFilesOrChunks;
        }

        public static string ToDisplayName(this DocumentReference documentReference)
        {
            if (documentReference == null)
            {
                return string.Empty;
            }
            
            return string.Join(" - ", documentReference.Collection?.Name, documentReference.LiteDocument["_id"].AsString);
        }

        public static string ToDisplayValue(this v4::LiteDB.BsonValue bsonValue, int? maxLength = null, CultureInfo cultureInfo = null)
        {
            if (bsonValue == null)
            {
                return string.Empty;
            }

            if (cultureInfo == null)
            {
                cultureInfo = CultureInfo.InvariantCulture;
            }

            string result;

            try
            {
                switch (bsonValue.Type)
                {
                    case v4::LiteDB.BsonType.MinValue:
                        result = @"-∞";
                        break;
                    case v4::LiteDB.BsonType.MaxValue:
                        result = @"+∞";
                        break;
                    case v4::LiteDB.BsonType.Boolean:
                        result = bsonValue.AsBoolean.ToString(cultureInfo).ToLower();
                        break;
                    case v4::LiteDB.BsonType.DateTime:
                        result = bsonValue.AsDateTime.ToString(cultureInfo);
                        break;
                    case v4::LiteDB.BsonType.Null:
                        result = "(null)";
                        break;
                    case v4::LiteDB.BsonType.Binary:
                        result = Convert.ToBase64String(bsonValue.AsBinary);
                        break;
                    case v4::LiteDB.BsonType.Int32:
                    case v4::LiteDB.BsonType.Int64:
                    case v4::LiteDB.BsonType.Double:
                    case v4::LiteDB.BsonType.Decimal:
                    case v4::LiteDB.BsonType.String:
                    case v4::LiteDB.BsonType.ObjectId:
                    case v4::LiteDB.BsonType.Guid:
                        result = Convert.ToString(bsonValue.RawValue, cultureInfo);
                        break;
                    case v4::LiteDB.BsonType.Document:
                        result = @"[Document]";
                        break;
                    case v4::LiteDB.BsonType.Array:
                        result = @"[Array]";
                        break;
                    default:
                        result = v4::LiteDB.JsonSerializer.Serialize(bsonValue);
                        break;
                }

                if (maxLength.HasValue)
                {
                    return result?.Truncate(maxLength.Value);
                }

                return result;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
    }
}