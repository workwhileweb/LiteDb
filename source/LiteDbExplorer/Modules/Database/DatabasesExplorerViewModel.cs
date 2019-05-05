using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Input;
using Caliburn.Micro;
using CSharpFunctionalExtensions;
using JetBrains.Annotations;
using LiteDbExplorer.Framework;
using LiteDbExplorer.Modules.DbCollection;
using LiteDbExplorer.Modules.Main;

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

            OpenRecentItemCommand = new RelayCommand<RecentFileInfo>(OpenRecentItem);
            ItemDoubleClickCommand = new RelayCommand<CollectionReference>(NodeDoubleClick);

            CloseDatabaseCommand = new RelayCommand(_ => CloseDatabase(), o => CanCloseDatabase());
            AddFileCommand = new RelayCommand(_ => AddFile(), _ => CanAddFile());
            AddCollectionCommand = new RelayCommand(_ => AddCollection(), _ => CanAddCollection());
            RefreshDatabaseCommand = new RelayCommand(_ => RefreshDatabase(), _ => CanRefreshDatabase());
            RevealInExplorerCommand = new RelayCommand(_ => RevealInExplorer(), _ => CanRevealInExplorer());
            RenameCollectionCommand = new RelayCommand(_ => RenameCollection(), _ => CanRenameCollection());
            DropCollectionCommand = new RelayCommand(_ => DropCollection(), _ => CanDropCollection());
            ExportCollectionCommand = new RelayCommand(_ => ExportCollection(), _ => CanExportCollection());
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

        [UsedImplicitly]
        public void OpenDatabase()
        {
            _databaseInteractions.OpenDatabase();
        }

        [UsedImplicitly]
        public void OpenRecentItem(RecentFileInfo info)
        {
            if (info == null)
            {
                return;
            }

            _databaseInteractions.OpenDatabase(info.FullPath);
        }

        [UsedImplicitly]
        public void OpenDatabases(IEnumerable<string> paths)
        {
            _databaseInteractions.OpenDatabases(paths);
        }

        public DatabaseReference SelectedDatabase { get; private set; }

        public CollectionReference SelectedCollection { get; private set; }

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

            Store.Current.SelectDatabase(SelectedDatabase);
            Store.Current.SelectCollection(SelectedCollection);
        }

        public void NodeDoubleClick(CollectionReference value)
        {
            var documentSet = IoC.Get<IDocumentSet>();
            documentSet.OpenDocument<CollectionExplorerViewModel, CollectionReference>(value);
        }

        #region Routed Commands

        [UsedImplicitly]
        public void CloseDatabase()
        {
            _databaseInteractions.CloseDatabase(SelectedDatabase);

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
        public void AddFile()
        {
            _databaseInteractions.AddFileToDatabase(SelectedDatabase)
                .OnSuccess(reference =>
                {
                    _applicationInteraction.ActivateCollection(reference.CollectionReference, reference.Items);
                });
        }

        [UsedImplicitly]
        public bool CanAddFile()
        {
            return SelectedDatabase != null;
        }

        [UsedImplicitly]
        public void AddCollection()
        {
            _databaseInteractions.AddCollection(SelectedDatabase)
                .OnSuccess(reference => { _applicationInteraction.ActivateCollection(reference); });
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
        public void RevealInExplorer()
        {
            _databaseInteractions.RevealInExplorer(SelectedDatabase);
        }

        [UsedImplicitly]
        public bool CanRevealInExplorer()
        {
            return SelectedDatabase != null;
        }

        [UsedImplicitly]
        public void RenameCollection()
        {
            _databaseInteractions.RenameCollection(SelectedCollection);
        }

        [UsedImplicitly]
        public bool CanRenameCollection()
        {
            return SelectedCollection != null && !SelectedCollection.IsFilesOrChunks;
        }

        [UsedImplicitly]
        public void DropCollection()
        {
            _databaseInteractions.DropCollection(SelectedCollection);

            SelectedCollection = null;
        }

        [UsedImplicitly]
        public bool CanDropCollection()
        {
            return SelectedCollection != null;
        }

        [UsedImplicitly]
        public void ExportCollection()
        {
            _databaseInteractions.ExportCollection(SelectedCollection);
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