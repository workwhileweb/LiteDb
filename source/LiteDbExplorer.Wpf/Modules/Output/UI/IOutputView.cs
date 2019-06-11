namespace LiteDbExplorer.Wpf.Modules.Output.UI
{
    public interface IOutputView
    {
        void Clear();
        void ScrollToEnd();
        void AppendText(string text);
        void SetText(string text);
    }
}