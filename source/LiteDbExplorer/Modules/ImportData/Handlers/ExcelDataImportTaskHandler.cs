using System;
using System.ComponentModel.Composition;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Caliburn.Micro;
using CSharpFunctionalExtensions;
using Forge.Forms;
using Forge.Forms.Annotations;
using LiteDbExplorer.Controls;
using LiteDbExplorer.Modules.Shared;

namespace LiteDbExplorer.Modules.ImportData.Handlers
{
    [Form(Mode = DefaultFields.None)]
    [Export(typeof(IDataImportTaskHandler))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class ExcelDataImportTaskHandler : Screen, IDataImportTaskHandler, IStepsScreen, IOwnerViewLocator
    {
        private readonly SourceOptions _sourceOptions;
        private readonly TargetOptions _targetOptions;
        private readonly DataPreviewHolder _dataPreviewHolder;

        [ImportingConstructor]
        public ExcelDataImportTaskHandler(Lazy<IApplicationInteraction> lazyApplicationInteraction)
        {
            DisplayName = "Import Excel Options";

            _sourceOptions = new SourceOptions(this, lazyApplicationInteraction);
            _targetOptions = new TargetOptions();
            _dataPreviewHolder = new DataPreviewHolder();
        }

        public string HandlerDisplayName => "Excel";

        public int HandlerDisplayOrder => 20;

        public bool IsBusy { get; private set; }

        public object SourceOptionsContext => _sourceOptions;

        public object TargetOptionsContext => _targetOptions;

        public object DataPreview => _dataPreviewHolder;

        public bool HasNext => true;

        public object Next()
        {
            return IoC.Get<DocumentMapperViewModel>();
        }

        public bool Validate()
        {
            return ModelState.Validate(SourceOptionsContext) && ModelState.Validate(TargetOptionsContext);
        }

        public UIElement GetOwnView(object context)
        {
            return new DynamicFormStackView(SourceOptionsContext, TargetOptionsContext, DataPreview);
        }

        public async Task ProcessFile(Maybe<string> maybeFilePath)
        {
            if (maybeFilePath.HasNoValue)
            {
                _dataPreviewHolder.SetContent("No Preview");
                return;
            }

            IsBusy = true;
            
            var dataTable = await Task.Factory.StartNew(() => ToDataTable(maybeFilePath.Value, _sourceOptions));

            var dataPreview = new DataGrid
            {
                ItemsSource = dataTable.DefaultView,
                AutoGenerateColumns = true,
                CanUserAddRows = false,
                IsReadOnly = true,
                SelectionUnit = DataGridSelectionUnit.FullRow
            };

            _dataPreviewHolder.SetContent(dataPreview);

            IsBusy = false;
        }

        public static DataTable ToDataTable(string path, SourceOptions options)
        {
            var hasHeader = options.FileContainsHeaders;
            using (var pck = new OfficeOpenXml.ExcelPackage())
            {
                using (var stream = File.OpenRead(path))
                {
                    pck.Load(stream);
                }
                var ws = pck.Workbook.Worksheets.First();  
                DataTable tbl = new DataTable();
                foreach (var firstRowCell in ws.Cells[1, 1, 1, ws.Dimension.End.Column])
                {
                    tbl.Columns.Add(hasHeader ? firstRowCell.Text : $"Column {firstRowCell.Start.Column}");
                }
                var startRow = hasHeader ? 2 : 1;
                for (int rowNum = startRow; rowNum <= ws.Dimension.End.Row; rowNum++)
                {
                    var wsRow = ws.Cells[rowNum, 1, rowNum, ws.Dimension.End.Column];
                    DataRow row = tbl.Rows.Add();
                    foreach (var cell in wsRow)
                    {
                        row[cell.Start.Column - 1] = cell.Text;
                    }
                }
                return tbl;
            }
        }

        [Heading("Source Options")]
        public class SourceOptions : ImportSourceFileOptions
        {
            private readonly ExcelDataImportTaskHandler _handler;

            public SourceOptions(ExcelDataImportTaskHandler handler, Lazy<IApplicationInteraction> lazyApplicationInteraction) : 
                base(lazyApplicationInteraction)
            {
                _handler = handler;
            }

            [Field(Row = "1")]
            public bool FileContainsHeaders { get; set; } = true;

            protected override (string title, string filter) GetFileFilter()
            {
                return ("Open Excel File", "Excel File|*.xlsx");
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
            [Field]
            [DirectContent]
            public ViewContentProxy DataPreview { get; set; } = new ViewContentProxy("No Preview") { Margin = new Thickness(10,0,10,0) };

            public void SetContent(object content)
            {
                DataPreview.Content = content;
            }
        }
    }
}