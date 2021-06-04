using System.Collections.Generic;
using System.Data;
using LiteDB;

namespace LiteDbExplorer.Core
{
    public static class QueryResultExtensions
    {
        public static DataTable ToDataTable(this IEnumerable<BsonValue> bsonValues,
            ICultureFormat cultureFormat)
        {
            DefaultCultureFormat.EnsureValue(ref cultureFormat);

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
                    new BsonDocument { ["[value]"] = value };

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
                    var bsonValue = doc[key];

                    if (bsonValue.IsNull || bsonValue.IsArray || bsonValue.IsDocument || bsonValue.IsBinary) // convertToString || 
                    {
                        row[key] = bsonValue.ToDisplayValue(null, cultureFormat);
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


        public static LookupTable ToLookupTable(this IEnumerable<BsonValue> bsonValues, ICultureFormat cultureFormat)
        {
            DefaultCultureFormat.EnsureValue(ref cultureFormat);

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
                    new BsonDocument { ["[value]"] = value };

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
                    var bsonValue = doc[key];
                    if (bsonValue.IsNull || bsonValue.IsArray || bsonValue.IsDocument || bsonValue.IsBinary)
                    {
                        row[key] = bsonValue.ToDisplayValue(null, cultureFormat);
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