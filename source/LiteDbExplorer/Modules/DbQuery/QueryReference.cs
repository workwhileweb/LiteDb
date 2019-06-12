using LiteDbExplorer.Core;

namespace LiteDbExplorer.Modules.DbQuery
{
    public class QueryReference
    {
        private readonly string _rawQuery;

        public QueryReference(string rawQuery)
        {
            _rawQuery = rawQuery;
        }

        public string GetRawQuery()
        {
            return _rawQuery;
        }

        public static QueryReference Empty()
        {
            return new QueryReference(string.Empty);
        }

        public static QueryReference Find(CollectionReference collectionReference, int? skip = null, int? limit = null)
        {
            var rawQuery = $"db.{collectionReference.Name}.find";
            if (skip.HasValue)
            {
                rawQuery += $" skip {skip} ";
            }
            if (limit.HasValue)
            {
                rawQuery += $" limit {limit} ";
            }
            return new QueryReference(rawQuery.Trim());
        }
    }
}