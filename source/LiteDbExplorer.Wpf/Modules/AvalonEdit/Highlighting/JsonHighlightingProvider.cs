using System;
using CSharpFunctionalExtensions;
using ICSharpCode.AvalonEdit.Highlighting;

namespace LiteDbExplorer.Wpf.Modules.AvalonEdit
{
    [ExportHighlightingDefinition(Name, 0, ".json")]
    public class JsonHighlightingProvider : IHighlightingProvider
    {
        public const string Name = @"Json";

        public const string ThemeDark = @"dark";

        public IHighlightingDefinition LoadDefinition(Maybe<string> theme)
        {
            var resourceName = @"LiteDbExplorer.Wpf.Modules.AvalonEdit.Highlighting.Json.xshd";

            if (theme.HasValue && theme.Value.Equals(ThemeDark, StringComparison.OrdinalIgnoreCase))
            {
                resourceName = resourceName.Replace(@".xshd", @".dark.xshd");
            }

            return HighlightingHelper.LoadHighlightingFromAssembly(typeof(HighlightingHelper).Assembly, resourceName);
        }
    }
}