extern alias v5;
using System.Collections.Generic;
using System.Data;

namespace LiteDbExplorer.Core
{
    extern alias v4;

    public static class QueryResultExtensions
    {
        public static DataTable ToDataTable(this IEnumerable<v4::LiteDB.BsonValue> bsonValues)
        {
            var table = new DataTable();
            if (bsonValues == null)
            {
                return table;
            }

            foreach (var value in bsonValues)
            {
                var row = table.NewRow();
                var doc = value.IsDocument ?
                    value.AsDocument :
                    new v4::LiteDB.BsonDocument { ["[value]"] = value };

                if (doc.Keys.Count == 0)
                {
                    doc["[root]"] = "{}";
                }

                foreach (var key in doc.Keys)
                {
                    var col = table.Columns[key];
                    if (col == null)
                    {
                        table.Columns.Add(key);
                        var readOnly = key == "_id";
                        col = table.Columns[key];
                        col.ColumnName = key;
                        col.Caption = key;
                        col.ReadOnly = readOnly;
                    }
                }

                foreach (var key in doc.Keys)
                {
                    v4::LiteDB.BsonValue bsonValue = doc[key];
                    if (bsonValue.IsNull || bsonValue.IsArray || bsonValue.IsDocument || bsonValue.IsBinary)
                    {
                        row[key] = bsonValue.ToDisplayValue();
                    }
                    else
                    {
                        row[key] = bsonValue.RawValue;                        
                    }
                }

                table.Rows.Add(row);
            }

            return table;
        }


        public static LookupTable ToLookupTable(this IEnumerable<v4::LiteDB.BsonValue> bsonValues)
        {
            var table = new LookupTable();
            if (bsonValues == null)
            {
                return table;
            }

            foreach (var value in bsonValues)
            {
                var row = table.NewRow();
                var doc = value.IsDocument ?
                    value.AsDocument :
                    new v4::LiteDB.BsonDocument { ["[value]"] = value };

                if (doc.Keys.Count == 0)
                {
                    doc["[root]"] = "{}";
                }

                foreach (var key in doc.Keys)
                {
                    var col = table.Columns[key];
                    if (col == null)
                    {
                        table.Columns.Add(new LookupDataColumn { ColumnName = key });
                    }
                }

                foreach (var key in doc.Keys)
                {
                    v4::LiteDB.BsonValue bsonValue = doc[key];
                    if (bsonValue.IsNull || bsonValue.IsArray || bsonValue.IsDocument || bsonValue.IsBinary)
                    {
                        row[key] = bsonValue.ToDisplayValue();
                    }
                    else
                    {
                        row[key] = bsonValue.RawValue;                        
                    }
                }

                table.Rows.Add(row);
            }

            return table;
        }

    }
}