using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Caliburn.Micro;
using CSharpFunctionalExtensions;
using Enterwell.Clients.Wpf.Notifications;
using JetBrains.Annotations;
using LiteDbExplorer.Core;
using LiteDbExplorer.Framework;
using LiteDbExplorer.Modules.DbDocument;
using LiteDbExplorer.Presentation;
using LiteDbExplorer.Wpf.Framework;
using LiteDbExplorer.Wpf.Framework.Shell;
using MaterialDesignThemes.Wpf;
using Serilog;

namespace LiteDbExplorer.Modules.DbCollection
{
    [Export(typeof(CollectionExplorerViewModel))]
    [PartCreationPolicy (CreationPolicy.NonShared)]
    public class CollectionExplorerViewModel : DocumentConductor<CollectionReferencePayload, IDocumentPreview>, INavigationTarget<CollectionReferencePayload>
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IApplicationInteraction _applicationInteraction;
        private readonly IDatabaseInteractions _databaseInteractions;
        private DocumentReference _selectedDocument;
        private ICollectionReferenceListView _view;
        private bool _showDocumentPreview = true;
        private CollectionReference _collectionReference;

        [ImportingConstructor]
        public CollectionExplorerViewModel(
            IEventAggregator eventAggregator, 
            IApplicationInteraction applicationInteraction,
            IDatabaseInteractions databaseInteractions)
        {
            _eventAggregator = eventAggregator;
            _applicationInteraction = applicationInteraction;
            _databaseInteractions = databaseInteractions;

            SplitOrientation = Properties.Settings.Default.CollectionExplorer_SplitOrientation;
            ShowDocumentPreview = Properties.Settings.Default.CollectionExplorer_ShowPreview;
            ContentMaxLength = Properties.Settings.Default.CollectionExplorer_ContentMaxLength;
            DoubleClickAction = Properties.Settings.Default.CollectionExplorer_DoubleClickAction;

            FindTextModel = new FindTextModel();

            ItemDoubleClickCommand = new RelayCommand<DocumentReference>(async doc=> await OnItemDoubleClick(doc));

            AddDocumentCommand = new RelayCommand(async _=> await AddDocument(), o => CanAddDocument());
            EditDocumentCommand = new RelayCommand(async _=> await EditDocument(), o => CanEditDocument());
            RemoveDocumentCommand = new RelayCommand(async _=> await RemoveDocument(), o => CanRemoveDocument());
            ExportDocumentCommand = new RelayCommand(async _=> await ExportDocument(), o => CanExportDocument());
            CopyDocumentCommand = new RelayCommand(async _=> await CopyDocument(), o => CanCopyDocument());
            PasteDocumentCommand = new RelayCommand(async _=> await PasteDocument(), o => CanPasteDocument());
            RefreshCollectionCommand = new RelayCommand(_=> RefreshCollection(), o => CanRefreshCollection());
            EditDbPropertiesCommand = new RelayCommand(_=> EditDbProperties(), o => CanEditDbProperties());
            FindCommand = new RelayCommand(_=> OpenFind(), o => CanOpenFind());
            FindNextCommand = new RelayCommand(_=> Find(), o => CanFind());
            FindPreviousCommand = new RelayCommand(_=> FindPrevious(), o => CanFind());
        }

        public CollectionItemDoubleClickAction DoubleClickAction { get; }

        public int ContentMaxLength { get; }
        
        public Orientation? SplitOrientation { get; }

        public RelayCommand<DocumentReference> ItemDoubleClickCommand { get; }

        public ICommand AddDocumentCommand { get; }

        public ICommand EditDocumentCommand { get; }

        public ICommand RemoveDocumentCommand { get; }

        public ICommand ExportDocumentCommand { get; }

        public ICommand CopyDocumentCommand { get; }

        public ICommand PasteDocumentCommand { get; }

        public ICommand RefreshCollectionCommand { get; }

        public ICommand EditDbPropertiesCommand { get; }

        public ICommand FindCommand { get; }

        public ICommand FindNextCommand { get; }

        public ICommand FindPreviousCommand { get; }

        public FindTextModel FindTextModel { get; }

        [UsedImplicitly]
        public CollectionReference CollectionReference
        {
            get => _collectionReference;
            private set
            {
                if (_collectionReference != null)
                {
                    _collectionReference.ReferenceChanged -= OnCollectionReferenceChanged;
                    _collectionReference.DocumentsCollectionChanged -= OnDocumentsCollectionChanged;
                }
                _collectionReference = value;
                if (_collectionReference != null)
                {
                    _collectionReference.ReferenceChanged += OnCollectionReferenceChanged;
                    _collectionReference.DocumentsCollectionChanged += OnDocumentsCollectionChanged;
                }
            }
        }
        
