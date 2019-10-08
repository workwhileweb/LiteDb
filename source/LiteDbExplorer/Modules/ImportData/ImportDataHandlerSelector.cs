using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using Forge.Forms;
using Forge.Forms.Annotations;
using LiteDbExplorer.Modules.Shared;

namespace LiteDbExplorer.Modules.ImportData
{
    [Export(typeof(ImportDataHandlerSelector))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    [Form(Mode = DefaultFields.None)]
    public class ImportDataHandlerSelector : Screen, IStepsScreen, IOwnerViewLocator
    {
        public ImportDataHandlerSelector()
        {
            DataImportHandlers = IoC.GetAll<IDataImportTaskHandler>()
                .OrderBy(p => p.HandlerDisplayOrder)
                .ThenBy(p => p.HandlerDisplayName);
        }

        public IEnumerable<IDataImportTaskHandler> DataImportHandlers { get; }

        [Field]
        [Value(Must.NotBeEmpty)]
        [SelectFrom("{Binding DataImportHandlers}", SelectionType = SelectionType.RadioButtons, DisplayPath = nameof(IDataImportTaskHandler.HandlerDisplayName))]
        public IDataImportTaskHandler ImportFormat { get; set; }

        public bool HasNext => ImportFormat != null;

        public override string DisplayName
        {
            get
            {
                if (ImportFormat == null)
                {
                    return "Import data from source";
                }
                return $"Import data from {ImportFormat.HandlerDisplayName}";
            }
        }

        public Task<object> Next()
        {
            return Task.FromResult<object>(ImportFormat);
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