using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using Caliburn.Micro;
using Forge.Forms;
using Forge.Forms.Annotations;
using JetBrains.Annotations;
using LiteDB;
using LiteDbExplorer.Core;
using LiteDbExplorer.Modules.Shared;
using LiteDbExplorer.Wpf.Framework;
using MaterialDesignThemes.Wpf;
using ReactiveUI;

namespace LiteDbExplorer.Modules.ImportData
{
    [Export(typeof(ImportDataWizardViewModel))]
    public class ImportDataWizardViewModel : Conductor<IStepsScreen>.Collection.OneActive, INavigationTarget<ImportDataOptions>
    {
        private IDisposable _activeItemObservable;
        private bool _supressPreviousPush;

        public ImportDataWizardViewModel()
        {
            DisplayName = "Import Wizard";

            ActivateItem(IoC.Get<ImportFormatSelector>());
        }

        public void Init(ImportDataOptions modelParams)
        {
        }

        public Stack<IStepsScreen> PreviousItems { get; } = new Stack<IStepsScreen>();

        public bool CanNext => ActiveItem?.HasNext ?? false;

        public bool CanPrevious => PreviousItems.Count > 1;

        public override void ActivateItem(IStepsScreen item)
        {
            _activeItemObservable?.Dispose();

            if (!_supressPreviousPush)
            {
                PreviousItems.Push(ActiveItem);
            }
            
            base.ActivateItem(item);
            
            _activeItemObservable = item
                .ObservableForProperty(screen => screen.HasNext)
                .Subscribe(args => NotifyOfPropertyChange(nameof(CanNext)));

            NotifyOfPropertyChange(nameof(CanNext));
            NotifyOfPropertyChange(nameof(CanPrevious));
        }

        public override void DeactivateItem(IStepsScreen item, bool close)
        {
            base.DeactivateItem(item, close);

            PreviousItems.Push(item);

            NotifyOfPropertyChange(nameof(CanNext));
            NotifyOfPropertyChange(nameof(CanPrevious));
        }

        [UsedImplicitly]
        public void Next()
        {
            if (ActiveItem?.Next() is IStepsScreen next)
            {
                ActivateItem(next);
            }
        }

        [UsedImplicitly]
        public void Previous()
        {
            var previous = PreviousItems.Pop();
            if (previous != null)
            {
                _supressPreviousPush = true;
                ActivateItem(previous);
                _supressPreviousPush = false;
            }

            NotifyOfPropertyChange(nameof(CanNext));
            NotifyOfPropertyChange(nameof(CanPrevious));
        }
    }

    public interface IDataImportTaskHandler
    {
        string DisplayName { get; }
        int DisplayOrder { get; }
        object SourceOptionsContext { get; }
        object TargetOptionsContext { get; }
    }

    public interface IStepsScreen : INotifyPropertyChanged
    {
        bool HasNext { get; }
        object Next();
    }

