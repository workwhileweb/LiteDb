using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Caliburn.Micro;
using CSharpFunctionalExtensions;
using Enterwell.Clients.Wpf.Notifications;
using JetBrains.Annotations;
using LiteDbExplorer.Core;
using LiteDbExplorer.Framework;
using LiteDbExplorer.Modules.DbCollection;
using LiteDbExplorer.Modules.DbQuery;
using LiteDbExplorer.Modules.Shared;
using LiteDbExplorer.Presentation;

namespace LiteDbExplorer.Modules.Database
{
    [Export(typeof(IDocumentExplorer))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class DatabasesExplorerViewModel : Screen, IDocumentExplorer
    {
        private readonly IDatabaseInteractions _databaseInteractions;
        private readonly IApplicationInteraction _applicationInteraction;

        [ImportingConstructor]
        public DatabasesExplorerViewModel(
            IDatabaseInteractions databaseInteractions,
            IApplicationInteraction applicationInteraction)
        {
            _databaseInteractions = databaseInteractions;
            _applicationInteraction = applicationInteraction;

            PathDefinitions = databaseInteractions.PathDefinitions;

            OpenRecentItemCommand = new RelayCommand<RecentFileInfo>(async info => await OpenRecentItem(info));
            ItemDoubleClickCommand = new RelayCommand<CollectionReference>(NodeDoubleClick);

            SaveDatabaseCopyAsCommand = new RelayCommand(async _ => await SaveDatabaseCopyAs(), o => CanSaveDatabaseCopyAs());
            CloseDatabaseCommand = new RelayCommand(async _ => await CloseDatabase(), o => CanCloseDatabase());
            AddFileCommand = new RelayCommand(async _ => await AddFile(), _ => CanAddFile());
            AddCollectionCommand = new RelayCommand(async _ => await AddCollection(), _ => CanAddCollection());
            RefreshDatabaseCommand = new RelayCommand(_ => RefreshDatabase(), _ => CanRefreshDatabase());
            RevealInExplorerCommand = new RelayCommand(async _ => await RevealInExplorer(), _ => CanRevealInExplorer());
            RenameCollectionCommand = new RelayCommand(async _ => await RenameCollection(), _ => CanRenameCollection());
            DropCollectionCommand = new RelayCommand(async _ => await DropCollection(), _ => CanDropCollection());
            ExportCollectionCommand = new RelayCommand(async _ => await ExportCollection(), _ => CanExportCollection());
            EditDbPropertiesCommand = new RelayCommand(_ => EditDbProperties(), _ => CanEditDbProperties());
        }

        public Paths PathDefinitions { get; }

        public ICommand OpenRecentItemCommand { get; }

        public ICommand ItemDoubleClickCommand { get; }

        public ICommand CloseDatabaseCommand { get; }

        public ICommand AddFileCommand { get; }

        public ICommand AddCollectionCommand { get; }

        public ICommand RefreshDatabaseCommand { get; }

        public ICommand RevealInExplorerCommand { get; }

        public ICommand RenameCollectionCommand { get; }

        public ICommand DropCollectionCommand { get; }

        public ICommand ExportCollectionCommand { get; }

        public ICommand EditDbPropertiesCommand { get; }

        public ICommand SaveDatabaseCopyAsCommand { get; }

        [UsedImplicitly]
        public ObservableCollection<DatabaseReference> Databases => Store.Current.Databases;

        public DatabaseReference SelectedDatabase { get; private set; }

        public CollectionReference SelectedCollection { get; private set; }

        [UsedImplicitly]
        public async Task OpenDatabase()
        {
            await _databaseInteractions.OpenDatabase();
        }

        [UsedImplicitly]
        public async Task OpenRecentItem(RecentFileInfo info)
        {
            if (info == null)
            {
                return;
            }

            await _databaseInteractions.OpenDatabase(info.FullPath);
        }

        [UsedImplicitly]
        public async Task OpenDatabases(IEnumerable<string> paths)
        {
            await _databaseInteractions.OpenDatabases(paths);
        }

        [UsedImplicitly]
        public void OnSelectedItemChanged(RoutedPropertyChangedEventArgs<object> e)
        {
            var value = e.NewValue as IReferenceNode;

            switch (value)
            {
                case null:
                    SelectedDatabase = null;
                    SelectedCollection = null;
                    break;
                case CollectionReference collection:
                    SelectedDatabase = collection.Database;
                    SelectedCollection = collection;
                    break;
                case DatabaseReference reference:
                {
                    SelectedDatabase = reference;
                    if (SelectedCollection != null && SelectedCollection.Database != SelectedDatabase)
                    {
                        SelectedCollection = null;
                    }

                    break;
                }

                default:
                    SelectedDatabase = null;
                    SelectedCollection = null;
                    break;
            }
        }

        public void NodeDoubleClick(CollectionReference value)
        {
            var documentSet = IoC.Get<IDocumentSet>();
            documentSet.OpenDocument<CollectionExplorerViewModel, CollectionReferencePayload>(new CollectionReferencePayload(value));
        }

        [UsedImplicitly]
        public async Task NewQuery(object item)
        {
            switch (item)
            {
                case DatabaseReference databaseReference:
                    await _applicationInteraction.OpenQuery(new RunQueryContext(databaseReference));
                    break;
                case CollectionReference collectionReference:
                    await _applicationInteraction.OpenQuery(new RunQueryContext(collectionReference.Database, QueryReference.Find(collectionReference)));
                    break;
            }
        }

        [UsedImplicitly]
        public void NewFindQuery(CollectionReference item, int? skip = null, int? limit = null)
        {
            var queryReference = QueryReference.Find(item, skip, limit);

            _applicationInteraction.OpenQuery(new RunQueryContext(item.Database, queryReference) { RunOnStart = true });
        }

        #region Routed Commands

        [UsedImplicitly]
        public async Task SaveDatabaseCopyAs()
        {
            var newDatabasePath = await _databaseInteractions.SaveDatabaseCopyAs(SelectedDatabase);
            if (newDatabasePath.HasValue)
            {
                NotificationInteraction.Default()
                    .HasMessage($"Database copy saved in:\n{newDatabasePath.Value.ShrinkPath(128)}")
                    .Dismiss().WithButton("Open", async button =>
                    {
                        await _databaseInteractions.OpenDatabase(newDatabasePath.Value).ConfigureAwait(false);
                    })
                    .WithButton("Reveal in Explorer", button =>
                    {
                        _applicationInteraction.RevealInExplorer(newDatabasePath.Value);
                        })
                    .Dismiss().WithButton("Close", button => { })
                    .Queue();
            }
        }

        public bool CanSaveDatabaseCopyAs()
        {
            return SelectedDatabase != null;
        }

        [UsedImplicitly]
        public async Task CloseDatabase()
        {
            await _databaseInteractions.CloseDatabase(SelectedDatabase);

            if (SelectedCollection?.Database == SelectedDatabase)
            {
                SelectedCollection = null;
            }

            SelectedDatabase = null;
        }

        [UsedImplicitly]
        public bool CanCloseDatabase()
        {
            return SelectedDatabase != null;
        }

        [UsedImplicitly]
        public async Task AddFile()
        {
            await _databaseInteractions.AddFileToDatabase(SelectedDatabase)
                .OnSuccess(async reference =>
                {
                    await _applicationInteraction.ActivateCollection(reference.CollectionReference, reference.Items);
                });
        }

        [UsedImplicitly]
        public bool CanAddFile()
        {
            return SelectedDatabase != null;
        }

        [UsedImplicitly]
        public async Task AddCollection()
        {
            await _databaseInteractions.AddCollection(SelectedDatabase)
                .OnSuccess(async reference =>
                {
                    await _applicationInteraction.ActivateCollection(reference);
                });
        }

        [UsedImplicitly]
        public bool CanAddCollection()
        {
            return SelectedDatabase != null;
        }

        [UsedImplicitly]
        public void RefreshDatabase()
        {
            SelectedDatabase?.Refresh();
        }

        [UsedImplicitly]
        public bool CanRefreshDatabase()
        {
            return SelectedDatabase != null;
        }

        [UsedImplicitly]
        public async Task RevealInExplorer()
        {
            await _databaseInteractions.RevealInExplorer(SelectedDatabase);
        }

        [UsedImplicitly]
        public bool CanRevealInExplorer()
        {
            return SelectedDatabase != null;
        }

        [UsedImplicitly]
        public async Task RenameCollection()
        {
            await _databaseInteractions.RenameCollection(SelectedCollection);
        }

        [UsedImplicitly]
        public bool CanRenameCollection()
        {
            return SelectedCollection != null && !SelectedCollection.IsFilesOrChunks;
        }

        [UsedImplicitly]
        public async Task DropCollection()
        {
            await _databaseInteractions.DropCollection(SelectedCollection);

            SelectedCollection = null;
        }

        [UsedImplicitly]
        public bool CanDropCollection()
        {
            return SelectedCollection != null;
        }

        [UsedImplicitly]
        public async Task ExportCollection()
        {
            await _databaseInteractions.ExportCollection(SelectedCollection);
        }

        [UsedImplicitly]
        public bool CanExportCollection()
        {
            return SelectedCollection != null;
        }

        [UsedImplicitly]
        public void EditDbProperties()
        {
            _applicationInteraction.OpenDatabaseProperties(SelectedDatabase);
        }

        [UsedImplicitly]
        public bool CanEditDbProperties()
        {
            return SelectedDatabase != null;
        }

        #endregion
    }
}