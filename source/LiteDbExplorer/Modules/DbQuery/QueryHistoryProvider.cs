using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using DynamicData;
using DynamicData.Binding;
using JetBrains.Annotations;

namespace LiteDbExplorer.Modules.DbQuery
{
    public interface IQueryHistoryProvider
    {
        ReadOnlyObservableCollection<RawQueryHistory> QueryHistories { get; }
        bool IsEmpty { get; }
        void Add(RawQueryHistory item);
        void Remove(RawQueryHistory item);
    }

    public class QueryHistoryProvider : INotifyPropertyChanged, IQueryHistoryProvider
    {
        private CompositeDisposable _compositeDisposable;
        readonly ReadOnlyObservableCollection<RawQueryHistory> _queryHistories;
        private readonly SourceList<RawQueryHistory> _historySourceList;

        public QueryHistoryProvider()
        {
            _historySourceList = new SourceList<RawQueryHistory>();

            var counter = _historySourceList.CountChanged.Subscribe(i => IsEmpty = i == 0);
            
            var limiter = _historySourceList.LimitSizeTo(100).Subscribe();

            var sharedList = _historySourceList
                .Connect()
                .DeferUntilLoaded()
                .Sort(SortExpressionComparer<RawQueryHistory>.Descending(t => t.LastRunAt))
                .ObserveOnDispatcher()
                .Bind(out _queryHistories)
                .Subscribe();

            _compositeDisposable = new CompositeDisposable(counter, limiter, sharedList);
        }


        public ReadOnlyObservableCollection<RawQueryHistory> QueryHistories => _queryHistories;

        public bool IsEmpty { get; private set; }

        public void Add(RawQueryHistory item)
        {
            _historySourceList.Add(item);
        }

        public void Remove(RawQueryHistory item)
        {
            _historySourceList.Remove(item);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [UsedImplicitly]
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}