using System.ComponentModel.Composition;
using System.Windows.Input;
using Caliburn.Micro;
using LiteDbExplorer.Framework;
using LiteDbExplorer.Modules.Shared;
using LiteDbExplorer.Wpf.Framework.Shell;

namespace LiteDbExplorer.Modules.Main
{
    [Export(typeof(IToolPanelSet))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class ToolPanelSetViewModel : Conductor<IToolPanel>.Collection.OneActive, IToolPanelSet
    {
        private readonly IEventAggregator _eventAggregator;

        [ImportingConstructor]
        public ToolPanelSetViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;

            CloseRequestCommand = new RelayCommand(_ => ClosePanel());
        }

        public ICommand CloseRequestCommand { get; }

        public void ClosePanel()
        {
            _eventAggregator.BeginPublishOnUIThread(new ToolSetPanelActionRequest(ToolSetPanelAction.Close));
        }
    }

    public enum ToolSetPanelAction
    {
        Open,
        Close
    }

    public class ToolSetPanelActionRequest
    {
        public ToolSetPanelActionRequest(ToolSetPanelAction action)
        {
            Action = action;
        }

        public ToolSetPanelAction Action { get; }
    }
}