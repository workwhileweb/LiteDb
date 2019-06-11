using Serilog.Events;

namespace LiteDbExplorer.Wpf.Modules.Output
{
    public interface IOutputLogFilter
    {
        void InvalidateCache();
        bool Filter(LogEvent logEvent);
    }
}