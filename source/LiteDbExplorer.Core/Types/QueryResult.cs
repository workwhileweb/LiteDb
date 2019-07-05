using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using LiteDB;

namespace LiteDbExplorer.Core
{
    public class QueryResult : IReferenceNode, IJsonSerializerProvider
    {
        private const string EXPR_PATH = @"expr";

        public QueryResult(IEnumerable<BsonValue> bsonValues)
        {
            InstanceId = Guid.NewGuid().ToString("D");

            if (bsonValues == null)
            {
                HasValue = false;
            }
            else
            {
                HasValue = true;

                var items = bsonValues as BsonValue[] ?? bsonValues.ToArray();

                if (bsonValues is BsonArray bsonArray)
                {
                    IsArray = true;
                    AsArray = bsonArray;
                    Count = bsonArray.Count;
                }
                else if (items.Length == 1 && items[0].IsArray)
                {
                    IsArray = true;
                    AsArray = items[0].AsArray;
                    Count = items[0].AsArray.Count;
                }
                else if (items.Length == 1 && items[0].IsDocument)
                {
                    var bsonDocument = items[0].AsDocument;
                    if (bsonDocument.ContainsKey(EXPR_PATH))
                    {
                        if (bsonDocument[EXPR_PATH].IsArray)
                        {
                            IsArray = true;
                            AsArray = bsonDocument[EXPR_PATH].AsArray;
                            Count = bsonDocument[EXPR_PATH].AsArray.Count;
                        }
                        else
                        {
                            IsDocument = true;
                            AsDocument = new BsonDocument {{"value", bsonDocument[EXPR_PATH]}};
                            Count = 1;
                        }
                    }
                    else
                    {
                        IsDocument = true;
                        AsDocument = items[0].AsDocument;
                        Count = 1;
                    }
                }
                else
                {
                    IsArray = true;
                    AsArray = new BsonArray(items);
                    Count = items.Length;
                }
            }
        }

        public string InstanceId { get; }

        public bool IsArray { get; private set; }

        public bool IsDocument { get; private set; }

        public int Count { get; private set; }

        public bool HasValue { get; private set; }

        public BsonArray AsArray { get; private set; }

        public BsonDocument AsDocument { get; private set; }

        public string Serialize(bool pretty = false, bool decoded = true)
        {
            var json = string.Empty;
            
            if (IsArray)
            {
                json = JsonSerializer.Serialize(AsArray, pretty, false);
            }
            else if (IsDocument)
            {
                json = JsonSerializer.Serialize(AsDocument, pretty, false);
            }

            return decoded ? EncodingExtensions.DecodeEncodedNonAsciiCharacters(json) : json;
        }

        public void Serialize(TextWriter writer, bool pretty = false)
        {
            if (IsArray)
            {
                JsonSerializer.Serialize(AsArray, writer, pretty, false);
            }
            else if (IsDocument)
            {
                JsonSerializer.Serialize(AsDocument, writer, pretty, false);
            }
        }
    }

    public class QueryResultDataTableAdapter
    {
        public QueryResultDataTableAdapter(IEnumerable<BsonValue> bsonValues)
        {
            var table = new DataTable();

            foreach (var value in bsonValues)
            {
                var row = table.NewRow();
                var doc = value.IsDocument ?
                    value.AsDocument :
                    new BsonDocument { ["[value]"] = value };

                if (doc.Keys.Count == 0) doc["[root]"] = "{}";

                foreach (var key in doc.Keys)
                {
                    var col = table.Columns[key];
                    if (col == null)
                    {
                        table.Columns.Add(key);

                        col = table.Columns[key];
                        col.ReadOnly = key == "_id";
                    }
                }

                foreach (var key in doc.Keys)
                {
                    row[key] = doc[key].ToDisplayValue();
                }

                table.Rows.Add(row);
            }
        }
    }

}