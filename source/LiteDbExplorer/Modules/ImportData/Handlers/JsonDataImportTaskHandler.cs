using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
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
    public class JsonDataImportTaskHandler : Screen, IDataImportTaskHandler, IStepsScreen, IOwnerViewLocator
    {
        private readonly SourceOptions _sourceOptions;

        [ImportingConstructor]
        public JsonDataImportTaskHandler(Lazy<IApplicationInteraction> lazyApplicationInteraction)
        {
            DisplayName = "Import JSON Options";

            _sourceOptions = new SourceOptions(lazyApplicationInteraction);
        }

        public string HandlerDisplayName => "JSON";

        public int HandlerDisplayOrder => 10;

        [Field]
        public object SourceOptionsContext => _sourceOptions;

        public bool CanContentScroll => true;

        public bool HasNext => true;

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
            private ExtendedTextEditor _textEditor;
            

            public SourceOptions(Lazy<IApplicationInteraction> lazyApplicationInteraction) : base(lazyApplicationInteraction)
            {
                _textEditor = new ExtendedTextEditor
                {
                    SyntaxHighlightingName = @"json",
                    ShowLineNumbers = true,
                    IsReadOnly = true
                };
            }

            public bool CanContentScroll => false;

            public bool HasNext => true;

            public object DataPreview => _textEditor;

            public string JsonContent { get; private set; }

            protected override (string title, string filter) GetFileFilter()
            {
                return ("Open JSON File", "Json File|*.json");
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

            private Task ProcessFile(Maybe<string> maybeFilePath)
            {
                JsonContent = maybeFilePath.HasValue ? File.ReadAllText(maybeFilePath.Value) : null;

                _textEditor.Document.Text = JsonContent ?? string.Empty;

                return Task.CompletedTask;
            }
        }

        [Heading("Target Options")]
        public class TargetOptions : ImportTargetSelectorScreen
        {

            private SourceOptions _sourceOptions;

            public TargetOptions(SourceOptions sourceOptions)
            {
                _sourceOptions = sourceOptions;
            }

            /*private readonly DocumentMapperViewModel _documentMapper;

            public TargetOptions()
            {
                _documentMapper = IoC.Get<DocumentMapperViewModel>();
            }

            [Field]
            [DirectContent]
            public ViewModelContentProxy Map => new ViewModelContentProxy(_documentMapper);*/
            
        }

    }
}