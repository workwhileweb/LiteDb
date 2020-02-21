using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using Serilog;

namespace LiteDbExplorer.Core
{
    extern alias v4;
    using LiteDBv4 = v4::LiteDB;

    public interface IDatabaseReference
    {
        string Name { get; set; }
        string Location { get; set; }
        ObservableCollection<CollectionReferenceLookup> CollectionsLookup { get; }
        ObservableCollection<CollectionReference> Collections { get; }
        string InstanceId { get; }
        CollectionReference this[string name] { get; }
        void BeforeDispose();
        DocumentReference AddFile(string id, string path);
        bool FileExists(string id);
        CollectionReference AddCollection(string name);
        void RenameCollection(string oldName, string newName);
        void DropCollection(string name);
        bool ContainsCollection(string name);
        void Refresh();
        bool ReferenceEquals(ReferenceNode<DatabaseReference> reference);
        void Dispose();
        event EventHandler<ReferenceChangedEventArgs<DatabaseReference>> ReferenceChanged;
        event PropertyChangedEventHandler PropertyChanged;
        event PropertyChangingEventHandler PropertyChanging;
    }

    public sealed class DatabaseReference : ReferenceNode<DatabaseReference>, IDatabaseReference
    {
        private readonly bool _enableLog;
        private ObservableCollection<CollectionReference> _collections;
        private bool _isDisposing;
        private bool _beforeDisposeHandled;

        public DatabaseReference([NotNull] DatabaseConnectionOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            _enableLog = options.EnableLog;

            Log.Information("Open database {path}, mode: {mode}.", options.Path, options.Mode);

            Location = options.Path;
            Name = Path.GetFileName(options.Path);

            var connectionString = options.GetConnectionString();

            LiteDatabase = new LiteDBv4.LiteDatabase(connectionString, log: GetLogger());

            UpdateCollections();

            OnReferenceChanged(ReferenceNodeChangeAction.Add, this);

        }

        public LiteDBv4.LiteDatabase LiteDatabase { get; }

        public string Name { get; set; }

        public string Location { get; set; }

        public ObservableCollection<CollectionReferenceLookup> CollectionsLookup { get; private set; }

        public ObservableCollection<CollectionReference> Collections
        {
            get => _collections;
            private set
            {
                OnPropertyChanging();
                if (_collections != null)
                {
                    _collections.CollectionChanged -= OnCollectionChanged;
                }
                
                _collections = value;

                if (_collections != null)
                {
                    _collections.CollectionChanged += OnCollectionChanged;
                }
                OnPropertyChanged();
            }
        }

        public CollectionReference this[string name]
        {
            get { return Collections.FirstOrDefault(p => p.Name.Equals(name)); }
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewItems != null)
                    {
                        BroadcastChanges(ReferenceNodeChangeAction.Add, e.NewItems.Cast<CollectionReference>());
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                case NotifyCollectionChangedAction.Reset:
                    if (e.OldItems != null)
                    {
                        BroadcastChanges(ReferenceNodeChangeAction.Remove, e.OldItems.Cast<CollectionReference>());
                    }
                    break;
            }
        }

        private void BroadcastChanges(ReferenceNodeChangeAction action, DatabaseReference reference)
        {
            BroadcastChanges(action, Collections);

            OnReferenceChanged(action, reference);
        }

        private void BroadcastChanges(ReferenceNodeChangeAction action, IEnumerable<CollectionReference> items)
        {
            foreach (var referenceCollection in items)
            {
                foreach (var documentReference in referenceCollection.Items)
                {
                    documentReference.OnReferenceChanged(action, documentReference);
                }
                
                referenceCollection.OnReferenceChanged(action, referenceCollection);
            }
        }

        public void BeforeDispose()
        {
            if (_isDisposing)
            {
                return;
            }

            _beforeDisposeHandled = true;

            BroadcastChanges(ReferenceNodeChangeAction.Dispose, this);
        }

