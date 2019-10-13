using CSharpFunctionalExtensions;
using ICSharpCode.AvalonEdit.Highlighting;

namespace LiteDbExplorer.Wpf.Modules.AvalonEdit
{
    public interface IHighlightingProvider
    {
        IHighlightingDefinition LoadDefinition(Maybe<string> theme);
    }
}