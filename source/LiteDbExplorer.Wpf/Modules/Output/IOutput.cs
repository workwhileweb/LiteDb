using System.IO;
using LiteDbExplorer.Wpf.Framework;
using LiteDbExplorer.Wpf.Framework.Shell;

namespace LiteDbExplorer.Wpf.Modules.Output
{
    public interface IOutput : IToolPanel
    {
        TextWriter Writer { get; }
        void AppendLine(string text);
        void Append(string text);
        void Clear();
    }
}