        protected override void Dispose(bool disposing)
        {
            if (_isDisposing)
            {
                return;
            }

            _isDisposing = true;

            if (!_beforeDisposeHandled)
            {
                BroadcastChanges(ReferenceNodeChangeAction.Dispose, this);
            }
            
            Log.Information("Dispose database {path}", Location);

            LiteDatabase.Dispose();
        }

        private LiteDBv4.Logger GetLogger()
        {
            if (_enableLog)
            {
                return new LiteDBv4.Logger(LiteDBv4.Logger.FULL, log =>
                {
                    Log.ForContext("DatabaseName", Name).Information(log);
                });
            }

            return null;
        }

        private void UpdateCollections()
        {
            // TODO: Bind database tree and lazy load CollectionReference
            CollectionsLookup = new ObservableCollection<CollectionReferenceLookup>(
                LiteDatabase.GetCollectionNames()
                    .Where(name => name != @"_chunks")
                    .OrderBy(name => name)
                    .Select(name => new CollectionReferenceLookup(name))
            );

            Collections = new ObservableCollection<CollectionReference>(MapCollectionReference(CollectionsLookup));
        }

        private IEnumerable<CollectionReference> MapCollectionReference(IEnumerable<CollectionReferenceLookup> lookups)
        {
            return lookups.Select(MapCollectionReference);
        }

        private CollectionReference MapCollectionReference(CollectionReferenceLookup lookup)
        {
            return lookup.Type == CollectionHandlerType.Files
                ? new FileCollectionReference(lookup.Name, this)
                : new CollectionReference(lookup.Name, this);
        }

        public DocumentReference AddFile(string id, string path)
        {
            LiteDatabase.FileStorage.Upload(id, path);
            UpdateCollections();
            var collection = Collections.First(a => a is FileCollectionReference);
            return collection.Items.First(a => a.LiteDocument["_id"] == id);
        }

        public bool FileExists(string id)
        {
            return LiteDatabase.FileStorage.Exists(id);
        }

        public CollectionReference AddCollection(string name)
        {
            if (LiteDatabase.GetCollectionNames().Contains(name))
            {
                throw new Exception($"Cannot add collection \"{name}\", collection with that name already exists.");
            }

            var coll = LiteDatabase.GetCollection(name);
            var newDoc = new LiteDBv4.BsonDocument
            {
                ["_id"] = LiteDBv4.ObjectId.NewObjectId()
            };

            coll.Insert(newDoc);
            coll.Delete(newDoc["_id"]);

            UpdateCollections();

            return Collections.FirstOrDefault(p => p.Name.Equals(name));
        }

        public void RenameCollection(string oldName, string newName)
        {
            LiteDatabase.RenameCollection(oldName, newName);
            var item = Collections.FirstOrDefault(p => p.Name.Equals(oldName, StringComparison.Ordinal));
            if (item != null)
            {
                item.Name = newName;
            }
            var lookupItem = CollectionsLookup.FirstOrDefault(p => p.Name.Equals(oldName, StringComparison.Ordinal));
            if (lookupItem != null)
            {
                lookupItem.Name = newName;
            }
            // UpdateCollections();
        }

        public void DropCollection(string name)
        {
            LiteDatabase.DropCollection(name);
            var item = Collections.FirstOrDefault(p => p.Name.Equals(name, StringComparison.Ordinal));
            if (item != null)
            {
                Collections.Remove(item);
            }
            // UpdateCollections();
        }

        public bool ContainsCollection(string name)
        {
            return Collections.Any(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public static bool IsDbPasswordProtected(string path)
        {
            using (var db = new LiteDBv4.LiteDatabase(path))
            {
                try
                {
                    db.GetCollectionNames();
                    return false;
                }
                catch (LiteDBv4.LiteException e)
                {
                    if (e.ErrorCode == LiteDBv4.LiteException.DATABASE_WRONG_PASSWORD || e.Message.Contains("password"))
                    {
                        return true;
                    }

                    throw;
                }
            }
        }

        public void Refresh()
        {
            UpdateCollections();
        }
    }
}