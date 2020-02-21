using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;


namespace LiteDbExplorer.Core
{
    extern alias v4;
    using LiteDBv4 = v4::LiteDB;

    public class QueryResult : IReferenceNode, IJsonSerializerProvider
    {
        private const string EXPR_PATH = @"expr";

        public QueryResult(IEnumerable<LiteDBv4.BsonValue> bsonValues)
        {
            InstanceId = Guid.NewGuid().ToString("D");

            Initialize(bsonValues);
        }

        public string InstanceId { get; }

        public bool IsArray { get; private set; }

        public bool IsDocument { get; private set; }

        public int Count { get; private set; }

        public bool HasValue { get; private set; }

        public IEnumerable<LiteDBv4.BsonValue> Source { get; private set; }

        public LiteDBv4.BsonArray AsArray { get; private set; }

        public LiteDBv4.BsonDocument AsDocument { get; private set; }

        public DataTable DataTable => Source.ToDataTable();

        public string Serialize(bool pretty = false, bool decoded = true)
        {
            var json = string.Empty;
            
            if (IsArray)
            {
                json = LiteDBv4.JsonSerializer.Serialize(AsArray, pretty, false);
            }
            else if (IsDocument)
            {
                json = LiteDBv4.JsonSerializer.Serialize(AsDocument, pretty, false);
            }

            return decoded ? EncodingExtensions.DecodeEncodedNonAsciiCharacters(json) : json;
        }

        public void Serialize(TextWriter writer, bool pretty = false)
        {
            if (IsArray)
            {
                LiteDBv4.JsonSerializer.Serialize(AsArray, writer, pretty, false);
            }
            else if (IsDocument)
            {
                LiteDBv4.JsonSerializer.Serialize(AsDocument, writer, pretty, false);
            }
        }

        protected void Initialize(IEnumerable<LiteDBv4.BsonValue> bsonValues)
        {
            Source = bsonValues;

            if (bsonValues == null)
            {
                HasValue = false;
            }
            else
            {
                HasValue = true;

                var items = bsonValues as LiteDBv4.BsonValue[] ?? bsonValues.ToArray();

                if (bsonValues is LiteDBv4.BsonArray bsonArray)
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
                            AsDocument = new LiteDBv4.BsonDocument {{@"value", bsonDocument[EXPR_PATH]}};
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
                    AsArray = new LiteDBv4.BsonArray(items);
                    Count = items.Length;
                }
            }
        }

    }
}