        [UsedImplicitly]
        public DocumentReference SelectedDocument
        {
            get => _selectedDocument;
            set
            {
                _selectedDocument = value;
                if (_showDocumentPreview)
                {
                    ActivateDocumentPreview();
                }
                else
                {
                    DeactivateDocumentPreview();
                }
            }
        }

        [UsedImplicitly]
        public IList<DocumentReference> SelectedDocuments { get; set; }

        [UsedImplicitly]
        public bool IsFindOpen { get; private set; }

        public bool ShowDocumentPreview
        {
            get => _showDocumentPreview;
            set
            {
                if (_showDocumentPreview == false && value)
                {
                    ActivateDocumentPreview();
                }
                _showDocumentPreview = value;
                if (_showDocumentPreview == false)
                {
                    DeactivateDocumentPreview();
                }
            }
        }

        [UsedImplicitly]
        public bool HideDocumentPreview => SelectedDocument == null || !ShowDocumentPreview;

        [UsedImplicitly]
        public string DocumentsCountInfo
        {
            get
            {
                if (CollectionReference?.Items == null)
                {
                    return "Collection is Null";
                }

                return CollectionReference.Items.Count == 1 ? "1 item" : $"{CollectionReference.Items.Count} items";
            }
        }

        [UsedImplicitly]
        public string SelectedDocumentsCountInfo 
        {
            get
            {
                if (SelectedDocuments == null)
                {
                    return string.Empty;
                }

                return SelectedDocuments.Count == 1 ? "1 selected item" : $"{SelectedDocuments.Count} selected items";
            }
        }

        public override void Init(CollectionReferencePayload value)
        {
            if (value == null)
            {
                TryClose(false);
                return;
            }

            Log.Debug("Init. {ViewModelName}, ReferenceId {ReferenceId}", nameof(CollectionExplorerViewModel), value.InstanceId);

            InstanceId = value.InstanceId;

            var collectionReference = value.CollectionReference;

            DisplayName = collectionReference.Name;

            if (collectionReference.Database != null)
            {
                GroupId = collectionReference.Database.InstanceId;
                GroupDisplayName = collectionReference.Database.Name;
            }
            
            IconContent = collectionReference is FileCollectionReference ? new PackIcon { Kind = PackIconKind.FileMultiple } : new PackIcon { Kind = PackIconKind.TableLarge, Height = 16 };
            
            CollectionReference = collectionReference;
        }

        protected override void OnViewLoaded(object view)
        {
            _view = view as ICollectionReferenceListView;

            if (_view != null)
            {
                _view.CollectionLoadedAction = () =>
                {
                    if (SelectedDocument != null)
                    {
                        _view?.ScrollIntoItem(SelectedDocument);
                    }

                    _view?.FocusListView();
                };
            }
        }
        
        protected override void OnDeactivate(bool close)
        {
            if (close)
            {
                Log.Debug("Deactivate {ViewModelName}, ReferenceId {ReferenceId}", nameof(CollectionExplorerViewModel), InstanceId);

                DeactivateItem(ActiveItem, true);

                SelectedDocuments = null;
                SelectedDocument = null;
                CollectionReference = null;
            }
        }

        protected void ActivateDocumentPreview()
        {
            if (ActiveItem == null)
            {
                ActiveItem = IoC.Get<IDocumentPreview>();
            }

            ActiveItem?.SetActiveDocument(_selectedDocument);
            ActivateItem(ActiveItem);
        }

        protected void DeactivateDocumentPreview()
        {
            DeactivateItem(ActiveItem, false);
        }
        
        #region Handles

        private void OnCollectionReferenceChanged(object sender, ReferenceChangedEventArgs<CollectionReference> e)
        {
            switch (e.Action)
            {
                case ReferenceNodeChangeAction.Remove:
                case ReferenceNodeChangeAction.Dispose:
                    TryClose();
                    break;
                case ReferenceNodeChangeAction.Update:
                case ReferenceNodeChangeAction.Add:
                    _view?.UpdateView(e.Reference);
                    break;
            }
        }

        private void OnDocumentsCollectionChanged(object sender, CollectionReferenceChangedEventArgs<DocumentReference> e)
        {
            if (e.Action == ReferenceNodeChangeAction.Add)
            {
                SelectedDocument = e.Items.FirstOrDefault() ?? SelectedDocument;
                _view?.UpdateView(SelectedDocument);
            }

            if (e.Action == ReferenceNodeChangeAction.Update)
            {
                _view?.UpdateView(SelectedDocument);
            }
        }
        
