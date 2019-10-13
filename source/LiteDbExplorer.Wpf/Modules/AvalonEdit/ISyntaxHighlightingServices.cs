using ICSharpCode.AvalonEdit.Highlighting;

namespace LiteDbExplorer.Wpf.Modules.AvalonEdit
{
    public interface ISyntaxHighlightingServices
    {
        IHighlightingDefinition LoadDefinition(string name, string theme);
    }
}