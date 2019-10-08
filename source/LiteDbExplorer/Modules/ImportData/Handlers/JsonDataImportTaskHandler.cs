using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using Forge.Forms;
using Forge.Forms.Annotations;
using LiteDbExplorer.Modules.Shared;

namespace LiteDbExplorer.Modules.ImportData.Handlers
{
    [Form(Mode = DefaultFields.None)]
    [Export(typeof(IDataImportTaskHandler))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class JsonDataImportTaskHandler : Screen, IDataImportTaskHandler, IStepsScreen, IOwnerViewLocator
    {
        private readonly SourceOptions _sourceOptions;
        private readonly TargetOptions _targetOptions;

        [ImportingConstructor]
        public JsonDataImportTaskHandler(Lazy<IApplicationInteraction> lazyApplicationInteraction)
        {
            DisplayName = "Import JSON Options";

            _sourceOptions = new SourceOptions(lazyApplicationInteraction);
            _targetOptions = new TargetOptions();
        }

        public string HandlerDisplayName => "JSON";

        public int HandlerDisplayOrder => 10;

        [Field]
        public object SourceOptionsContext => _sourceOptions;

        [Field]
        public object TargetOptionsContext => _targetOptions;

        public bool HasNext => true;

        public Task<object> Next()
        {
            var viewModel = IoC.Get<DocumentMapperViewModel>();
            return Task.FromResult<object>(viewModel);
        }

        public bool Validate()
        {
            return ModelState.Validate(SourceOptionsContext) && ModelState.Validate(TargetOptionsContext);
        }

        public UIElement GetOwnView(object context)
        {
            return new DynamicFormStackView(SourceOptionsContext, TargetOptionsContext);
        }

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

    }
}