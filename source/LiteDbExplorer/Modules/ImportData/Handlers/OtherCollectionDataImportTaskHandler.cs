using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows;
using Caliburn.Micro;
using Forge.Forms;
using Forge.Forms.Annotations;
using LiteDbExplorer.Core;
using LiteDbExplorer.Modules.Shared;

namespace LiteDbExplorer.Modules.ImportData.Handlers
{
    [Form(Mode = DefaultFields.None)]
    [Export(typeof(IDataImportTaskHandler))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class OtherCollectionDataImportTaskHandler : Screen, IDataImportTaskHandler, IStepsScreen, IOwnerViewLocator
    {
        private readonly SourceOptions _sourceOptions;
        private readonly TargetOptions _targetOptions;

        [ImportingConstructor]
        public OtherCollectionDataImportTaskHandler()
        {
            DisplayName = "Import Another Collection Options";

            _sourceOptions = new SourceOptions(Store.Current.Databases);
            _targetOptions = new TargetOptions();
        }

        public string HandlerDisplayName => "Another Collection";

        public int HandlerDisplayOrder => 100;

        [Field]
        public object SourceOptionsContext => _sourceOptions;

        [Field]
        public object TargetOptionsContext => _targetOptions;

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
            return new DynamicFormStackView(SourceOptionsContext, TargetOptionsContext);
        }

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
}