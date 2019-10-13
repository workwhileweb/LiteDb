using CSharpFunctionalExtensions;
using ICSharpCode.AvalonEdit.Highlighting;

namespace LiteDbExplorer.Wpf.Modules.AvalonEdit
{
    [ExportHighlightingDefinition(Name, 0)]
    public class LogHighlightingProvider : IHighlightingProvider
    {
        public const string Name = @"Log";

        public IHighlightingDefinition LoadDefinition(Maybe<string> theme)
        {
            return HighlightingHelper.LoadHighlightingFromAssembly(typeof(HighlightingHelper).Assembly,
                @"LiteDbExplorer.Wpf.Modules.AvalonEdit.Highlighting.Log.xshd");
        }
    }
}