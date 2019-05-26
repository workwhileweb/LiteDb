using System.Collections.Generic;
using LiteDB;
using LiteDbExplorer.Core;

namespace LiteDbExplorer.Extensions
{
    public static class JsonSerializerExtension
    {
        public static string SerializeDecoded(this QueryResult queryResult, bool pretty = false)
        {
            return queryResult.Serialize(pretty);
        }

        public static string SerializeDecoded(this IEnumerable<BsonValue> bsonValue, bool pretty = false)
        {
            var queryResult = new QueryResult(bsonValue);

            return SerializeDecoded(queryResult, pretty);
        }

        public static string SerializeDecoded(this BsonValue bsonValue, bool pretty = false)
        {
            var json = JsonSerializer.Serialize(bsonValue, pretty, false);

            return EncodingExtensions.DecodeEncodedNonAsciiCharacters(json);
        }
    }
}