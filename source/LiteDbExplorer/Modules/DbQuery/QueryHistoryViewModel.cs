using System;
using System.ComponentModel.Composition;
using System.Windows.Data;
using Caliburn.Micro;
using JetBrains.Annotations;
using LiteDbExplorer.Core;

namespace LiteDbExplorer.Modules.DbQuery
{
    [Export(typeof(QueryHistoryViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class QueryHistoryViewModel : Screen
    {
        private readonly IQueryHistoryProvider _queryHistoryProvider;
        private readonly IApplicationInteraction _applicationInteraction;
        private DatabaseReference _activeDatabase;
        private bool _filterActiveDatabase;

        [ImportingConstructor]
        public QueryHistoryViewModel(IQueryHistoryProvider queryHistoryProvider, IApplicationInteraction applicationInteraction)
        {
            _queryHistoryProvider = queryHistoryProvider;
            _applicationInteraction = applicationInteraction;

            DisplayName = "Query History";

            QueryHistoriesView = new CollectionViewSource
            {
                Source = _queryHistoryProvider.QueryHistories
            };

            QueryHistoriesView.Filter += (sender, args) =>
            {
                if (_activeDatabase != null && FilterActiveDatabase && args.Item is RawQueryHistory rawQueryHistory)
                {
                    args.Accepted = rawQueryHistory.DatabaseLocation.Equals(_activeDatabase.Location,
                        StringComparison.OrdinalIgnoreCase);
                }
                else
                {
                    args.Accepted = true;
                }
            };

        }

        public CollectionViewSource QueryHistoriesView { get; }

        public bool FilterActiveDatabase
        {
            get => _filterActiveDatabase;
            set
            {
                _filterActiveDatabase = value;
                QueryHistoriesView.View.Refresh();
            }
        }

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
                _queryHistoryProvider.Remove(item);
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

        public void SetActiveDatabase(DatabaseReference currentDatabase)
        {
            _activeDatabase = currentDatabase;

            QueryHistoriesView.View.Refresh();
        }
    }
}