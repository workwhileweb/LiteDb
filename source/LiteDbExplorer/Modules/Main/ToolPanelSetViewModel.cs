using System.ComponentModel.Composition;
using Caliburn.Micro;
using LiteDbExplorer.Modules.Shared;
using LiteDbExplorer.Wpf.Framework.Shell;

namespace LiteDbExplorer.Modules.Main
{
    [Export(typeof(IToolPanelSet))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class ToolPanelSetViewModel : Conductor<IToolPanel>.Collection.OneActive, IToolPanelSet
    {
        [ImportingConstructor]
        public ToolPanelSetViewModel()
        {
        }
    }
}