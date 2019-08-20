using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Caliburn.Micro;
using CSharpFunctionalExtensions;
using CsvHelper;
using CsvHelper.Configuration;
using Forge.Forms;
using Forge.Forms.Annotations;
using LiteDbExplorer.Controls;
using LiteDbExplorer.Modules.Shared;

namespace LiteDbExplorer.Modules.ImportData.Handlers
{
    [Form(Mode = DefaultFields.None)]
    [Export(typeof(IDataImportTaskHandler))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class CsvDataImportTaskHandler : Screen, IDataImportTaskHandler, IStepsScreen, IOwnerViewLocator
    {
        private readonly SourceOptions _sourceOptions;
        private readonly TargetOptions _targetOptions;
        private readonly DataPreviewHolder _dataPreviewHolder;

        [ImportingConstructor]
        public CsvDataImportTaskHandler(Lazy<IApplicationInteraction> lazyApplicationInteraction)
        {
            DisplayName = "Import CSV Options";

            _sourceOptions = new SourceOptions(this, lazyApplicationInteraction);
            _targetOptions = new TargetOptions();
            _dataPreviewHolder = new DataPreviewHolder();
        }

        public string HandlerDisplayName => "CSV";

        public int HandlerDisplayOrder => 30;

        public object SourceOptionsContext => _sourceOptions;

        public object TargetOptionsContext => _targetOptions;

        public object DataPreview => _dataPreviewHolder;

        public bool HasNext => true;

        public DataTable ItemsSource { get; private set; }

        public Task<object> Next()
        {
            var viewModel = IoC.Get<DocumentMapperViewModel>();

            IEnumerable<string> sourceFields = null;
            
            if (ItemsSource != null)
            {
                sourceFields = ItemsSource.Columns.OfType<DataColumn>().Select(p => p.ColumnName);
            }

            var collectionReference = _targetOptions.GetTargetCollection();

            viewModel.Init(sourceFields, collectionReference);

            return Task.FromResult<object>(viewModel);
        }

        public bool Validate()
        {
            return ModelState.Validate(SourceOptionsContext) && ModelState.Validate(TargetOptionsContext);
        }

        public UIElement GetOwnView(object context)
        {
            return new DynamicFormStackView(SourceOptionsContext, TargetOptionsContext, DataPreview);
        }

        public Task ProcessFile(Maybe<string> maybeFilePath)
        {
            ItemsSource = maybeFilePath.HasValue ? ToDataTable(maybeFilePath.Value, _sourceOptions) : null;

            _dataPreviewHolder.RefreshView(ItemsSource);

            return Task.CompletedTask;
        }

        public static DataTable ToDataTable(string path, SourceOptions options)
        {
            var configuration = new Configuration
            {
                Delimiter = options.Delimiter,
                HasHeaderRecord = options.FileContainsHeaders
            };

            using (var reader = new StreamReader(path))
            using (var csv = new CsvReader(reader, configuration))
            {
                // Do any configuration to `CsvReader` before creating CsvDataReader.
                using (var dr = new CsvDataReader(csv))
                {		
                    var dt = new DataTable();
                    dt.Load(dr);

                    return dt;
                }
            }
        }

        [Form(Grid = new[] { 1d, 1d })]
        [Heading("Source Options")]
        public class SourceOptions : ImportSourceFileOptions
        {
            private readonly CsvDataImportTaskHandler _handler;

            public SourceOptions(CsvDataImportTaskHandler handler, Lazy<IApplicationInteraction> lazyApplicationInteraction) 
                : base(lazyApplicationInteraction)
            {
                _handler = handler;
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
            public bool FileContainsHeaders { get; set; } = true;

            protected override (string title, string filter) GetFileFilter()
            {
                return ("Open CSV File", "CSV File|*.csv");
            }

            protected override Task OnFileOpen(Maybe<string> maybeFilePath)
            {
                return _handler.ProcessFile(maybeFilePath);
            }
        }

        [Heading("Target Options")]
        public class TargetOptions : ImportTargetDefaultOptions
        {
        }

        [Heading("Preview")]
        [Form(Mode = DefaultFields.None)]
        public class DataPreviewHolder : PropertyChangedBase
        {
            private readonly DataGrid _dataGrid;

            public DataPreviewHolder()
            {
                _dataGrid = new DataGrid
                {
                    AutoGenerateColumns = true,
                    CanUserAddRows = false,
                    IsReadOnly = true,
                    SelectionUnit = DataGridSelectionUnit.FullRow,
                    GridLinesVisibility = DataGridGridLinesVisibility.All,
                    MaxHeight = 200
                };

                DataPreview = new ViewContentProxy(_dataGrid);
            }

            [Field]
            [DirectContent]
            public ViewContentProxy DataPreview { get; set; }

            public void RefreshView(DataTable itemsSource)
            {
                _dataGrid.ItemsSource = itemsSource?.DefaultView;
            }
        }

    }
}