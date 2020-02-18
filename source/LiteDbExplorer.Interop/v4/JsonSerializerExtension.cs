using System.Collections.Generic;

namespace LiteDbExplorer.Core
{
    extern alias v4;

    public static class JsonSerializerExtension
    {
        public static string SerializeDecoded(this QueryResult queryResult, bool pretty = false)
        {
            return queryResult.Serialize(pretty);
        }

        public static string SerializeDecoded(this IEnumerable<v4::LiteDB.BsonValue> bsonValue, bool pretty = false)
        {
            var queryResult = new QueryResult(bsonValue);

            return SerializeDecoded(queryResult, pretty);
        }

        public static string SerializeDecoded(this v4::LiteDB.BsonValue bsonValue, bool pretty = false)
        {
            var json = v4::LiteDB.JsonSerializer.Serialize(bsonValue, pretty, false);

            return EncodingExtensions.DecodeEncodedNonAsciiCharacters(json);
        }
    }
}