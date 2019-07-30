using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Runtime.CompilerServices;
using Caliburn.Micro;
using Forge.Forms;
using Forge.Forms.Annotations;
using JetBrains.Annotations;
using LiteDbExplorer.Core;
using LiteDbExplorer.Modules.Shared;
using LiteDbExplorer.Wpf.Framework;
using MaterialDesignThemes.Wpf;

namespace LiteDbExplorer.Modules.ImportData
{
    [Export(typeof(ImportDataWizardViewModel))]
    public class ImportDataWizardViewModel : Screen, INavigationTarget<ImportDataOptions>
    {
        public ImportDataWizardViewModel()
        {
            DisplayName = "Import Wizard";

            FormatSelector = new ImportFormatSelector();

            TargetSelector = new ImportTargetOptions();
        }

        public void Init(ImportDataOptions modelParams)
        {
            
        }

        public ImportFormatSelector FormatSelector { get; set; }
        public ImportTargetOptions TargetSelector { get; set; }
    }

    public class ImportFormatSelector
    {
        public ImportFormatSelector()
        {
            DataImportHandlers = IoC.GetAll<IDataImportHandler>()
                .OrderBy(p => p.DisplayOrder)
                .ThenBy(p => p.DisplayName);
        }

        public IEnumerable<IDataImportHandler> DataImportHandlers { get; }

        [Value(Must.NotBeEmpty)]
        [SelectFrom("{Binding DataImportHandlers}", SelectionType = SelectionType.RadioButtons, DisplayPath = nameof(IDataImportHandler.DisplayName))]
        public IDataImportHandler ImportFormat { get; set; }
    }

    public enum RecordInsertMode
    {
        [EnumDisplay("Insert with new _id if exists")]
        Opt1,

        [EnumDisplay("Overwrite documents with some _id")]
        Opt2,

        [EnumDisplay("Skip documents with some _id")]
        Opt3,

        [EnumDisplay("Always insert with new _id")]
        Opt4,

        [EnumDisplay("Drop collection first if already exists")]
        Opt5,

        [EnumDisplay("Abort if id already exists")]
        Opt6,
    }

    public class ImportTargetOptions
    {
        public ImportTargetOptions()
        {
            Databases = Store.Current.Databases;
        }

        public IEnumerable<DatabaseReference> Databases { get; }

        [Value(Must.NotBeEmpty)]
        [SelectFrom("{Binding Databases}", SelectionType = SelectionType.ComboBox, DisplayPath = nameof(DatabaseReference.Name))]
        public DatabaseReference TargetDatabase { get; set; }

        [Value(Must.NotBeEmpty)]
        [SelectFrom("{Binding TargetDatabase.Collections}", SelectionType = SelectionType.ComboBox, DisplayPath = nameof(CollectionReference.Name))]
        public CollectionReference TargetCollection { get; set; }

        [SelectFrom(typeof(RecordInsertMode))]
        public RecordInsertMode InsertMode { get; set; }
    }

    public interface IDataImportHandler
    {
        string DisplayName { get; }
        int DisplayOrder { get; }
        object OptionsSource { get; }
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

    [Export(typeof(IDataImportHandler))]
    public class JsonDataImportHandler : IDataImportHandler
    {
        private readonly Options _options;

        [ImportingConstructor]
        public JsonDataImportHandler(Lazy<IApplicationInteraction> lazyApplicationInteraction)
        {
            _options = new Options(lazyApplicationInteraction);
        }

        public string DisplayName => "JSON";

        public int DisplayOrder => 10;

        public object OptionsSource => _options;

        public class Options : ImportSourceFileOptions
        {
            public Options(Lazy<IApplicationInteraction> lazyApplicationInteraction) : base(lazyApplicationInteraction)
            {
            }

            protected override (string title, string filter) GetFileFilter()
            {
                return ("Open JSON File", "Json File|*.json");
            }
        }

    }

    [Export(typeof(IDataImportHandler))]
    public class ExcelDataImportHandler : IDataImportHandler
    {
        private readonly Options _options;

        [ImportingConstructor]
        public ExcelDataImportHandler(Lazy<IApplicationInteraction> lazyApplicationInteraction)
        {
            _options = new Options(lazyApplicationInteraction);
        }

        public string DisplayName => "Excel";

        public int DisplayOrder => 20;

        public object OptionsSource => _options;

        public class Options : ImportSourceFileOptions
        {
            public Options(Lazy<IApplicationInteraction> lazyApplicationInteraction) : base(lazyApplicationInteraction)
            {
            }

            protected override (string title, string filter) GetFileFilter()
            {
                return ("Open Excel File", "Excel File|*.xlsx");
            }
        }
    }

    [Export(typeof(IDataImportHandler))]
    public class CsvDataImportHandler : IDataImportHandler
    {
        private readonly Options _options;

        [ImportingConstructor]
        public CsvDataImportHandler(Lazy<IApplicationInteraction> lazyApplicationInteraction)
        {
            _options = new Options(lazyApplicationInteraction);
        }

        public string DisplayName => "CSV";

        public int DisplayOrder => 30;

        public object OptionsSource => _options;

        public class Options : ImportSourceFileOptions
        {
            public Options(Lazy<IApplicationInteraction> lazyApplicationInteraction) : base(lazyApplicationInteraction)
            {
            }

            [Value(Must.NotBeEmpty)]
            [SelectFrom(new[] { "Comma (,)", "Semicolon (;)", "Tab", "Space", "Dot (.)", "Ampersand (&)", "Other" })]
            public string Delimiter { get; set; }

            public string OtherDelimiter { get; set; }

            public int SkipFirstLines { get; set; }

            public bool FileContainsHeaders { get; set; }

            protected override (string title, string filter) GetFileFilter()
            {
                return ("Open CSV File", "CSV File|*.csv");
            }
        }
    }

    [Export(typeof(IDataImportHandler))]
    public class OtherCollectionDataImportHandler : IDataImportHandler
    {
        private readonly Options _options;

        [ImportingConstructor]
        public OtherCollectionDataImportHandler()
        {
            _options = new Options(Store.Current.Databases);
        }

        public string DisplayName => "Another Collection";

        public int DisplayOrder => 100;

        public object OptionsSource => _options;

        public class Options
        {
            public Options(IEnumerable<DatabaseReference> databases)
            {
                Databases = databases;
            }

            public IEnumerable<DatabaseReference> Databases { get; }

            [Value(Must.NotBeEmpty)]
            [SelectFrom("{Binding Databases}", SelectionType = SelectionType.ComboBox, DisplayPath = nameof(DatabaseReference.Name))]
            public DatabaseReference SourceDatabase { get; set; }

            [Value(Must.NotBeEmpty)]
            [SelectFrom("{Binding SourceDatabase.Collections}", SelectionType = SelectionType.ComboBox, DisplayPath = nameof(CollectionReference.Name))]
            public CollectionReference SourceCollection { get; set; }
        }
    }
}