    [Export(typeof(ImportFormatSelector))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    [Form(Mode = DefaultFields.None)]
    public class ImportFormatSelector : PropertyChangedBase, IStepsScreen, IOwnerViewLocator
    {
        public ImportFormatSelector()
        {
            DataImportHandlers = IoC.GetAll<IDataImportTaskHandler>()
                .OrderBy(p => p.DisplayOrder)
                .ThenBy(p => p.DisplayName);
        }

        public IEnumerable<IDataImportTaskHandler> DataImportHandlers { get; }

        [Field]
        [Value(Must.NotBeEmpty)]
        [SelectFrom("{Binding DataImportHandlers}", SelectionType = SelectionType.RadioButtons, DisplayPath = nameof(IDataImportTaskHandler.DisplayName))]
        public IDataImportTaskHandler ImportFormat { get; set; }

        public bool HasNext => ImportFormat != null;

        public object Next()
        {
            return ImportFormat;
        }

        public UIElement GetView(object context)
        {
            return new DynamicFormView(this);
        }
    }

    public enum RecordIdHandlerPolice
    {
        [EnumDisplay("Insert with new _id if exists")]
        InsertNewIfExists,

        [EnumDisplay("Overwrite documents with some _id")]
        OverwriteDocuments,

        [EnumDisplay("Skip documents with some _id")]
        SkipDocuments,

        [EnumDisplay("Always insert with new _id")]
        AlwaysInsertNew,

        [EnumDisplay("Drop collection first if already exists")]
        DropCollectionIfExists,

        [EnumDisplay("Abort if id already exists")]
        AbortIfExists,
    }

    public enum RecordNullOrEmptyHandlerPolice
    {
        [EnumDisplay("Import as Null")]
        ImportAsNull,

        [EnumDisplay("Import as Default Type Value")]
        ImportAsDefaultTypeValue,

        [EnumDisplay("Exclude")]
        Exclude,
    }

    [Form(Grid = new[] { 1d, 1d })]
    public class ImportTargetDefaultOptions
    {
        public ImportTargetDefaultOptions()
        {
            Databases = Store.Current.Databases;
        }

        public IEnumerable<DatabaseReference> Databases { get; }

        [Field(Row = "0")]
        [Value(Must.NotBeEmpty)]
        [SelectFrom("{Binding Databases}", SelectionType = SelectionType.ComboBox, DisplayPath = nameof(DatabaseReference.Name))]
        public DatabaseReference TargetDatabase { get; set; }

        [Field(Row = "0")]
        [Value(Must.NotBeEmpty)]
        [SelectFrom("{Binding TargetDatabase.Collections}", SelectionType = SelectionType.ComboBox, DisplayPath = nameof(CollectionReference.Name))]
        public CollectionReference TargetCollection { get; set; }
        
        [Break]

        [Field(Row = "1")]
        [SelectFrom(typeof(RecordIdHandlerPolice))]
        public RecordIdHandlerPolice InsertMode { get; set; }

        [Field(Row = "1")]
        [SelectFrom(typeof(RecordNullOrEmptyHandlerPolice))]
        public RecordNullOrEmptyHandlerPolice EmptyFieldsMode { get; set; }
    }

    public class ImportSourceFileOptions : IActionHandler, INotifyPropertyChanged
    {
        private readonly Lazy<IApplicationInteraction> _lazyApplicationInteraction;

        protected const string ACTION_OPEN_FILE = "open_file";

        public ImportSourceFileOptions(Lazy<IApplicationInteraction> lazyApplicationInteraction)
        {
            _lazyApplicationInteraction = lazyApplicationInteraction;
        }

        [Field(IsReadOnly = true)]
        [Value(Must.NotBeEmpty)]
        [Action(ACTION_OPEN_FILE, "Open", Placement = Placement.Inline, Icon = PackIconKind.FolderOpen)]
        public string SourceFile { get; set; }

        public async void HandleAction(IActionContext actionContext)
        {
            var action = actionContext.Action;
            switch (action)
            {
                case ACTION_OPEN_FILE:
                    var (title, filter) = GetFileFilter();
                    var maybeFilePath = await _lazyApplicationInteraction.Value.ShowOpenFileDialog(title, filter);
                    if (maybeFilePath.HasValue)
                    {
                        SourceFile = maybeFilePath.Value;
                    }
                    break;
            }
        }

        protected virtual (string title, string filter) GetFileFilter()
        {
            return ("All Files", "All Files|*.*");
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [UsedImplicitly]
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    [Export(typeof(IDataImportTaskHandler))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    [Form(Mode = DefaultFields.None)]
    public class JsonDataImportTaskHandler : PropertyChangedBase, IDataImportTaskHandler, IStepsScreen, IOwnerViewLocator
    {
        private readonly SourceOptions _sourceOptions;
        private readonly TargetOptions _targetOptions;

        [ImportingConstructor]
        public JsonDataImportTaskHandler(Lazy<IApplicationInteraction> lazyApplicationInteraction)
        {
            _sourceOptions = new SourceOptions(lazyApplicationInteraction);
            _targetOptions = new TargetOptions();
        }

        public string DisplayName => "JSON";

        public int DisplayOrder => 10;

        [Field]
        public object SourceOptionsContext => _sourceOptions;

        [Field]
        public object TargetOptionsContext => _targetOptions;

        [Heading("Source Options")]
        public class SourceOptions : ImportSourceFileOptions
        {
            public SourceOptions(Lazy<IApplicationInteraction> lazyApplicationInteraction) : base(lazyApplicationInteraction)
            {
            }

            protected override (string title, string filter) GetFileFilter()
            {
                return ("Open JSON File", "Json File|*.json");
            }
        }

        [Heading("Target Options")]
        public class TargetOptions : ImportTargetDefaultOptions
        {
            /*private readonly DocumentMapperViewModel _documentMapper;

            public TargetOptions()
            {
                _documentMapper = IoC.Get<DocumentMapperViewModel>();
            }

            [Field]
            [DirectContent]
            public ViewModelContentProxy Map => new ViewModelContentProxy(_documentMapper);*/
        }

        public bool HasNext => true;

        public object Next()
        {
            return IoC.Get<DocumentMapperViewModel>();
        }

        public UIElement GetView(object context)
        {
            return new ImportDynamicFormOptionsView();
        }
    }

    [Export(typeof(IDataImportTaskHandler))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class ExcelDataImportTaskHandler : IDataImportTaskHandler
    {
        private readonly SourceOptions _sourceOptions;
        private readonly TargetOptions _targetOptions;

        [ImportingConstructor]
        public ExcelDataImportTaskHandler(Lazy<IApplicationInteraction> lazyApplicationInteraction)
        {
            _sourceOptions = new SourceOptions(lazyApplicationInteraction);
            _targetOptions = new TargetOptions();
        }

        public string DisplayName => "Excel";

        public int DisplayOrder => 20;

        public object SourceOptionsContext => _sourceOptions;

        public object TargetOptionsContext => _targetOptions;

        [Heading("Source Options")]
        public class SourceOptions : ImportSourceFileOptions
        {
            public SourceOptions(Lazy<IApplicationInteraction> lazyApplicationInteraction) : base(lazyApplicationInteraction)
            {
            }

            protected override (string title, string filter) GetFileFilter()
            {
                return ("Open Excel File", "Excel File|*.xlsx");
            }
        }

        [Heading("Target Options")]
        public class TargetOptions : ImportTargetDefaultOptions
        {
        }
    }

    [Export(typeof(IDataImportTaskHandler))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class CsvDataImportTaskHandler : IDataImportTaskHandler
    {
        private readonly SourceOptions _sourceSourceOptions;
        private readonly TargetOptions _targetOptions;

        [ImportingConstructor]
        public CsvDataImportTaskHandler(Lazy<IApplicationInteraction> lazyApplicationInteraction)
        {
            _sourceSourceOptions = new SourceOptions(lazyApplicationInteraction);
            _targetOptions = new TargetOptions();
        }

        public string DisplayName => "CSV";

        public int DisplayOrder => 30;

        public object SourceOptionsContext => _sourceSourceOptions;

        public object TargetOptionsContext => _targetOptions;

        [Form(Grid = new[] { 1d, 1d })]
        [Heading("Source Options")]
        public class SourceOptions : ImportSourceFileOptions
        {
            public SourceOptions(Lazy<IApplicationInteraction> lazyApplicationInteraction) : base(lazyApplicationInteraction)
            {
                Delimiter = DelimiterOptions.Keys.FirstOrDefault();
            }

            public IReadOnlyDictionary<string, string> DelimiterOptions => new Dictionary<string, string>
            {
                {",","Comma (,)"},
                {";","Semicolon (;)"},
                {"\t","Tab"},
                {" ","Space"},
                {".","Dot (.)"},
                {"&","Ampersand (&)"},
                {"", "Other"}
            };

            [Field(Row = "0")]
            [Value(Must.NotBeNull)]
            [SelectFrom("{Binding DelimiterOptions}", DisplayPath = "Value", ValuePath = "Key")]
            public string Delimiter { get; set; }

            [Field(Row = "0", IsVisible = "{Binding Delimiter|IsEmpty}")]
            public string OtherDelimiter { get; set; }

            [Field(Row = "1")]
            public int SkipFirstLines { get; set; }

            [Field(Row = "1")]
            public bool FileContainsHeaders { get; set; }

            protected override (string title, string filter) GetFileFilter()
            {
                return ("Open CSV File", "CSV File|*.csv");
            }
        }

        [Heading("Target Options")]
        public class TargetOptions : ImportTargetDefaultOptions
        {
        }
    }

    [Export(typeof(IDataImportTaskHandler))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class OtherCollectionDataImportTaskHandler : IDataImportTaskHandler
    {
        private readonly SourceOptions _sourceOptions;
        private readonly TargetOptions _targetOptions;

        [ImportingConstructor]
        public OtherCollectionDataImportTaskHandler()
        {
            _sourceOptions = new SourceOptions(Store.Current.Databases);
            _targetOptions = new TargetOptions();
        }

        public string DisplayName => "Another Collection";

        public int DisplayOrder => 100;

        public object SourceOptionsContext => _sourceOptions;

        public object TargetOptionsContext => _targetOptions;

        [Heading("Source Options")]
        [Form(Grid = new[] { 1d, 1d })]
        public class SourceOptions
        {
            public SourceOptions(IEnumerable<DatabaseReference> databases)
            {
                Databases = databases;
            }

            public IEnumerable<DatabaseReference> Databases { get; }

            [Field(Row = "0")]
            [Value(Must.NotBeEmpty)]
            [SelectFrom("{Binding Databases}", SelectionType = SelectionType.ComboBox, DisplayPath = nameof(DatabaseReference.Name))]
            public DatabaseReference SourceDatabase { get; set; }

            [Field(Row = "0")]
            [Value(Must.NotBeEmpty)]
            [SelectFrom("{Binding SourceDatabase.Collections}", SelectionType = SelectionType.ComboBox, DisplayPath = nameof(CollectionReference.Name))]
            public CollectionReference SourceCollection { get; set; }
        }

        [Heading("Target Options")]
        public class TargetOptions : ImportTargetDefaultOptions
        {
        }
    }

    public class DocumentKeyInfo
    {
        public DocumentKeyInfo()
        {
        }

        public DocumentKeyInfo(string name, BsonType bsonType, bool exists)
        {
            Name = name;
            BsonType = bsonType;
            Exists = exists;
        }

        public string Name { get; set; }

        public BsonType BsonType { get; set; }

        public bool Exists { get; set; }
    }

    public class DocumentToDocumentMap : INotifyPropertyChanged
    {
        public bool Active { get; set; }

        public string SourceKey { get; set; }

        public string TargetKey { get; set; }

        public BsonType? FieldType { get; set; }

        public bool SourceIsReadonly { get; set; }

        public bool SourceIsEditable => !SourceIsReadonly;

        public event PropertyChangedEventHandler PropertyChanged;

        [UsedImplicitly]
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    [Export(typeof(DocumentMapperViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class DocumentMapperViewModel : Screen, IStepsScreen
    {
        public DocumentMapperViewModel()
        {
            BsonTypes = Enum.GetValues(typeof(BsonType))
                .Cast<BsonType>()
                .Except(new[]{ BsonType.MinValue, BsonType.MaxValue });

            Init();
        }

        public IEnumerable<BsonType> BsonTypes { get; private set; }

        public IObservableCollection<string> SourceFields { get; private set; }

        public IObservableCollection<string> TargetFields { get; private set; }

        public IObservableCollection<DocumentToDocumentMap> MappingSet { get; private set; }

        private void Init()
        {
            SourceFields = new BindableCollection<string>
            {
                "_id",
                "Name",
                "Age",
                "IsEnable",
            };

            TargetFields = new BindableCollection<string>
            {
                "_id",
                "Name",
                "IsEnable",
            };

            var mappingSet = new BindableCollection<DocumentToDocumentMap>();
            // Fill from source
            foreach (var sourceField in SourceFields)
            {
                var documentMap = new DocumentToDocumentMap
                {
                    SourceKey = sourceField,
                    SourceIsReadonly = true
                };

                mappingSet.Add(documentMap);
            }

            // Fill from target
            foreach (var targetField in TargetFields)
            {
                var documentMap = mappingSet.FirstOrDefault(p => p.SourceKey.Equals(targetField, StringComparison.OrdinalIgnoreCase));
                if (documentMap != null)
                {
                    documentMap.TargetKey = targetField;
                }
                else
                {
                    documentMap = new DocumentToDocumentMap
                    {
                        TargetKey = targetField
                    };
                    mappingSet.Add(documentMap);
                }
            }

            foreach (var documentMap in mappingSet)
            {
                if (documentMap.SourceKey != null && documentMap.TargetKey != null)
                {
                    documentMap.Active = true;
                }
            }

            MappingSet = mappingSet;
        }

        public bool HasNext => false;

        public object Next()
        {
            return null;
        }
    }
}