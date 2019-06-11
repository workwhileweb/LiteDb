using LiteDbExplorer.Wpf.Framework.Shell;

namespace LiteDbExplorer.Modules.Shared
{
    public interface IToolPanelSet
    {
        void ActivateItem(IToolPanel item);
        void DeactivateItem(IToolPanel item, bool close);
    }
}