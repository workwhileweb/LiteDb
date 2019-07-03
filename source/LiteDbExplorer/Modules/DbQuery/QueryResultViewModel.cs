using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Caliburn.Micro;
using Humanizer;
using JetBrains.Annotations;
using LiteDbExplorer.Core;

namespace LiteDbExplorer.Modules.DbQuery
{
    [Export(typeof(QueryResultViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class QueryResultViewModel : Screen
    {
        private readonly IDatabaseInteractions _databaseInteractions;

        [ImportingConstructor]
        public QueryResultViewModel(IDatabaseInteractions databaseInteractions)
        {
            _databaseInteractions = databaseInteractions;
        }

        public void SetResult(string displayName, string rawQuery, QueryResult queryResult)
        {
            DisplayName = displayName;
            RawQuery = rawQuery;
            QueryResult = queryResult;
        }

        public QueryResult QueryResult { get; private set; }

        public string RawQuery { get; set; } = string.Empty;

        [UsedImplicitly]
        public string ResultSetCountInfo => !QueryResult.HasValue ? "No records" : "record".ToQuantity(QueryResult.Count);

        [UsedImplicitly]
        public int ContentMaxLength { get; private set; } = 10000;

        [UsedImplicitly]
        public async Task ExportJson()
        {
            await _databaseInteractions.ExportToJson(QueryResult);
        }

        [UsedImplicitly]
        public bool CanExportJson => QueryResult != null && QueryResult.HasValue;
    }
}