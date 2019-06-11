namespace LiteDbExplorer.Wpf.Framework.Shell
{
    public interface IToolPanel : ILayoutItem
    {
        double PreferredHeight { get; }

        bool IsVisible { get; set; }
    }
}