using System.ComponentModel.Composition;
using Caliburn.Micro;
using JetBrains.Annotations;
using LiteDbExplorer.Core;

namespace LiteDbExplorer.Modules.DbQuery
{
    [Export(typeof(QueryHistoryViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class QueryHistoryViewModel : Screen
    {
        private readonly IApplicationInteraction _applicationInteraction;

        [ImportingConstructor]
        public QueryHistoryViewModel(IQueryHistoryProvider queryHistoryProvider, IApplicationInteraction applicationInteraction)
        {
            _applicationInteraction = applicationInteraction;

            DisplayName = "Query History";

            HistoryProvider = queryHistoryProvider;
        }

        public IQueryHistoryProvider HistoryProvider { get; }

        [UsedImplicitly]
        public void CopyQuery(RawQueryHistory item)
        {
            _applicationInteraction.PutClipboardText(item.RawQuery);
        }

        [UsedImplicitly]
        public bool CanCopyQuery(RawQueryHistory item)
        {
            return item != null;
        }

        [UsedImplicitly]
        public void RemoveQuery(RawQueryHistory item)
        {
            if (item != null)
            {
                HistoryProvider.Remove(item);
            }
        }

        [UsedImplicitly]
        public bool CanRemoveQuery(RawQueryHistory item)
        {
            return item != null;
        }

        [UsedImplicitly]
        public void InsertQuery(RawQueryHistory item)
        {
            if (Parent is IQueryHistoryHandler queryHistoryHandler)
            {
                queryHistoryHandler.InsertQuery(item);
            }
        }

        [UsedImplicitly]
        public bool CanInsertQuery(RawQueryHistory item)
        {
            return item != null && Parent is IQueryHistoryHandler;
        }

        public void DefineFilter(DatabaseReference currentDatabase)
        {
            // TODO: Filter from database
        }
    }
}