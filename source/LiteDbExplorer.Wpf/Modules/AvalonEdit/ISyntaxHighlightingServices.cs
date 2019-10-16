using ICSharpCode.AvalonEdit.Highlighting;

namespace LiteDbExplorer.Wpf.Modules.AvalonEdit
{
    public interface ISyntaxHighlightingServices
    {
        IHighlightingDefinition LoadDefinitionFromName(string name, string theme);
        IHighlightingDefinition LoadDefinitionFromExtension(string fileExtension, string theme);
    }
}