        protected async Task OnItemDoubleClick(DocumentReference documentReference)
        {
            if(documentReference == null)
            {
                return;
            }

            switch (DoubleClickAction)
            {
                case CollectionItemDoubleClickAction.EditDocument:
                    await _databaseInteractions.OpenEditDocument(documentReference);
                    break;
                case CollectionItemDoubleClickAction.OpenPreview:
                    await _applicationInteraction.ActivateDefaultDocumentView(documentReference);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #endregion
        
        #region Routed Commands
        
        [UsedImplicitly]
        public async Task AddDocument()
        {
            await _databaseInteractions.CreateItem(CollectionReference)
                .OnSuccess(async reference =>
                {
                    await _applicationInteraction.ActivateDefaultCollectionView(reference.CollectionReference, reference.Items);
                    _eventAggregator.PublishOnUIThread(reference);
                });
        }

        [UsedImplicitly]
        public bool CanAddDocument()
        {
            return CollectionReference != null;
        }

        [UsedImplicitly]
        public async Task EditDocument()
        {
            await _databaseInteractions.OpenEditDocument(SelectedDocument);
        }

        [UsedImplicitly]
        public bool CanEditDocument()
        {
            return SelectedDocument != null;
        }

        [UsedImplicitly]
        public async Task RemoveDocument()
        {
            await _databaseInteractions.RemoveDocuments(SelectedDocuments);
        }

        [UsedImplicitly]
        public bool CanRemoveDocument()
        {
            return SelectedDocuments.HasAnyDocumentsReference();
        }

        [UsedImplicitly]
        public async Task ExportDocument()
        {
            var maybeFileName = await _databaseInteractions.ExportExcel(CollectionReference.Items, CollectionReference.Name);
            if (maybeFileName.HasValue)
            {
                NotificationInteraction.Default()
                    .HasMessage($"Export saved in:\n{maybeFileName.Value.ShrinkPath(128)}")
                    .Dismiss().WithButton("Open", button =>
                    {
                        _applicationInteraction.OpenFileWithAssociatedApplication(maybeFileName.Value);
                    })
                    .WithButton("Reveal in Explorer", button =>
                    {
                        _applicationInteraction.RevealInExplorer(maybeFileName.Value);
                    })
                    .Dismiss().WithButton("Close", button => { })
                    .Queue();
            }

            // await _databaseInteractions.ExportDocuments(SelectedDocuments.ToList());
        }

        [UsedImplicitly]
        public bool CanExportDocument()
        {
            return SelectedDocuments.HasAnyDocumentsReference();
        }

        [UsedImplicitly]
        public async Task CopyDocument()
        {
            await _databaseInteractions.CopyDocuments(SelectedDocuments);
        }

        [UsedImplicitly]
        public bool CanCopyDocument()
        {
            return SelectedDocuments.HasAnyDocumentsReference();
        }

        [UsedImplicitly]
        public async Task PasteDocument()
        {
            var textData = Clipboard.GetText();

            await _databaseInteractions
                .ImportDataFromText(CollectionReference, textData)
                .OnSuccess(update => _eventAggregator.PublishOnUIThread(update));
        }

        [UsedImplicitly]
        public bool CanPasteDocument()
        {
            return !CollectionReference.IsFilesCollection();
        }

        [UsedImplicitly]
        public void RefreshCollection()
        {
            CollectionReference?.Refresh();
        }

        [UsedImplicitly]
        public bool CanRefreshCollection()
        {
            return CollectionReference != null;
        }

        [UsedImplicitly]
        public void EditDbProperties()
        {
            _applicationInteraction.OpenDatabaseProperties(CollectionReference.Database);
        }

        [UsedImplicitly]
        public bool CanEditDbProperties()
        {
            return CollectionReference?.Database != null;
        }

        [UsedImplicitly]
        public void OpenFind()
        {
            IsFindOpen = true;
        }

        [UsedImplicitly]
        public void Find()
        {
            IsFindOpen = true;

            _view?.Find(FindTextModel.Text, FindTextModel.MatchCase);
        }

        [UsedImplicitly]
        public void FindPrevious()
        {
            IsFindOpen = true;

            _view?.FindPrevious(FindTextModel.Text, FindTextModel.MatchCase);
        }

        [UsedImplicitly]
        public void CloseFind()
        {
            IsFindOpen = false;
        }

        [UsedImplicitly]
        public bool CanOpenFind()
        {
            return CollectionReference != null;
        }

        [UsedImplicitly]
        public bool CanFind()
        {
            return CollectionReference != null && IsFindOpen;
        }

        #endregion
    }

    public class FindTextModel : INotifyPropertyChanged
    {
        public string Text { get; set; }
        
        public bool MatchCase { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        [UsedImplicitly]
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
