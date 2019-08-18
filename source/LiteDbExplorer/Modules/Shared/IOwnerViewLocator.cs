using System.Windows;

namespace LiteDbExplorer.Modules.Shared
{
    public interface IOwnerViewLocator
    {
        UIElement GetOwnView(object context);
    }
}