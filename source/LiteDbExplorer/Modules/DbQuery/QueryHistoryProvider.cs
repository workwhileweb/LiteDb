using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using DynamicData;
using DynamicData.Binding;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace LiteDbExplorer.Modules.DbQuery
{
    public interface IQueryHistoryProvider
    {
        ReadOnlyObservableCollection<RawQueryHistory> QueryHistories { get; }
        bool IsEmpty { get; }
        void Upsert(RawQueryHistory item);
        void Remove(RawQueryHistory item);
    }

    public class QueryHistoryProvider : INotifyPropertyChanged, IQueryHistoryProvider, IDisposable
    {
        private readonly CompositeDisposable _compositeDisposable;
        readonly ReadOnlyObservableCollection<RawQueryHistory> _queryHistories;
        private readonly SourceList<RawQueryHistory> _historySourceList;

        public QueryHistoryProvider()
        {
            _historySourceList = new SourceList<RawQueryHistory>();

            var counter = _historySourceList.CountChanged.Subscribe(i => IsEmpty = i == 0);
            
            var limiter = _historySourceList.LimitSizeTo(100).Subscribe();

            _historySourceList.AddRange(LoadFileData());

            var sharedList = _historySourceList
                .Connect()
                .DeferUntilLoaded()
                .Sort(SortExpressionComparer<RawQueryHistory>.Descending(t => t.LastRunAt))
                .ObserveOnDispatcher()
                .Bind(out _queryHistories)
                .Subscribe();

            var disposable = _queryHistories
                .ObserveCollectionChanges()
                .Throttle(TimeSpan.FromSeconds(2))
                .Subscribe(pattern =>
                {
                    SaveFileData(_queryHistories);
                });

            _compositeDisposable = new CompositeDisposable(counter, limiter, sharedList,disposable);
        }


        public ReadOnlyObservableCollection<RawQueryHistory> QueryHistories => _queryHistories;

        public bool IsEmpty { get; private set; }

        public void Upsert(RawQueryHistory item)
        {
            _historySourceList.Edit(list =>
            {
                var lastHistory = list
                    .OrderByDescending(p => p.LastRunAt)
                    .FirstOrDefault(p => RawQueryHistory.RawSourceComparer.Equals(item, p));

                if (lastHistory != null)
                {
                    lastHistory.LastRunAt = DateTime.UtcNow;
                }
                else
                {
                    list.Add(item);
                }
            });
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

        protected static string StorageFilePath => Path.Combine(Paths.AppDataPath, "query_history.json");

        private static readonly JsonSerializerSettings _serializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new IgnoreParentPropertiesResolver(true),
            Formatting = Formatting.Indented
        };

        private static IEnumerable<RawQueryHistory> LoadFileData()
        {
            try
            {
                if (File.Exists(StorageFilePath))
                {
                    var value = File.ReadAllText(StorageFilePath);
                    var rawQueryHistories = JsonConvert.DeserializeObject<IEnumerable<RawQueryHistory>>(value, _serializerSettings);
                    return rawQueryHistories ?? Enumerable.Empty<RawQueryHistory>();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                App.ShowError(e, $"An error occurred while reading the configuration file: '{StorageFilePath}'.\n\nTo avoid this error again a new configuration will be created!");
                if (File.Exists(StorageFilePath))
                {
                    var destFileName = Path.Combine(
                            Path.GetDirectoryName(StorageFilePath), 
                            $"{Path.GetFileNameWithoutExtension(StorageFilePath)}_{DateTime.UtcNow.Ticks}_fail.{Path.GetExtension(StorageFilePath).TrimStart('.')}"
                        );
                    File.Copy(StorageFilePath, destFileName);
                    File.WriteAllText(StorageFilePath, JsonConvert.SerializeObject(new List<RawQueryHistory>()));
                }
            }

            return Enumerable.Empty<RawQueryHistory>();
        }

        private static void SaveFileData(IEnumerable<RawQueryHistory> data)
        {
            File.WriteAllText(StorageFilePath, JsonConvert.SerializeObject(data, _serializerSettings));
        }

        public void Dispose()
        {
            _compositeDisposable?.Dispose();
        }
    }
}