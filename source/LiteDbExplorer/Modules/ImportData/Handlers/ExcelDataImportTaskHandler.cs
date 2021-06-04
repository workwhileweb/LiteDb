using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;
using Caliburn.Micro;
using CSharpFunctionalExtensions;
using Forge.Forms;
using Forge.Forms.Annotations;
using LiteDbExplorer.Modules.Shared;
using LiteDbExplorer.Presentation.Behaviors;

namespace LiteDbExplorer.Modules.ImportData.Handlers
{
    [Form(Mode = DefaultFields.None)]
    [Export(typeof(IDataImportTaskHandler))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class ExcelDataImportTaskHandler : Screen, IDataImportTaskHandler, IStepsScreen, IOwnerViewLocator
    {
        private readonly SourceOptions _sourceOptions;

        [ImportingConstructor]
        public ExcelDataImportTaskHandler(Lazy<IApplicationInteraction> lazyApplicationInteraction)
        {
            DisplayName = "Import Excel Options";

            _sourceOptions = new SourceOptions(lazyApplicationInteraction);
        }

        public string HandlerDisplayName => "Excel";

        public int HandlerDisplayOrder => 20;

        public object SourceOptionsContext => _sourceOptions;

        public bool HasNext => true;

        public bool CanContentScroll => false;

        public Task<object> Next()
        {
            return _sourceOptions.Next();
        }

        public bool Validate()
        {
            return ModelState.Validate(SourceOptionsContext);
        }

        public UIElement GetOwnView(object context)
        {
            return _sourceOptions.GetOwnView(context);
        }

        [Heading("Source Options")]
        public class SourceOptions : ImportSourceFromFileScreen, IStepsScreen, IOwnerViewLocator
        {
            private readonly DataGrid _dataGrid;

            public SourceOptions(Lazy<IApplicationInteraction> lazyApplicationInteraction) : 
                base(lazyApplicationInteraction)
            {
                _dataGrid = new DataGrid
                {
                    AutoGenerateColumns = true,
                    CanUserAddRows = false,
                    IsReadOnly = true,
                    SelectionUnit = DataGridSelectionUnit.FullRow,
                    GridLinesVisibility = DataGridGridLinesVisibility.All,
                    EnableRowVirtualization = true,
                    EnableColumnVirtualization = true
                };

                Interaction.GetBehaviors(_dataGrid).Add(new EscapeAccessKeyColumnHeaderBehavior());
            }

            public bool CanContentScroll => false;

            public bool HasNext => true;

            [Field(Row = "1")]
            public bool FileContainsHeaders { get; set; } = true;

            public object DataPreview => _dataGrid;

            public DataTable ItemsSource { get; private set; }

            protected override (string title, string filter) GetFileFilter()
            {
                return ("Open Excel File", "Excel File|*.xlsx");
            }

            protected override Task OnFileOpen(Maybe<string> maybeFilePath)
            {
                return ProcessFile(maybeFilePath);
            }

            public Task<object> Next()
            {
                var targetOptions = new TargetOptions(this);
                return Task.FromResult<object>(targetOptions);
            }

            public bool Validate()
            {
                return ModelState.Validate(this);
            }

            public UIElement GetOwnView(object context)
            {
                return new DynamicFormGrid(
                    new DynamicFormGrid.Item(this, GridLength.Auto),
                    new DynamicFormGrid.Item(DataPreview, new GridLength(1, GridUnitType.Star), true)
                );
            }

            public Task ProcessFile(Maybe<string> maybeFilePath)
            {
                ItemsSource = maybeFilePath.HasValue ? ToDataTable(maybeFilePath.Value, this) : null;

                RefreshPreview(ItemsSource);

                return Task.CompletedTask;
            }

            public void RefreshPreview(DataTable itemsSource)
            {
                _dataGrid.ItemsSource = itemsSource?.DefaultView;
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
                        tbl.Columns.Add(hasHeader ? firstRowCell.Text : $"Column_{firstRowCell.Start.Column}");
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

        }

        [Heading("Target Options")]
        public class TargetOptions : ImportTargetSelectorScreen, IStepsScreen, IOwnerViewLocator
        {
            private readonly SourceOptions _sourceOptions;

            public TargetOptions(SourceOptions sourceOptions)
            {
                _sourceOptions = sourceOptions;
            }

            public bool CanContentScroll => true;

            public bool HasNext => true;

            public Task<object> Next()
            {
                var viewModel = IoC.Get<DocumentMapperViewModel>();

                IEnumerable<string> sourceFields = null;

                if (_sourceOptions.ItemsSource != null)
                {
                    sourceFields = _sourceOptions.ItemsSource.Columns.OfType<DataColumn>().Select(p => p.ColumnName);
                }

                var collectionReference = GetTargetCollection();

                viewModel.Init(sourceFields, collectionReference);

                return Task.FromResult<object>(viewModel);
            }

            public bool Validate()
            {
                return ModelState.Validate(this);
            }

            public UIElement GetOwnView(object context)
            {
                return new DynamicFormView(this);
            }
        }
    }
}