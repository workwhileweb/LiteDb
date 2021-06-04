using System;
using CSharpFunctionalExtensions;
using ICSharpCode.AvalonEdit.Highlighting;
using LiteDbExplorer.Wpf.Modules.AvalonEdit;

namespace LiteDbExplorer.Controls.Highlighting
{
    [ExportHighlightingDefinition(Name, 0)]
    public class LiteDbCmdHighlightingProvider : IHighlightingProvider
    {
        public const string Name = @"LiteDbCmd";

        public const string ThemeDark = @"dark";

        public IHighlightingDefinition LoadDefinition(Maybe<string> theme)
        {
            var resourceName = @"LiteDbExplorer.Controls.Highlighting.LiteDbCmd.xshd";

            if (theme.HasValue && theme.Value.Equals(ThemeDark, StringComparison.OrdinalIgnoreCase))
            {
                resourceName = resourceName.Replace(@".xshd", @".dark.xshd");
            }

            return HighlightingHelper.LoadHighlightingFromAssembly(typeof(LiteDbCmdHighlightingProvider).Assembly, resourceName);
        }
    }
}